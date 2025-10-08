# 🚀 Guía de Despliegue - Sistema Nelson

Esta guía te ayudará a desplegar el sistema completo en DigitalOcean paso a paso.

## 📋 Requisitos Previos

1. **Cuenta de DigitalOcean** con créditos disponibles
2. **Docker y Docker Compose** instalados en tu servidor
3. **Git** instalado
4. **Base de datos MySQL** (ya tienes Railway configurado)

---

## 🎯 Paso 1: Crear Droplet en DigitalOcean

### 1.1 Crear el servidor

1. Ve a [DigitalOcean](https://cloud.digitalocean.com)
2. Click en **Create** → **Droplets**
3. Configuración recomendada:
   - **Image**: Ubuntu 24.04 LTS
   - **Plan**: Basic
   - **CPU**: Regular - $12/month (2GB RAM, 1 vCPU)
   - **Datacenter**: Elige el más cercano a tus usuarios
   - **Authentication**: SSH Key (recomendado) o Password
   - **Hostname**: `nelson-server`

4. Click en **Create Droplet**

### 1.2 Conectarte al servidor

```bash
ssh root@TU_IP_DEL_DROPLET
```

---

## 🔧 Paso 2: Instalar Docker en el Droplet

Ejecuta estos comandos en tu servidor:

```bash
# Actualizar el sistema
apt update && apt upgrade -y

# Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Instalar Docker Compose
apt install docker-compose -y

# Verificar instalación
docker --version
docker-compose --version
```

---

## 📦 Paso 3: Clonar el Proyecto

```bash
# Instalar Git si no está instalado
apt install git -y

# Clonar tu repositorio (o subir archivos vía SCP/FTP)
cd /opt
git clone <URL_DE_TU_REPOSITORIO> nelson
cd nelson
```

**Si no tienes Git configurado**, sube los archivos manualmente usando:
```bash
# Desde tu PC local
scp -r "C:\Users\Nelson García\Downloads\nelson-master (1)\nelson-master" root@TU_IP:/opt/nelson
```

---

## ⚙️ Paso 4: Configurar Variables de Entorno

```bash
cd /opt/nelson

# Copiar archivo de ejemplo
cp .env.example .env

# Editar variables de entorno
nano .env
```

### Configuración del archivo `.env`:

```env
# Base de datos (Railway - ya configurada)
DATABASE_HOST=interchange.proxy.rlwy.net
DATABASE_PORT=49952
DATABASE_NAME=railway
DATABASE_USER=root
DATABASE_PASSWORD=aRcZJTlisNyoJXrKRLmaQceAwtQgGfiw

# JWT Configuration (¡Cambia el secret en producción!)
JWT_SECRET=TuClaveSecretaSuperSegura2024!
JWT_ISSUER=https://tu-dominio.com
JWT_AUDIENCE=https://tu-dominio.com

# MinIO Configuration
MINIO_ENDPOINT=minio
MINIO_PORT=9000
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin123  # Cambia esto en producción
MINIO_USE_SSL=false

# Puertos
API_PORT=5000
BACKOFFICE_PORT=5001
```

Guarda el archivo: `Ctrl + X` → `Y` → `Enter`

---

## 🚀 Paso 5: Desplegar con Docker Compose

```bash
# Dar permisos de ejecución al script
chmod +x deploy.sh

# Ejecutar despliegue
./deploy.sh
```

O manualmente:

```bash
# Construir imágenes
docker-compose build

# Iniciar servicios
docker-compose up -d

# Ver logs
docker-compose logs -f
```

---

## 🌐 Paso 6: Configurar Firewall

```bash
# Permitir puertos necesarios
ufw allow 22/tcp      # SSH
ufw allow 80/tcp      # HTTP
ufw allow 443/tcp     # HTTPS
ufw allow 5000/tcp    # API
ufw allow 5001/tcp    # BackOffice
ufw allow 9000/tcp    # MinIO
ufw allow 9001/tcp    # MinIO Console

# Activar firewall
ufw enable
```

---

## 📱 Paso 7: Acceder a los Servicios

Una vez desplegado, puedes acceder a:

- **API GraphQL**: `http://TU_IP:5000/graphql`
- **BackOffice**: `http://TU_IP:5001`
- **MinIO Console**: `http://TU_IP:9001`

### Ejemplo para tu app móvil:

```dart
// Configuración del cliente GraphQL
final link = HttpLink('http://TU_IP:5000/graphql');
```

---

## 🔒 Paso 8: (Opcional) Configurar Dominio y SSL

### 8.1 Configurar Dominio

1. En tu proveedor de dominios, crea registros DNS:
   - `api.tudominio.com` → `TU_IP` (Tipo A)
   - `admin.tudominio.com` → `TU_IP` (Tipo A)

### 8.2 Instalar Nginx como Reverse Proxy

```bash
apt install nginx certbot python3-certbot-nginx -y

# Crear configuración para API
nano /etc/nginx/sites-available/nelson-api
```

Contenido del archivo:
```nginx
server {
    listen 80;
    server_name api.tudominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

```bash
# Activar configuración
ln -s /etc/nginx/sites-available/nelson-api /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx

# Instalar certificado SSL
certbot --nginx -d api.tudominio.com -d admin.tudominio.com
```

---

## 🛠️ Comandos Útiles

### Ver logs de los servicios
```bash
docker-compose logs -f api
docker-compose logs -f backoffice
docker-compose logs -f minio
```

### Reiniciar servicios
```bash
docker-compose restart api
docker-compose restart backoffice
```

### Detener todo
```bash
docker-compose down
```

### Ver servicios activos
```bash
docker-compose ps
```

### Actualizar código
```bash
git pull origin main
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

---

## 🔍 Troubleshooting

### Problema: No puedo conectar a la API
```bash
# Verificar que el contenedor está corriendo
docker ps

# Ver logs del contenedor
docker-compose logs api

# Verificar conectividad
curl http://localhost:5000/graphql
```

### Problema: Error de base de datos
```bash
# Verificar conexión desde el servidor
apt install mysql-client -y
mysql -h interchange.proxy.rlwy.net -P 49952 -u root -p
```

### Problema: MinIO no funciona
```bash
# Reiniciar MinIO
docker-compose restart minio

# Verificar logs
docker-compose logs minio
```

---

## 📊 Monitoreo

### Ver uso de recursos
```bash
docker stats
```

### Ver espacio en disco
```bash
df -h
```

---

## 🎉 ¡Listo!

Tu sistema está desplegado. Para conectar tu app móvil:

**Endpoint GraphQL**: `http://TU_IP:5000/graphql`

O si configuraste dominio: `https://api.tudominio.com/graphql`

---

## 📞 Soporte

Si tienes problemas, revisa los logs:
```bash
docker-compose logs -f
```
