# 🚀 DESPLIEGUE API + MinIO - PASO A PASO

Esta guía te llevará de 0 a tener tu API funcionando en DigitalOcean.

---

## ✅ ANTES DE EMPEZAR - Checklist

- [ ] Tienes cuenta en DigitalOcean con créditos
- [ ] Sabes la IP de tu servidor Railway (ya la tienes: interchange.proxy.rlwy.net)
- [ ] Tienes este proyecto descargado en tu PC

---

## 📍 PASO 1: Crear archivo .env

**En tu PC**, abre el archivo `.env.example` y crea un nuevo archivo `.env` con este contenido:

```env
# Base de datos Railway (NO CAMBIAR - ya funciona)
DATABASE_HOST=interchange.proxy.rlwy.net
DATABASE_PORT=49952
DATABASE_NAME=railway
DATABASE_USER=root
DATABASE_PASSWORD=aRcZJTlisNyoJXrKRLmaQceAwtQgGfiw

# JWT Configuration
JWT_SECRET=Micontraseñasupersecreta
JWT_ISSUER=https://nelson.com
JWT_AUDIENCE=https://nelson.com

# MinIO Configuration
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
```

**Ubicación del archivo**: `C:\Users\Nelson García\Downloads\nelson-master (1)\nelson-master\.env`

---

## 📍 PASO 2: Crear Droplet en DigitalOcean

1. Ve a: https://cloud.digitalocean.com/droplets/new

2. **Configuración del servidor:**
   - ✅ **Choose an image**: `Ubuntu 24.04 (LTS) x64`
   - ✅ **Choose Size**:
     - Click en "Regular"
     - Selecciona: **$12/mo** (2 GB RAM / 1 CPU / 50 GB SSD)
   - ✅ **Choose a datacenter region**:
     - Selecciona el más cercano a ti (ej: `New York` o `San Francisco`)
   - ✅ **Authentication**:
     - Selecciona "Password"
     - Crea una contraseña FUERTE (anótala bien)
   - ✅ **Hostname**: `nelson-api`

3. Click en **Create Droplet**

4. ⏰ **Espera 1-2 minutos** hasta que aparezca la IP

5. 📝 **ANOTA LA IP** que aparece (ej: `164.90.XXX.XXX`)

---

## 📍 PASO 3: Conectarte al servidor

### Opción A: Desde Windows PowerShell

1. Abre **PowerShell** (botón derecho en inicio → Windows PowerShell)

2. Conéctate al servidor:
```powershell
ssh root@TU_IP_AQUI
```

Ejemplo:
```powershell
ssh root@164.90.123.45
```

3. Escribe `yes` cuando pregunte si confías en el servidor

4. Ingresa la contraseña que creaste en el paso 2

✅ **Si ves algo como `root@nelson-api:~#` significa que estás dentro del servidor**

---

## 📍 PASO 4: Instalar Docker en el servidor

**Copia y pega estos comandos UNO POR UNO** en el servidor:

```bash
# 1. Actualizar el sistema
apt update && apt upgrade -y
```
⏰ Espera 1-2 minutos

```bash
# 2. Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
```
⏰ Espera 30 segundos

```bash
# 3. Instalar Docker Compose
apt install docker-compose -y
```
⏰ Espera 30 segundos

```bash
# 4. Verificar que se instaló correctamente
docker --version
docker-compose --version
```

✅ **Deberías ver algo como:**
```
Docker version 24.x.x
docker-compose version 1.29.x
```

---

## 📍 PASO 5: Subir tu código al servidor

### Opción A: Con Git (si tu código está en GitHub)

En el servidor:
```bash
cd /opt
git clone <URL_DE_TU_REPOSITORIO> nelson
cd nelson
```

### Opción B: Subir manualmente desde tu PC

1. **En tu PC**, abre otra PowerShell (deja la conexión SSH abierta)

2. Ve a la carpeta del proyecto:
```powershell
cd "C:\Users\Nelson García\Downloads\nelson-master (1)\nelson-master"
```

3. Sube todo al servidor:
```powershell
scp -r * root@TU_IP:/opt/nelson/
```

Ejemplo:
```powershell
scp -r * root@164.90.123.45:/opt/nelson/
```

⏰ **Esto tomará 2-5 minutos** dependiendo de tu internet

✅ Cuando termine, verás algo como: `Api.csproj 100%`

---

## 📍 PASO 6: Configurar variables de entorno en el servidor

**Vuelve a la PowerShell donde estás conectado al servidor** y ejecuta:

```bash
cd /opt/nelson
```

```bash
# Crear archivo .env
nano .env
```

Copia y pega esto:
```env
DATABASE_HOST=interchange.proxy.rlwy.net
DATABASE_PORT=49952
DATABASE_NAME=railway
DATABASE_USER=root
DATABASE_PASSWORD=aRcZJTlisNyoJXrKRLmaQceAwtQgGfiw

JWT_SECRET=Micontraseñasupersecreta
JWT_ISSUER=https://nelson.com
JWT_AUDIENCE=https://nelson.com

MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
```

Para guardar:
- Presiona `Ctrl + X`
- Presiona `Y`
- Presiona `Enter`

✅ **Archivo guardado**

---

## 📍 PASO 7: Construir y desplegar API + MinIO

```bash
# Verificar que estás en la carpeta correcta
pwd
```
✅ Deberías ver: `/opt/nelson`

```bash
# Construir las imágenes Docker (primera vez toma 5-10 minutos)
docker-compose build api
```

⏰ **ESPERA 5-10 MINUTOS** - Verás muchas líneas de compilación

✅ Cuando termine verás: `Successfully built` y `Successfully tagged`

```bash
# Iniciar SOLO API y MinIO
docker-compose up -d minio api
```

⏰ Espera 30 segundos

```bash
# Verificar que están corriendo
docker-compose ps
```

✅ **Deberías ver algo así:**
```
Name              State    Ports
nelson-api        Up       0.0.0.0:5000->80/tcp
nelson-minio      Up       0.0.0.0:9000->9000/tcp, 0.0.0.0:9001->9001/tcp
```

---

## 📍 PASO 8: Abrir puertos del firewall

```bash
# Permitir tráfico HTTP en los puertos necesarios
ufw allow 22/tcp      # SSH (no cerrar o te quedarás fuera)
ufw allow 5000/tcp    # API
ufw allow 9000/tcp    # MinIO
ufw allow 9001/tcp    # MinIO Console
```

```bash
# Activar firewall
ufw enable
```

Escribe `y` y presiona Enter

---

## 📍 PASO 9: Verificar que funciona

### 9.1 Ver logs de la API

```bash
docker-compose logs -f api
```

✅ Deberías ver líneas como:
```
Now listening on: http://[::]:80
Application started. Press Ctrl+C to quit.
```

Presiona `Ctrl + C` para salir de los logs

### 9.2 Probar desde tu navegador

En tu PC, abre Chrome y ve a:
```
http://TU_IP:5000/graphql
```

Ejemplo:
```
http://164.90.123.45:5000/graphql
```

✅ **Deberías ver la interfaz de Banana Cake Pop (GraphQL IDE)**

### 9.3 Probar una query

En la interfaz de GraphQL, escribe:

```graphql
query {
  animales {
    items {
      id
      codigoRfid
      pesoActualLibras
    }
  }
}
```

Click en el botón ▶️ (Run)

✅ **Si ves datos o un array vacío `[]` = TODO FUNCIONA**

---

## 📍 PASO 10: Probar MinIO Console

En Chrome, ve a:
```
http://TU_IP:9001
```

- **Usuario**: `minioadmin`
- **Contraseña**: `minioadmin`

✅ Deberías poder entrar al panel de MinIO

---

## 🎉 ¡LISTO! Tu API está desplegada

### 📱 Para conectar tu app móvil:

**Endpoint GraphQL:**
```
http://TU_IP:5000/graphql
```

**Ejemplo de query para buscar animal por RFID:**
```graphql
query BuscarAnimal($rfid: String!) {
  animal(where: { codigoRfid: { eq: $rfid } }) {
    id
    codigoRfid
    pesoActualLibras
    raza {
      nombre
    }
    estadoAnimal {
      nombre
    }
  }
}
```

**Variables:**
```json
{
  "rfid": "RFID123456"
}
```

---

## 🔧 Comandos útiles

### Ver logs en tiempo real
```bash
docker-compose logs -f api
```

### Reiniciar la API
```bash
docker-compose restart api
```

### Detener todo
```bash
docker-compose down
```

### Volver a iniciar
```bash
docker-compose up -d minio api
```

### Ver uso de recursos
```bash
docker stats
```

---

## ❌ Solución de problemas

### Problema: No puedo acceder a http://IP:5000/graphql

```bash
# Verificar que el contenedor está corriendo
docker ps

# Ver logs de error
docker-compose logs api

# Verificar firewall
ufw status
```

### Problema: Error de base de datos

```bash
# Probar conexión a Railway desde el servidor
apt install mysql-client -y
mysql -h interchange.proxy.rlwy.net -P 49952 -u root -p
# Contraseña: aRcZJTlisNyoJXrKRLmaQceAwtQgGfiw
```

### Problema: MinIO no funciona

```bash
docker-compose restart minio
docker-compose logs minio
```

---

## 📞 Siguiente paso

Una vez que confirmes que la API funciona:
1. ✅ Prueba desde tu app móvil
2. ✅ Registra un animal con RFID
3. ✅ Consulta el animal por RFID
4. ✅ Sube una imagen de prueba

Cuando todo esté funcionando, podemos desplegar el BackOffice también.
