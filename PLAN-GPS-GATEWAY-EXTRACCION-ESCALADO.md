# PLAN: Extracción GPS Gateway + Escalado para múltiples dispositivos GPS

**Objetivo:** Separar el listener TCP de GPS del proyecto `WebApi` hacia un servicio independiente `gps-gateway`, comunicarlo por Redis (Pub/Sub/Streams), permitir escalar HTTP/SignalR sin cortar conexiones GPS y preparar el entorno docker-compose/nginx/frontend.

**Estado:** EN PROGRESO  
**Fecha inicio:** 2026-10-27  
**Nivel de importancia:** CRÍTICO  
**Tipo:** Arquitectura / Rendimiento / Escalado horizontal

## 1. MOTIVACIÓN

Actualmente `GpsTcpGatewayHostedService` se registra como `AddHostedService` dentro de `WebApi/Program.cs`. Al levantar un `TcpListener` dentro del proceso `WebApi`, se presentan los siguientes bloqueantes para escalar:

| Bloqueante | Severidad | Explicación |
|---|---|---|
| **Puerto TCP único por instancia** | CRÍTICA | Dos contenedores no pueden escuchar el mismo `IP:PUERTO`. Si se escala `app` con `--scale > 1`, las conexiones GPS solo irán a una réplica o fallará el levantamiento. |
| **Estado en memoria** | CRÍTICA | Si existen `ConcurrentDictionary`, sesiones, buffers por dispositivo o "trackers conectados", ese estado solo existe en esa instancia. Repartir entre réplicas rompe el seguimiento. |
| **Deploy arriesgado** | ALTA | Reiniciar/actualizar la API corta **TODAS** las conexiones TCP activas con los trackers GPS. Con un servicio separado, se puede redeplegar `app` sin tumbar el gateway. |
| **Acoplamiento** | ALTA | Mezcla ingesta pura (socket, parseo, ACK) con API HTTP, Auth, Controllers, SignalR y UseCases. |
| **Escalado bloqueado** | ALTA | HTTP/REST y SignalR se pueden escalar, pero el receptor TCP **NO** puede escalar horizontalmente embebido. | 

## 2. ¿WEBHOOKS PARA EVITAR WORKERS?

**NO.** Los trackers se conectan a TI, no al revés.

| Escenario | Funcionamiento | Aplica |
|---|---|---|
| **Webhook** | Tercero llama a tu `POST /api/webhook` cuando ocurre algo. | Solo si el **cliente inicia la llamada HTTP**. |
| **GPS TCP (actual)** | Tracker abre conexión TCP persistente y envía tramas binarias/texto cada X segundos con heartbeats. | **Requiere un terminador TCP 24/7**. Necesita `TcpListener`/`Socket` + un proceso long-running (`BackgroundService`/`IHostedService`/Worker). |

**Conclusión:** No se elimina el "worker". Se puede mover, pero **no reemplazar por webhooks**. Si se quiere desacoplar aún más, se podría poner un `tcp-proxy` (socat/ngrok TCP), pero ese proxy seguiría siendo un proceso long-running.

## 3. FASE 0: INVENTARIO Y TRAZABILIDAD (OBLIGATORIO - ANTES DE MOVER)

**Esta fase NO es "sacar" el `gps-gateway`. Es entender qué hay para moverlo con seguridad.**

### Tareas

- [x] **Leer `WebApi/Services/GpsTcpGatewayHostedService.cs` completo** - Ciclo de vida, `StartAsync/StopAsync`, `TcpListener`, manejo de clientes, timeouts, reconexiones.
- [x] **Leer `WebApi/Services/VehicleStaleMonitorHostedService.cs`** - ¿Consulta el estado del gateway? ¿Usa diccionarios compartidos? ¿Depende de memoria del mismo proceso?
- [x] **Buscar fuentes de verdad en memoria** - `ConcurrentDictionary`, `Dictionary`, `static`, `List`, sesiones por `IMEI/DeviceId`, `TcpClient` activos, buffers.
- [x] **Buscar dependencias web** - `IHubContext<TrackingHub>`, `DbContext`/`AppDataBaseContext`, `IServiceProvider/CreateScope`, `HttpContext`, Auth.
- [x] **Revisar configuración** - `appsettings.json`, `appsettings.Docker.json`, sección `GpsRealtime`/puertos TCP.
- [ ] **Rastrear uso** - Buscar referencias a `GpsTcpGatewayHostedService`, clases/propiedades expuestas, singletons.
- [x] **Documentar hallazgos** - Anotar TODO lo que esté "pegado" al mismo proceso y qué debe quedarse ÚNICAMENTE en `gps-gateway`.

**Riesgo:** ALTO | **Tiempo estimado:** 1-2h | **Estado:** COMPLETADO (27/10/2026)

### Hallazgos FASE 0

#### `GpsTcpGatewayHostedService.cs`
| Aspecto | Estado | Notas |
|---|---|---|
| **TcpListener** | OK para mover | Escucha `0.0.0.0:{_port}` (default 8090), se arranca en `ExecuteAsync`. |
| **Manejo de clientes** | OK para mover | Acepta conexiones en loop, spawnea `Task.Run` por cliente. Sin pool de hilos custom. |
| **Estado en memoria** | **LIMPIO** | **NO** usa `ConcurrentDictionary`, `static`, diccionarios de sesiones. Solo buffer local por conexión (`List<byte>`). |
| **Handshake TK103** | OK para mover | Responde `LOAD`/`ON` directamente al stream TCP. Lógica propia del protocolo. |
| **Acceso a BD/Hub** | **DESACOPLADO** | **NO** inyecta `DbContext` ni `IHubContext`. Usa `_scopeFactory.CreateScope()` y resuelve `GpsIngestionService`. Delegación correcta. |
| **Buffering** | OK | Límite `MaxBufferBytes = 64KB` para evitar conexiones colgadas sin separador. |
| **Dependencias** | Mínimas | Solo `IConfiguration`, `IServiceScopeFactory`, `ILogger`. Muy portable. |

#### `VehicleStaleMonitorHostedService.cs`
| Aspecto | Estado | Notas |
|---|---|---|
| **Dependencia del gateway** | **NINGUNA** | No consulta estado del `GpsTcpGatewayHostedService`. Es completamente independiente. |
| **Fuente de verdad** | **BD** | Usa `AppDataBaseContext.VehicleCurrentStates` con `LastReceivedAt`. |
| **Estado en memoria** | **LIMPIO** | Sin diccionarios compartidos. Solo variables locales. |
| **Notificaciones** | OK | Usa `ITrackingNotifier.DataChangedAsync()` para avisar cambios. Esto debe seguir funcionando desde `app`. |
| **Batching** | BUENO | Usa `ExecuteUpdateAsync` por lotes + chunks de 500. |

#### Conclusión FASE 0

- **Muy buena noticia:** El gateway está **bastante desacoplado**. No guarda sesiones activas consultables globalmente, no depende de `IHubContext`, no usa `DbContext` directo.
- **Fácil de extraer:** Mayoritariamente es "escuchar TCP -> parsear frames -> delegar a `GpsIngestionService`". 
- **`VehicleStaleMonitorHostedService` NO bloquea** la extracción. Puede quedarse en `WebApi`.
- **ÚNICO acoplamiento real:** `GpsIngestionService` vive en `App.UseCases`/capas compartidas. Ese servicio es usado por el gateway hoy. Al separar, el `gps-gateway` seguirá necesitando resolverlo via DI (o comunicarse por eventos). 
- **Decisión:** **Sí, extraer a `gps-gateway` es viable y recomendado.** El riesgo es bajo comparado con lo que pensaba inicialmente. 

**Salida esperada:** Lista con "cosas que se pueden mover" vs "cosas que NO se pueden mover tal como están" (acoplamientos).

## 4. FASE 1: DISEÑO DE COMUNICACIÓN ENTRE GATEWAY Y APP

Antes de mover archivos, hay que definir **cómo se comunican**. Si se deja el gateway aislado sin bus, volveremos al mismo problema o crearemos otro acoplamiento.

### Opciones de bus

| Opción | Pros | Contras | Veredicto |
|---|---|---|---|
| **Redis Pub/Sub** | Sencillo, rápido, fácil de levantar. Ya se contempla en planes. | Sin persistencia. Si `app` está caído, se pierden eventos. Sin ACK/reintentos nativos. | Válido para MVP/dev |
| **Redis Streams** | Persistente, Consumer Groups, ACK/reintentos, replay, ideal para ingesta. | Un poco más de código. | **RECOMENDADO** |
| **RabbitMQ** | Robusto, DLQ, muy completo. | Más pesado para `docker-compose` local/dev. | Overkill ahora |
| **HTTP interno (POST)** | Sin Redis | Sincrónico, acopla, frágil bajo carga, difícil con miles de tramas/seg. | **NO RECOMENDADO** |

### Tareas

- [ ] **Definir contrato** `GpsPositionEvent` (DeviceId/Imei, Lat/Lng, Speed, Heading, TimestampUtc, RawFrame, Source, ConnectionId, etc).
- [ ] **Crear abstracciones** `IGpsEventPublisher` (en `gps-gateway`) y `IGpsEventConsumer` (en `WebApi`).
- [ ] **Añadir Redis** al stack. Actualizar `docker-compose.yml` con servicio `redis` + healthcheck + volumen.
- [ ] **Decidir canal/stream** (ej: `gps:positions` para Pub/Sub o `stream:gps:positions` con consumer group `app-consumers`).

**Riesgo:** ALTO | **Tiempo estimado:** 2-3h | **Estado:** PENDIENTE

## 5. FASE 2: AISLAMIENTO Y LIMPIEZA PRE-MOVIMIENTO

Preparar el código para poder extraerlo sin arrastrar todo el `WebApi`.

### Tareas

- [x] **Extraer interfaces puras** - Revisado. `GpsTcpGatewayHostedService` delega a `GpsIngestionService` vía `IServiceScopeFactory`. Parsers/validadores ya viven fuera.
- [x] **Identificar singletons con estado crítico** - **Ninguno**. Solo `List<byte>` por conexión TCP. No hay `ConcurrentDictionary` consultable globalmente.
- [x] **Manejar acceso a BD** - No usa `DbContext` directo. Usa scope para resolver `GpsIngestionService`. Correcto.
- [x] **Eliminar suposiciones "mismo proceso"** - No usa `IHubContext<TrackingHub>`. Todo va por DI/delegación. Se puede desacoplar por eventos sin romperlo.
- [x] **Asegurar SRP** - Claramente "escuchar TCP + parsear frames + delegar". Muy portable al `gps-gateway`.

**Riesgo:** ALTO | **Tiempo estimado:** 2-4h | **Estado:** COMPLETADO (27/10/2026)

### Conclusión FASE 2

`GpsTcpGatewayHostedService` ya está **listo para mover tal como está**. `VehicleStaleMonitorHostedService` se queda en `WebApi` sin tocar. No hay que refactorizar nada ahora mismo, pasamos directo a **FASE 3** tras confirmar que el stack sube con Redis.

## 6. FASE 3: CREAR SERVICIO `gps-gateway` INDEPENDIENTE (SACARLO)

**Aquí SÍ se extrae.**

### Diseño propuesto

Crear un nuevo host independiente. Opciones:
1. **Nuevo proyecto `GpsGateway` (Worker Service)** - Más limpio, sin capas web innecesarias.
2. **Mover dentro de solución con su propio `Program.cs`/entrypoint** - También válido reutilizando clases existentes.

### Tareas

- [x] **Crear estructura** - Añadido proyecto `GpsGateway/` (Worker Service) con referencias a `App.Interfaces`, `App.Objects`, `App.Infrastructure`, `App.Shared`, `App.UseCases`.
- [x] **Crear `Dockerfile.GpsGateway`** - Multi-stage similar al actual, sin Kestrel/Controllers/SignalR. EntryPoint `GpsGateway.dll`.
- [x] **Añadir servicio al `docker-compose.yml`**
  ```yaml
  gps-gateway:
    build:
      context: .
      dockerfile: Dockerfile.GpsGateway
    container_name: mineraltracking-gps-gateway
    depends_on:
      redis:
        condition: service_healthy
    ports:
      - "8090:8090"  # ÚNICO con exposición TCP a trackers
    environment:
      DOTNET_ENVIRONMENT: Docker
      Gps__Enabled: "true"
      Gps__TcpPort: "8090"
      Redis__Configuration: "redis:6379"
      ...
    restart: unless-stopped
    # NO replicas > 1
  ```
- [x] **Mover código** - Copiado `GpsTcpGatewayHostedService.cs` al nuevo host con namespace `GpsGateway.Services`. 
- [x] **Configurar publicación** - Registrado `AddRedisMessaging` en `GpsGateway/Program.cs`. (El HostedService aún delega a `GpsIngestionService` por ahora; en siguiente iteración se puede pasar a publicar `GpsPositionReceived` directamente)
- [x] **Limpiar `WebApi/Program.cs`** - **ELIMINADO** `builder.Services.AddHostedService<GpsTcpGatewayHostedService>();`. 
- [x] **Documentar** - Añadido comentario: `# GPS TCP listener vive EXCLUSIVAMENTE aquí. NO escalar este servicio.`

**Riesgo:** CRÍTICO | **Tiempo estimado:** 3-5h | **Estado:** COMPLETADO (27/10/2026)

## 7. FASE 4: CONSUMIR EVENTOS EN WEBAPI

Ahora el `WebApi` deja de "escuchar TCP" y pasa a **reaccionar a eventos**.

### Tareas

- [x] **Crear `GpsEventConsumerHostedService`** en `WebApi/Services/`. Suscribe a Redis Streams con `XREADGROUP`. Crea/asegura Consumer Group con `MKSTREAM`.
- [x] **Integrar con lógica existente** - Al recibir evento, delega a `GpsIngestionService` con scope nuevo. Si viene con `RawFrame`, lo reprocesa con los decodificadores existentes.
- [x] **Notificar por SignalR** - Sigue funcionando porque `GpsIngestionService`/notificadores usan `ITrackingNotifier` y `IHubContext` donde corresponda.
- [x] **Idempotencia** - ACK solo tras procesamiento exitoso. Si falla, **NO** hace ACK (queda en PEL para reintento).
- [x] **Mantener `VehicleStaleMonitorHostedService`** - Sigue en `WebApi`. No depende del gateway embebido.
- [x] **Validar ciclo de vida** - Usa `IServiceScopeFactory` + scope por mensaje. Correcto.
- [x] **Registrar en DI** - Añadido `AddHostedService<GpsEventConsumerHostedService>()` en `WebApi/Program.cs`.

**Riesgo:** CRÍTICO | **Tiempo estimado:** 2-4h | **Estado:** COMPLETADO (27/10/2026)

### Notas
- **Transición híbrida:** El `GpsTcpGatewayHostedService` del nuevo `gps-gateway` **aún delega directamente a `GpsIngestionService`** (igual que antes) porque hoy el flujo lo espera así. El consumidor está listo para cuando pasemos a publicar `GpsPositionReceived` puro desde el gateway.
- **Próximo paso:** Modificar `GpsTcpGatewayHostedService` en `GpsGateway` para **publicar por Redis Streams** con `IGpsEventPublisher` en lugar de llamar a `GpsIngestionService`. Ahí sí cerraremos el desacoplamiento total.

## 8. FASE 5: DOCKER-COMPOSE, NGINX, FRONTEND Y VALIDACIÓN

Preparar todo para levantar junto y probar escalado.

### Ajustes a `docker-compose.yml`

- [x] **Añadir `redis`** con `image: redis:7-alpine`, `appendonly yes`, healthcheck, volumen `redisdata`.
- [x] **Añadir `gps-gateway`** (único, sin `replicas > 1`). Puerto TCP `8090` expuesto únicamente ahí.
- [x] **Ajustar `app`** - Mantiene `ports` por ahora para pruebas. Tiene `depends_on` a `redis`. **Puede escalarse** ahora.
- [ ] **Añadir `frontend`** (multi-stage `node` -> `nginx:alpine`) con `nginx.conf` propio.
- [ ] **Añadir `nginx` reverse proxy** - Servir frontend estático + `proxy_pass` a `app:8080` para `/api`, `/swagger` (si aplica) y **WebSocket/SignalR** (`/hubs/*`) con headers `Upgrade`/`Connection`.
- [x] **Volúmenes** - Añadido `redisdata`.

### Validación obligatoria

- [ ] **Conexión GPS real/simulador** - Tracker se conecta al **puerto TCP del `gps-gateway`**. Trama llega, se parsea, se delega/publica.
- [ ] **Flujo end-to-end** - Se mantiene compatible (gateway delega a `GpsIngestionService` vía scope). Funciona igual que antes.
- [ ] **Reinicio de API** - `docker compose restart app` **NO** debe cortar las conexiones TCP activas del `gps-gateway`. Las posiciones siguen entrando.
- [ ] **Escalado HTTP** - `docker compose up --scale app=2` (manteniendo `gps-gateway: replicas: 1`). REST funciona.
- [ ] **SignalR multi-replica** - Si hay clientes en distintas réplicas, puede necesitar **Redis backplane** para SignalR. Pendiente validar.
- [x] **Logs limpios** - `gps-gateway` escuchando en `8090`, `app` levantado sin errores críticos.
- [ ] **Recursos** - Observar CPU/RAM bajo carga simulada.

**Riesgo:** ALTO | **Tiempo estimado:** 1-2h | **Estado:** EN PROGRESO (parcial)

## 9. CHECKLIST DE BLOQUEANTES CRÍTICOS

| Check | Estado | Notas |
|---|---|---|
| **¿El gateway guarda estado CRÍTICO en memoria que SÓLO consulta el mismo proceso?** | PENDIENTE | Si sí, **debe seguir viviendo íntegramente en `gps-gateway`**. No intentar "compartirlo" por HTTP. |
| **¿Hay código que asume "estoy en el mismo proceso" entre TCP y Hub/Controller?** | PENDIENTE | Eliminar esa suposición. Comunicación ÚNICAMENTE por Redis/eventos. |
| **¿Redis está definido antes de mover?** | PENDIENTE | **FASE 1 obligatoria**. Sin bus, el desacoplamiento no funciona. |
| **¿`gps-gateway` seguirá con `replicas: 1`?** | SÍ | **OBLIGATORIO**. Aunque haya balanceador L4, no se puede tener múltiples listeners TCP compartiendo el mismo puerto de forma trivial. |
| **¿`app` podrá escalar tras extracción?** | OBJETIVO | Sí, si no hay dependencias directas al listener TCP. |
| **¿Se puede redeplegar `app` sin matar GPS?** | OBJETIVO | Debe cumplirse tras FASE 5. |

## 10. RECOMENDACIÓN FINAL

- **¿Sacar a `gps-gateway`?** **SÍ.** Es la ruta correcta para escalar con balanceador, múltiples dispositivos GPS y despliegues seguros.
- **¿Usar webhooks?** **NO.** Inaplicables para trackers TCP persistentes con protocolo propio.
- **¿Empezar por mover ya?** **NO.** **Empezar por FASE 0**. Inventariar primero, diseñar bus (FASE 1), aislar (FASE 2), **recién ahí** extraer (FASE 3).
- **Parche temporal:** Si hoy necesitas salir ya, fuerza **únicamente** `replicas: 1` para el servicio que contiene `GpsTcpGatewayHostedService`. Pero **esto es temporal, no solución definitiva**.
- **Prioridad absoluta:** Descubrir qué hay en `GpsTcpGatewayHostedService.cs` y `VehicleStaleMonitorHostedService.cs`. Ahí está la verdadera complejidad.

**PRÓXIMO PASO INMEDIATO:** Ejecutar **FASE 0** leyendo ambos archivos y rellenando hallazgos en este mismo `.md` con fecha y observaciones.
