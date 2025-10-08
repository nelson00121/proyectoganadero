#!/bin/bash

# Script de despliegue para DigitalOcean
# Este script construye y despliega todo el sistema Nelson

echo "🚀 Iniciando despliegue del sistema Nelson..."

# Verificar que existe el archivo .env
if [ ! -f .env ]; then
    echo "⚠️  Archivo .env no encontrado. Copiando desde .env.example..."
    cp .env.example .env
    echo "⚠️  Por favor edita el archivo .env con tus credenciales antes de continuar."
    exit 1
fi

# Cargar variables de entorno
export $(cat .env | xargs)

echo "📦 Construyendo imágenes Docker..."
docker-compose build --no-cache

echo "🔄 Deteniendo contenedores existentes..."
docker-compose down

echo "🚀 Iniciando servicios..."
docker-compose up -d

echo "⏳ Esperando que los servicios estén listos..."
sleep 10

echo "✅ Verificando estado de los servicios..."
docker-compose ps

echo ""
echo "🎉 ¡Despliegue completado!"
echo ""
echo "📍 URLs de acceso:"
echo "   - API GraphQL: http://localhost:5000/graphql"
echo "   - BackOffice: http://localhost:5001"
echo "   - MinIO Console: http://localhost:9001"
echo ""
echo "📊 Para ver logs en tiempo real:"
echo "   docker-compose logs -f"
echo ""
echo "🛑 Para detener los servicios:"
echo "   docker-compose down"
