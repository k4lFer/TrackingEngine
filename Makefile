# ============================================================
#  Mineral Tracking - backend_model
#  Requiere: Docker Desktop + GNU make (choco install make en Windows)
# ============================================================

.PHONY: help up down build logs app-log db status front front-install front-lint front-build migration web

help: ## Lista los objetivos disponibles
	@echo "Objetivos disponibles:"
	@grep -E '^[a-zA-Z0-9_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-20s\033[0m %s\n", $$1, $$2}'

up: ## Levanta la pila backend (postgres + valhalla + app) recompilando la app
	docker compose up -d --build

down: ## Detiene la pila backend (sin borrar volúmenes)
	docker compose down

status: ## Estado de los contenedores
	docker compose ps

build: ## Reconstruye solo la imagen de la app
	docker compose up -d --build app

logs: ## Logs en vivo de toda la pila
	docker compose logs -f

app-log: ## Logs en vivo de la app
	docker compose logs -f app

db: ## Consola PostgreSQL dentro del contenedor
	docker compose exec postgres psql -U postgres -d mineral_tracking

front: ## Servidor de desarrollo del frontend (http://localhost:5173)
	cd frontend && npm run dev

front-install: ## Instala dependencias del frontend
	cd frontend && npm install

front-lint: ## Lint del frontend
	cd frontend && npm run lint

front-build: ## Compila el frontend para producción
	cd frontend && npm run build

migration: ## Crea una migración EF nueva: make migration NAME=NombreDeLaMigracion
	dotnet ef migrations add $(NAME) --project App.Infrastructure --startup-project WebApi

valhalla-rebuild: ## Reconstruye los tiles de Valhalla (PBF Perú + elevación)
	@echo "Pasos:"
	@echo "  1) En docker-compose.yml cambia use_tiles_ignore_pbf a \"False\" y force_rebuild a \"True\"."
	@echo "  2) Ejecuta:  make up"
	@echo "  3) Espera que termine:  docker logs -f mineraltracking-valhalla"
	@echo "  4) Devuelve use_tiles_ignore_pbf a \"True\" y force_rebuild a \"False\"."