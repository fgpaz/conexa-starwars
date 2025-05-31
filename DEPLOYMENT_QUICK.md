# 🚀 Deployment Rápido - Conexa Star Wars API

## 🎯 **Opciones Gratuitas Recomendadas**

### 1. **Railway** ⭐ (MÁS FÁCIL)

1. **Preparar repositorio en GitHub**:
   ```bash
   git add .
   git commit -m "Ready for deployment"
   git push origin main
   ```

2. **Desplegar en Railway**:
   - Ve a [railway.app](https://railway.app)
   - Conecta tu cuenta de GitHub
   - Clic en "Deploy from GitHub repo"
   - Selecciona el repositorio `conexa-starwars`
   - Railway detectará automáticamente el Dockerfile

3. **Variables de entorno** (se configuran automáticamente):
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ASPNETCORE_URLS=http://+:$PORT`
   - Railway genera automáticamente el JWT secret

4. **URL final**: `https://tu-app.railway.app`

---

### 2. **Render** ⭐ (RECOMENDADO)

1. **Preparar repositorio**:
   ```bash
   git add .
   git commit -m "Ready for Render deployment"
   git push origin main
   ```

2. **Desplegar en Render**:
   - Ve a [render.com](https://render.com)
   - Clic en "New +" → "Web Service"
   - Conecta tu repositorio de GitHub
   - Selecciona "Docker" como environment
   - Dockerfile path: `./Dockerfile`

3. **Variables de entorno**:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:10000
   JwtSettings__SecretKey=TuClaveSecretaMinimo32CaracteresAqui123456789
   JwtSettings__Issuer=ConexaStarWarsAPI
   JwtSettings__Audience=ConexaStarWarsClient
   ```

4. **URL final**: `https://tu-app.onrender.com`

---

### 3. **Fly.io** ⭐ (PARA EXPERTOS)

1. **Instalar Fly CLI**:
   ```bash
   # Windows (PowerShell)
   iwr https://fly.io/install.ps1 -useb | iex
   
   # O descargar desde https://fly.io/docs/getting-started/installing-flyctl/
   ```

2. **Login y deployment**:
   ```bash
   fly auth login
   fly launch --name conexa-starwars-api
   fly deploy
   ```

3. **Configurar secretos**:
   ```bash
   fly secrets set JwtSettings__SecretKey="TuClaveSecretaMinimo32CaracteresAqui123456789"
   ```

4. **URL final**: `https://conexa-starwars-api.fly.dev`

---

## 🔑 **Usuarios de Prueba**

Tu API ya incluye usuarios por defecto para las pruebas:

### Administrador
- **Email**: `admin@conexa.com`
- **Password**: `Admin123!`
- **Roles**: Puede crear, actualizar y eliminar películas

### Usuario Regular
- **Email**: `user@conexa.com`
- **Password**: `User123!`
- **Roles**: Solo puede ver películas

---

## 📱 **Endpoints Principales**

### Autenticación
```
POST /api/auth/login
POST /api/auth/register
```

### Películas (requiere autenticación)
```
GET    /api/movies              # Ver todas las películas
GET    /api/movies/{id}         # Ver película específica
POST   /api/movies              # Crear película (solo admin)
PUT    /api/movies/{id}         # Actualizar película (solo admin)
DELETE /api/movies/{id}         # Eliminar película (solo admin)
```

### Sincronización con SWAPI
```
POST /api/movies/sync           # Sincronizar con Star Wars API (solo admin)
```

---

## 🌟 **Características Incluidas**

✅ **Base de datos In-Memory** (no necesita configuración externa)
✅ **Usuarios pre-creados** para pruebas inmediatas
✅ **Autenticación JWT** configurada
✅ **Swagger/OpenAPI** documentación automática
✅ **CORS** habilitado para frontend
✅ **Logging** estructurado
✅ **Health checks**
✅ **Docker** optimizado para cloud

---

## 🚀 **Recomendación Final**

**Para los evaluadores del challenge, recomiendo Railway** porque:
1. ⚡ **Setup más rápido** (2 minutos)
2. 🔧 **Zero configuración** de base de datos
3. 🌐 **SSL automático**
4. 📊 **Logs en tiempo real**
5. 💰 **500 horas gratis/mes**

**URL de ejemplo**: Una vez desplegado, los evaluadores podrán acceder a:
- **API**: `https://tu-app.railway.app/api`
- **Swagger**: `https://tu-app.railway.app/swagger`
- **Health**: `https://tu-app.railway.app/health`

---

## 📞 **Soporte**

Si tienes problemas con el deployment, revisa:
1. Los logs de la plataforma elegida
2. Que el puerto sea correcto (8080)
3. Variables de entorno configuradas
4. Dockerfile sin errores de sintaxis 