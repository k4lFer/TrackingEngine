# DESIGN.md

## Identidad
Control room industrial/minero con tono sobrio, profesional y confiable.

## Paleta
Basada en variables CSS actuales:

| Token | Uso | Valor |
|---|---|---|
| `--bg` / `--color-canvas` | Fondo general | `#f3f5f8` |
| `--bg-elev` / `--color-surface` | Tarjetas/paneles | `#ffffff` |
| `--bg-elev-2` / `--color-surface-2` | Inputs/hover sutil | `#e9edf2` |
| `--border` / `--color-line` | Separadores | `#e1e6ec` |
| `--border-strong` / `--color-line-strong` | Controles | `#c8d1dc` |
| `--text` / `--color-ink` | Texto principal | `#1c2433` |
| `--text-dim` / `--color-ink-dim` | Texto secundario | `#5c6b7d` |
| `--text-faint` / `--color-ink-faint` | Texto terciario | `#8b98a8` |
| `--accent` | Acción primario / marca | `#b9801f` (cobre/mena) |
| `--accent-strong` | Hover primario | `#d2982e` |
| `--ok` | En ruta / OK | `#2f9e6e` |
| `--warn` | Advertencia | `#b8861a` |
| `--alert` | Crítico / sin señal | `#d24a38` |
| `--info` | Info | `#3d83d0` |

## Tipografía
| Fuente | Uso |
|---|---|
| `Space Grotesk` | Títulos, cabeceras, nombres de sección |
| `Inter` | UI general, tablas, formularios |
| `JetBrains Mono` | Datos técnicos (IMEI, coords, IDs) |

## Espaciado / Radios
- `--radius` / `--radius-panel`: `10px` (tarjetas/paneles)
- `--radius-sm` / `--radius-control`: `6px` (botones/inputs)
- Escala Tailwind coherente con gaps de 4/6/8/12/16

## Sombras
Usar sombras muy sutiles. Evitar blur exagerado. Preferir bordes + sutiles eleveaciones.

## Componentes base
- **Layout**: Sidebar + Topbar + contenido scrollable
- **Cards**: header con título uppercase sutil + body
- **DataTable**: header compacto, filas con hover, estados vacíos/carga
- **Buttons**: primario/acento, ghost/outline, danger
- **Badges/Estados**: colores semánticos
- **Forms**: inputs limpios, focus visible
- **Map**: debe tener prioridad visual, paneles flotantes bien legibles

## Reglas de diseño
- **Profesional sobrio**: industrial sin futurismo exagerado
- **Escaneable**: densidad controlada, buen contraste
- **Responsive**: sidebar se adapta, tablas con scroll horizontal, mapa flexible
- **Performance**: minimizar CSS custom, reutilizar clases Tailwind
- **Accesible**: focus states visibles, contraste razonable
- **Consistente**: usar tokens CSS + clases utilitarias
