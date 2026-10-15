# Guía de puesta en marcha del entorno de desarrollo

Guía para clonar, configurar y ejecutar **(Centro Odontológico)**
en Windows 10/11. Incluye MySQL en Docker, API ASP.NET Core sobre .NET
10 y frontend React/Vite con Mantine.

**Tiempo estimado:** 20--30 minutos en un equipo con las herramientas
instaladas. La primera instalación puede demorar más.

> **Importante:** seguí los pasos en orden. En particular, configurá los
> secretos **antes del primer arranque de la API** y comprobá que Docker
> tenga iniciado el motor.

## Contenido

1.  [Requisitos](#1-requisitos)
2.  [Clonar el repositorio](#2-clonar-el-repositorio)
3.  [Iniciar MySQL con Docker](#3-iniciar-mysql-con-docker)
4.  [Confiar en el certificado
    HTTPS](#4-confiar-en-el-certificado-https)
5.  [Configurar los secretos
    locales](#5-configurar-los-secretos-locales)
6.  [Restaurar, compilar y ejecutar el
    backend](#6-restaurar-compilar-y-ejecutar-el-backend)
7.  [Instalar y ejecutar el
    frontend](#7-instalar-y-ejecutar-el-frontend)
8.  [Usuarios de prueba](#8-usuarios-de-prueba)
9.  [Rutina de trabajo diaria con
    Git](#9-rutina-de-trabajo-diaria-con-git)
10. [Solución de problemas](#10-solución-de-problemas)
11. [Checklist de verificación](#11-checklist-de-verificación)

------------------------------------------------------------------------

## 1. Requisitos

Instalá las siguientes herramientas antes de empezar.

  --------------------------------------------------------------------------
  Herramienta             Requisito               Verificación
  ----------------------- ----------------------- --------------------------
  Git                     Versión actual          `git --version`

  SDK de .NET             **10.0.x**              `dotnet --list-sdks`

  Docker Desktop          Versión actual, con     `docker --version` y
                          motor iniciado          `docker compose version`

  Node.js                 **20 o superior**,      `node --version`
                          preferentemente LTS     

  npm                     Incluido con Node.js    `npm --version`

  `dotnet-ef`             Compatible con el       `dotnet ef --version`
                          SDK/proyecto            

  Editor                  Visual Studio, Rider o  ---
                          VS Code con C# Dev Kit  
  --------------------------------------------------------------------------

### 1.1 Verificar .NET 10

En PowerShell o Git Bash, ejecutá:

``` bash
dotnet --list-sdks
dotnet --version
```

Tenés que tener instalado el SDK **10.0.x**. Si `dotnet --version`
muestra otra versión, eso no demuestra por sí solo que falte .NET 10:
revisá `dotnet --list-sdks`. Si el SDK 10 no aparece, instalalo desde la
página oficial de .NET de Microsoft y abrí una terminal nueva.

### 1.2 Verificar Docker Desktop

En Windows 10/11, Docker Desktop normalmente requiere **WSL 2** y
virtualización habilitada en BIOS/UEFI. Abrí Docker Desktop y esperá
hasta que indique que el motor está funcionando.

Después ejecutá:

``` bash
docker --version
docker compose version
docker info
```

Si `docker info` falla porque no puede conectarse al motor, todavía no
continúes: abrí Docker Desktop y resolvé ese problema primero.

### 1.3 Instalar `dotnet-ef`

Comprobá primero si está disponible:

``` bash
dotnet ef --version
```

Si el comando no se reconoce, instalalo:

``` bash
dotnet tool install --global dotnet-ef
```

Si ya estaba instalado:

``` bash
dotnet tool update --global dotnet-ef
```

Si la terminal sigue sin reconocer `dotnet ef`, cerrala y volvé a
abrirla. Si el proyecto requiere una versión específica de la
herramienta, usá la versión compatible con la configuración del
repositorio.

------------------------------------------------------------------------

## 2. Clonar el repositorio

Abrí **Git Bash** o **PowerShell** en la carpeta donde guardás tus
proyectos. Por ejemplo, `C:\Dev`.

``` bash
git clone https://github.com/lucianoalicata/CentroOdontologico.git
cd CentroOdontologico
git checkout development
git pull origin development
```

**Trabajamos sobre `development`, no directamente sobre
`main`/\`master.** Para desarrollar una tarea propia, creá una rama como
se explica en la [rutina diaria](#9-rutina-de-trabajo-diaria-con-git).

Comprobá que estás en la raíz del repositorio:

``` bash
git status
```

En esta carpeta deberían encontrarse, entre otros, `Clinica.Api`,
`Clinica.Infraestructura`, `frontend` y el archivo `docker-compose.yml`.

> **Consejo:** los comandos de esta guía se ejecutan desde la raíz del
> repositorio salvo que se indique explícitamente cambiar de carpeta.

------------------------------------------------------------------------

## 3. Iniciar MySQL con Docker

### 3.1 Arrancar el contenedor

Asegurate de que Docker Desktop esté abierto y que el motor esté
funcionando. Desde la raíz del repositorio:

``` bash
docker compose up -d
docker compose ps
```

El servicio de MySQL debería figurar como `Up`/`running` o `healthy`,
según la configuración del contenedor. En la publicación de puertos
deberías ver el puerto local **3306** conectado con el 3306 del
contenedor.

Si el contenedor está arrancando por primera vez, puede tardar unos
segundos. Volvé a consultar:

``` bash
docker compose ps
```

Si no queda en ejecución, consultá los registros:

``` bash
docker compose logs --tail=100
```

### 3.2 Error frecuente: el puerto 3306 ya está ocupado

Si aparece un error similar a:

``` text
Bind for 0.0.0.0:3306 failed: port is already allocated
```

otro programa está usando el puerto 3306. Es común que sea un servicio
de MySQL instalado directamente en Windows.

**Cómo resolverlo:**

1.  Presioná `Win + R`.

2.  Escribí `services.msc` y presioná Enter.

3.  Buscá un servicio llamado `MySQL80`, `MySQL` o similar.

4.  Si confirmás que es tu instalación local de MySQL y no la necesitás
    en ese momento, hacé clic derecho → **Detener**.

5.  Volvé a la terminal, ubicada en la raíz del repositorio, y ejecutá:

    ``` bash
    docker compose up -d
    docker compose ps
    ```

Podés configurar el servicio local para que inicie manualmente, pero
hacelo solo si sabés que no lo necesitás para otros proyectos. **No
desinstales MySQL**: para este proyecto alcanza con liberar el puerto
mientras usás Docker.

> No continúes con el backend hasta que el contenedor de MySQL esté en
> ejecución.

------------------------------------------------------------------------

## 4. Confiar en el certificado HTTPS

La API se ejecuta con el perfil `https` y el frontend espera conectarse
a `https://localhost:7068`.

Ejecutá una vez en PowerShell o Git Bash:

``` bash
dotnet dev-certs https --trust
```

Si Windows muestra una confirmación de seguridad, aceptala. Si el
comando informa que el certificado ya es confiable, podés continuar.

Más adelante, si el navegador muestra un error de certificado, cerrá el
navegador y volvé a abrirlo. Si el problema persiste, ejecutá de nuevo
el comando y revisá el mensaje que devuelve.

------------------------------------------------------------------------

## 5. Configurar los secretos locales

Cada integrante debe configurar los secretos en su propio equipo. **Hacé
este paso antes de iniciar la API por primera vez**, porque el valor de
`Seed:Password` se usa al crear los usuarios iniciales.

### 5.1 Configurar los secretos

Desde la raíz del repositorio:

``` bash
cd Clinica.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "CAMBIAR_POR_UNA_CLAVE_LOCAL_ALEATORIA_DE_32_CARACTERES_O_MAS"
dotnet user-secrets set "Seed:Password" "Dev12345!"
cd ..
```

Reemplazá el texto de ejemplo de `Jwt:SecretKey` por una clave aleatoria
propia de **al menos 32 caracteres**. No uses la frase de ejemplo
literalmente.

`dotnet user-secrets init` se ejecuta una sola vez por proyecto. Si
avisa que ya existe un `UserSecretsId`, no hace falta inicializarlo otra
vez; ejecutá los dos comandos `set`.

En este proyecto, la contraseña de desarrollo prevista para los cuatro
usuarios de prueba es `Dev12345!`. Es una contraseña **solo para
desarrollo local**: no la uses en producción ni en cuentas reales.

### 5.2 Comprobar los secretos

Desde la raíz:

``` bash
dotnet user-secrets list --project Clinica.Api
```

Verificá que aparezcan `Jwt:SecretKey` y `Seed:Password`. Este comando
muestra los valores en pantalla; evitá compartir capturas o registros
que contengan secretos.

### 5.3 Cadena de conexión a MySQL

La configuración del repositorio debe apuntar al MySQL del contenedor
local, normalmente en `localhost:3306`, con la base `odontodb`. Revisá
`docker-compose.yml` y la configuración de la API para confirmar que
servidor, base, usuario y contraseña coincidan.

En la configuración actual documentada para este proyecto,
`appsettings.json` ya contiene la conexión local que coincide con
`docker-compose.yml`, por lo que no debería ser necesario agregar
`ConnectionStrings:DefaultConnection` a User Secrets. Si el repositorio
que clonaste tiene una configuración diferente o la API informa un error
de conexión, compará los valores reales de ambos archivos antes de
cambiar nada.

**No subas contraseñas ni claves reales a Git.**

------------------------------------------------------------------------

## 6. Restaurar, compilar y ejecutar el backend

Asegurate de estar en la **raíz** de `CentroOdontologico`, no dentro de
`Clinica.Api`.

### 6.1 Restaurar, compilar y probar

``` bash
dotnet restore
dotnet build
dotnet test
```

Esperá a que terminen los comandos. Si alguno falla, resolvé ese error
antes de iniciar la API. Prestá atención al primer error real de la
salida; los mensajes siguientes pueden ser consecuencia de ese mismo
problema.

### 6.2 Iniciar la API usando HTTPS

Desde la raíz del repositorio:

``` bash
dotnet run --project Clinica.Api --launch-profile https
```

**No omitas `--launch-profile https`.** Si iniciás con otro perfil, la
API puede usar HTTP o un puerto distinto del que espera el frontend.

Dejá esta terminal abierta. En la salida buscá una línea similar a:

``` text
Now listening on: https://localhost:7068
```

### 6.3 Qué sucede en el primer arranque

En el entorno de desarrollo, la aplicación aplica las migraciones de
Entity Framework Core y prepara la base de datos. Si la base está vacía,
el seeder crea los usuarios de prueba con la contraseña configurada en
`Seed:Password`.

Para verificar que la API responde, abrí en el navegador:

-   `https://localhost:7068/scalar/v1`
-   Si esa ruta no existe en la versión actual, probá
    `https://localhost:7068/scalar`.

Si no abre, revisá primero la terminal del backend y confirmá que esté
escuchando en el puerto 7068. No sigas con el frontend hasta que la API
arranque correctamente.

> **No cierres la terminal del backend** mientras probás el frontend.
> Para detener la API, usá `Ctrl + C`.

------------------------------------------------------------------------

## 7. Instalar y ejecutar el frontend

Dejá el backend en ejecución. Abrí una **segunda terminal** y entrá a la
carpeta del repositorio.

``` bash
cd CentroOdontologico/frontend
npm install
npm run dev
```

Si ya estás dentro de `CentroOdontologico`, alcanza con:

``` bash
cd frontend
npm install
npm run dev
```

Dejá esta terminal abierta. Vite debería iniciar en:

`http://localhost:5173`

Abrí esa dirección en el navegador e intentá iniciar sesión con uno de
los usuarios de prueba.

### Importante sobre el puerto 5173

El backend permite el origen local configurado para el frontend. Si el
puerto 5173 ya está ocupado, Vite puede fallar debido a la configuración
de puerto estricto. Cerrá la otra instancia que lo esté usando y ejecutá
de nuevo `npm run dev`.

### Si falta `frontend/.env.development`

Si la aplicación informa que falta `VITE_API_URL`, comprobá que el
archivo exista y esté versionado en la rama actual:

``` bash
git status
git checkout development -- frontend/.env.development
```

El segundo comando restaura ese archivo desde `development`; usalo solo
si necesitás recuperar la versión del repositorio y no tenés cambios
propios que debas conservar en él. La URL configurada debe coincidir con
la API HTTPS local, normalmente `https://localhost:7068`.

------------------------------------------------------------------------

## 8. Usuarios de prueba

Los siguientes usuarios se crean automáticamente cuando la aplicación
inicializa una base de datos vacía. Todos usan el valor definido en
`Seed:Password`; en la configuración de desarrollo indicada en esta guía
es `Dev12345!`.

  Rol             Correo electrónico          Ruta esperada
  --------------- --------------------------- ---------------
  Administrador   `admin@clinica.test`        `/admin`
  Secretaría      `secretaria@clinica.test`   `/secretaria`
  Odontólogo      `odontologo@clinica.test`   `/odontologo`
  Paciente        `paciente@clinica.test`     `/paciente`

Si el inicio de sesión devuelve `401` aunque estés usando los datos
correctos, consultá el [problema de
credenciales](#login-devuelve-401-con-la-contraseña-de-prueba).

------------------------------------------------------------------------

## 9. Rutina de trabajo diaria con Git

### 9.1 Primera vez: crear una rama para la tarea

No trabajes directamente sobre `development`. Desde la raíz:

``` bash
git checkout development
git pull origin development
git checkout -b feat/s1-XX-nombre-de-tu-tarea
```

Reemplazá el nombre de ejemplo por uno que describa tu tarea. Si la rama
ya existe, **no vuelvas a crearla**; usá
`git checkout nombre-de-tu-rama`.

### 9.2 Días siguientes: actualizar tu rama

Primero guardá o confirmá tus cambios para evitar perder trabajo.
Después:

``` bash
git checkout development
git pull origin development
git checkout feat/s1-XX-nombre-de-tu-tarea
git merge development
```

Resolvé cualquier conflicto de merge antes de seguir. Si tenés cambios
sin guardar, Git puede impedir el cambio de rama o el merge; revisá
`git status` y guardá tu trabajo primero.

### 9.3 Levantar el entorno

Desde la raíz del repositorio, en una terminal:

``` bash
docker compose up -d
dotnet run --project Clinica.Api --launch-profile https
```

En una segunda terminal:

``` bash
cd frontend
npm install
npm run dev
```

`npm install` es especialmente importante después de traer cambios que
modificaron `frontend/package.json` o `frontend/package-lock.json`. Si
ninguno cambió y las dependencias ya están instaladas, normalmente no
hace falta repetirlo todos los días.

### 9.4 Reiniciar la base de datos desde cero (acción destructiva)

**Esto elimina la base de datos local y todos sus datos.** Hacelo solo
si realmente necesitás reiniciarla y no hay datos que debas conservar.
Detené primero la API con `Ctrl + C` y asegurate de que `Seed:Password`
esté configurado.

Desde la raíz del repositorio:

``` bash
dotnet ef database drop --project Clinica.Infraestructura --startup-project Clinica.Api --force
dotnet run --project Clinica.Api --launch-profile https
```

Al iniciar de nuevo, la aplicación debería aplicar las migraciones y
volver a crear los usuarios de prueba si el seeder está configurado para
hacerlo.

------------------------------------------------------------------------

## 10. Solución de problemas

### La API indica que falta `Jwt:SecretKey`

**Causa probable:** el secreto no se configuró, se configuró en otro
proyecto o la clave tiene menos de 32 caracteres.

Desde la raíz:

``` bash
dotnet user-secrets list --project Clinica.Api
```

Si falta, repetí el paso de configuración de secretos de la [sección
5](#5-configurar-los-secretos-locales).

### `Unable to connect to any of the specified MySQL hosts`

**Causas probables:** Docker Desktop no está iniciado, el contenedor no
está en ejecución, el puerto 3306 está ocupado o la conexión no coincide
con la configuración de Docker.

Desde la raíz:

``` bash
docker compose ps
docker compose logs --tail=100
```

Si el contenedor no está en ejecución, abrí Docker Desktop y ejecutá
`docker compose up -d`. Si el error menciona que el puerto 3306 está
ocupado, seguí la [solución del puerto
3306](#32-error-frecuente-el-puerto-3306-ya-está-ocupado). Después
compará la conexión de la API con las variables de `docker-compose.yml`.

### Login devuelve 401 con la contraseña de prueba

**Causa probable:** la API se inició antes de configurar `Seed:Password`
o se cambió después. Si los usuarios ya existían, el seeder puede no
actualizar sus contraseñas automáticamente.

1.  Confirmá la contraseña configurada:

    ``` bash
    dotnet user-secrets list --project Clinica.Api
    ```

2.  Si necesitás que los usuarios se creen de nuevo, detené la API y
    seguí el procedimiento destructivo de [reinicio de base de
    datos](#94-reiniciar-la-base-de-datos-desde-cero-acción-destructiva).

**Advertencia:** ese procedimiento borra los datos de la base local. No
lo ejecutes si necesitás conservarlos.

### El frontend muestra "No se pudo conectar con el servidor"

1.  Confirmá que la API esté corriendo y que la terminal muestre
    `https://localhost:7068`.
2.  Verificá `https://localhost:7068/scalar/v1` en el navegador.
3.  Ejecutá `dotnet dev-certs https --trust`.
4.  Comprobá que `frontend/.env.development` tenga la URL correcta de la
    API.

### Error de CORS en la consola del navegador

El frontend debe abrirse desde `http://localhost:5173`. Si Vite no puede
usar ese puerto, cerrá la otra aplicación o instancia de Vite que lo
ocupa y volvé a ejecutar `npm run dev`. No cambies el puerto sin
actualizar también la configuración CORS del backend.

### `dotnet ef` no se reconoce

Instalá la herramienta global:

``` bash
dotnet tool install --global dotnet-ef
```

Si ya está instalada:

``` bash
dotnet tool update --global dotnet-ef
```

Cerrá y reabrí la terminal. Si el comando sigue sin reconocerse,
comprobá que la carpeta de herramientas globales de .NET esté incluida
en el `PATH`.

### `dotnet run` dice que no existe `Clinica.Api`

**Causa probable:** ejecutaste el comando desde una carpeta incorrecta.

Volvé a la raíz del repositorio:

``` bash
cd ..
```

Repetí `cd ..` si hace falta hasta llegar a `CentroOdontologico`, y
después ejecutá:

``` bash
dotnet run --project Clinica.Api --launch-profile https
```

### `npm install` falla o las dependencias parecen dañadas

Comprobá las versiones:

``` bash
node --version
npm --version
```

Usá Node.js LTS (20 o superior). Si sigue fallando, desde `frontend`
podés regenerar las dependencias instaladas localmente:

``` bash
npm install
```

Si el error persiste, compartí el mensaje completo antes de borrar
archivos o modificar el archivo de bloqueo de dependencias.

### Conflictos en archivos de migración

Si hay conflictos en `ClinicaDbContextModelSnapshot.cs` o en archivos de
migraciones, no borres archivos al azar ni generes otra migración para
intentar resolverlos. Coordiná con el equipo y seguí el procedimiento de
migraciones definido en `CONTRIBUTING.md`, si está disponible.

------------------------------------------------------------------------

## 11. Checklist de verificación

Antes de empezar a programar, comprobá lo siguiente:

-   [ ] Docker Desktop está abierto y el motor funciona.
-   [ ] `docker compose ps` muestra MySQL en ejecución.
-   [ ] Los secretos `Jwt:SecretKey` y `Seed:Password` están
    configurados.
-   [ ] `dotnet restore`, `dotnet build` y `dotnet test` finalizan
    correctamente.
-   [ ] La API muestra `https://localhost:7068` en la terminal.
-   [ ] La documentación de la API abre en
    `https://localhost:7068/scalar/v1` o `/scalar`.
-   [ ] El frontend inicia en `http://localhost:5173`.
-   [ ] Podés iniciar sesión con uno de los usuarios de prueba.

Si algo falla, compartí con el equipo **el paso exacto, el comando
ejecutado y el mensaje de error completo**. No compartas tokens, claves
JWT ni contraseñas reales.

------------------------------------------------------------------------

**Entorno local únicamente:** las credenciales de prueba y los secretos
de desarrollo de esta guía no deben utilizarse en producción.
