# Guía de puesta en marcha del entorno de desarrollo

Guía para clonar, configurar y ejecutar **SIGO (Centro Odontológico)** en Windows 10/11. Incluye MySQL en Docker, API ASP.NET Core sobre .NET 10 y frontend React/Vite con Mantine v9.

**Tiempo estimado:** 15–25 minutos en un equipo con las herramientas instaladas.

> **Importante:** seguí los pasos en orden. Configurá los secretos **antes del primer arranque de la API** y comprobá que Docker Desktop tenga el motor iniciado.

## Contenido

1. [Requisitos](#1-requisitos)
2. [Clonar el repositorio](#2-clonar-el-repositorio)
3. [Iniciar MySQL con Docker](#3-iniciar-mysql-con-docker)
4. [Confiar en el certificado HTTPS](#4-confiar-en-el-certificado-https)
5. [Configurar los secretos locales](#5-configurar-los-secretos-locales)
6. [Restaurar, compilar y ejecutar el backend](#6-restaurar-compilar-y-ejecutar-el-backend)
7. [Instalar y ejecutar el frontend](#7-instalar-y-ejecutar-el-frontend)
8. [Usuarios de prueba y pantallas disponibles](#8-usuarios-de-prueba-y-pantallas-disponibles)
9. [Rutina de trabajo diaria con Git](#9-rutina-de-trabajo-diaria-con-git)
10. [Notas de arquitectura para desarrollar sobre este repo](#10-notas-de-arquitectura-para-desarrollar-sobre-este-repo)
11. [Solución de problemas](#11-solución-de-problemas)
12. [Checklist de verificación](#12-checklist-de-verificación)

---

## 1. Requisitos

Instalá las siguientes herramientas antes de empezar:

| Herramienta | Requisito | Verificación |
| :--- | :--- | :--- |
| Git | Versión actual | `git --version` |
| SDK de .NET | **10.0.x** | `dotnet --list-sdks` |
| Docker Desktop | Versión actual, con motor iniciado | `docker --version` y `docker compose version` |
| Node.js | **20 o superior**, preferentemente LTS | `node --version` |
| npm | Incluido con Node.js | `npm --version` |
| `dotnet-ef` | Herramienta global de EF Core | `dotnet ef --version` |
| Editor | Visual Studio, Rider o VS Code con C# Dev Kit | — |

### 1.1 Verificar .NET 10

```bash
dotnet --list-sdks
```

Debe aparecer el SDK **10.0.x** en la lista. Si no aparece, instalalo desde la página oficial de .NET de Microsoft y abrí una terminal nueva.

### 1.2 Verificar Docker Desktop

Abrí Docker Desktop y esperá a que la barra inferior indique que el motor está en ejecución (*Engine running*). Comprobá desde la terminal:

```bash
docker compose version
docker info
```

### 1.3 Instalar `dotnet-ef`

```bash
dotnet tool install --global dotnet-ef
```

Si ya lo tenías instalado:

```bash
dotnet tool update --global dotnet-ef
```

---

## 2. Clonar el repositorio

Abrí **Git Bash** o **PowerShell** en tu carpeta de trabajo (por ejemplo, `C:\Dev`):

```bash
git clone https://github.com/lucianoalicata/CentroOdontologico.git
cd CentroOdontologico
git checkout development
git pull origin development
```

> **Importante:** trabajamos siempre sobre ramas creadas desde **`development`**, nunca directamente sobre `main`.

Comprobá que estás en la raíz del repositorio (`Clinica.Api`, `Clinica.Infraestructura`, `frontend` y `docker-compose.yml` deben estar visibles):

```bash
git status
```

---

## 3. Iniciar MySQL con Docker

### 3.1 Arrancar el contenedor

Desde la raíz del repositorio:

```bash
docker compose up -d
docker compose ps
```

El contenedor de MySQL debe figurar como `Up` o `healthy` exponiendo el puerto **3306**.

### 3.2 Error frecuente: el puerto 3306 ya está ocupado

Si aparece `Bind for 0.0.0.0:3306 failed: port is already allocated`, tenés un servicio local de MySQL instalado en Windows ocupando el puerto:

1. Presioná `Win + R`, escribí `services.msc` y presioná Enter.
2. Buscá el servicio `MySQL80` o `MySQL`, hacé clic derecho → **Detener**.
3. Volvé a ejecutar `docker compose up -d`.

> No continúes con el backend hasta que el contenedor de MySQL esté corriendo.

---

## 4. Confiar en el certificado HTTPS

La API corre con perfil `https` y el frontend se conecta a `https://localhost:7068`. Ejecutá una sola vez:

```bash
dotnet dev-certs https --trust
```

Aceptá la ventana de confirmación de Windows si aparece.

---

## 5. Configurar los secretos locales

**Hacé este paso antes de arrancar la API por primera vez**, ya que el seeder utiliza `Seed:Password` para crear los usuarios iniciales.

Desde la raíz del repositorio:

```bash
cd Clinica.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "CAMBIAR_POR_UNA_CLAVE_LOCAL_ALEATORIA_DE_32_CARACTERES_O_MAS"
dotnet user-secrets set "Seed:Password" "Dev12345!"
cd ..
```

* Reemplazá `Jwt:SecretKey` por cualquier texto aleatorio de **al menos 32 caracteres**.
* La cadena de conexión (`ConnectionStrings:DefaultConnection`) ya viene configurada en `appsettings.json` apuntando a `localhost:3306` (`odontodb`), por lo que **no hace falta agregarla** a User Secrets.

Verificá que quedaron guardados:

```bash
dotnet user-secrets list --project Clinica.Api
```

---

## 6. Restaurar, compilar y ejecutar el backend

Desde la **raíz** de `CentroOdontologico`:

```bash
dotnet restore
dotnet build
dotnet test
```

Con las pruebas en verde, iniciá la API **usando el perfil HTTPS**:

```bash
dotnet run --project Clinica.Api --launch-profile https
```

Dejá esta terminal abierta. Deberías ver:

```text
Now listening on: https://localhost:7068
```

### Verificar la API en Scalar

Abrí en el navegador `https://localhost:7068/scalar/v1`. En el primer arranque, EF Core aplica automáticamente todas las migraciones pendientes y carga los datos iniciales (4 usuarios de prueba, 1 odontóloga y 2 pacientes).

Para probar endpoints protegidos en Scalar:
1. Ejecutá `POST /api/auth/login` con `"email": "secretaria@clinica.test"` y `"password": "Dev12345!"`.
2. Copiá el `token` de la respuesta (si no lo ves en la vista compacta, hacé clic en **Open API Client** abajo a la izquierda).
3. Pegalo en la pestaña **Auth** (tipo `Bearer`) del endpoint que quieras probar.

---

## 7. Instalar y ejecutar el frontend

Abrí una **segunda terminal** (sin cerrar la del backend) y ejecutá:

```bash
cd frontend
npm install
npm run dev
```

Abrí `http://localhost:5173` en el navegador.

> **Nota sobre puertos:** Vite debe correr exactamente en `http://localhost:5173` porque es el origen habilitado en la configuración CORS del backend. Si está ocupado, cerrá la otra instancia antes de iniciar.

---

## 8. Usuarios de prueba y pantallas disponibles

Todos usan la contraseña `Dev12345!`:

| Rol | Correo electrónico | Ruta inicial | Qué podés probar hoy |
| :--- | :--- | :--- | :--- |
| Secretaría | `secretaria@clinica.test` | `/secretaria` | Menú lateral → **Pacientes** (`/pacientes`) y **Horarios** (`/disponibilidad`) |
| Odontólogo | `odontologo@clinica.test` | `/odontologo` | Menú lateral → **Pacientes** (`/pacientes`) y **Horarios** (`/disponibilidad`) |
| Administrador | `admin@clinica.test` | `/administrador` | Acceso total a administración y personal |
| Paciente | `paciente@clinica.test` | `/paciente` | Portal del paciente |

---

## 9. Rutina de trabajo diaria con Git

### 9.1 Arrancar una tarea nueva

Siempre partiendo de `development` actualizado:

```bash
git checkout development
git pull origin development
git checkout -b feat/s1-XX-nombre-tarea
```

### 9.2 Traer cambios de compañeros a tu rama

```bash
git checkout development
git pull origin development
git checkout feat/s1-XX-nombre-tarea
git merge development
```

### 9.3 Levantar el entorno todos los días

Terminal 1 (raíz):
```bash
docker compose up -d
dotnet run --project Clinica.Api --launch-profile https
```

Terminal 2 (`frontend/`):
```bash
cd frontend
npm run dev
```
*(Si `git pull` trajo cambios en `package.json`, ejecutá `npm install` antes de `npm run dev`.)*

### 9.4 Reiniciar la base de datos desde cero (si hubo cambios en el seeder o migraciones)

Detené el backend con `Ctrl + C` y ejecutá desde la raíz:

```bash
dotnet ef database drop --project Clinica.Infraestructura --startup-project Clinica.Api --force
dotnet run --project Clinica.Api --launch-profile https
```

---

## 10. Notas de arquitectura para desarrollar sobre este repo

### Backend
* **Autorización:** Los endpoints administrativos/clínicos usan `[Authorize(Policy = Politicas.SoloPersonal)]` (permite `Secretaria`, `Odontologo` y `Administrador`).
* **Errores:** Lanzá `DomainException` (devuelve `400 Bad Request`) o `NoEncontradoException` (devuelve `404 Not Found`); el middleware `ManejadorExcepciones` los convierte automáticamente a `ProblemDetails`.
* **Migraciones:** Si agregás o modificás entidades, generá la migración desde la raíz y avisá al equipo antes de mergear:
  ```bash
  dotnet ef migrations add NombreMigracion --project Clinica.Infraestructura --startup-project Clinica.Api
  ```

### Frontend (`frontend/src/`)
El frontend sigue **Clean Architecture + Atomic Design**:
* **`domain/`**: interfaces y tipos puros (`pacientes`, `odontologos`, `turnos`, `horarios`, `auth`).
* **`infrastructure/`**: cliente HTTP `apiClient` (`infrastructure/http/apiClient.ts`) y repositories (`pacientesRepository`, `odontologosRepository`, `turnosRepository`, `horariosRepository`). **No uses `fetch` ni `axios` directo en componentes.**
* **`application/`**: custom hooks (`usePacientes`, `useOdontologos`, `useDisponibilidadOdontologo`, etc.).
* **`ui/`**: componentes reutilizables divididos en `atoms/`, `molecules/` y `organisms/` (incluye `PacienteFormModal` y `TurnoFormModal` ya listos para invocar desde la agenda).
* **`features/`**: páginas de cada módulo.
* **Linter:** Antes de hacer commit en frontend, verificá que pasen `npm run lint` y `npm run build` (React 19 tiene activa la regla `react-hooks/set-state-in-effect`, por lo que no se debe llamar a `setState` síncronamente dentro de un `useEffect`).

---

## 11. Solución de problemas

* **La API pide `Jwt:SecretKey`:** Revisá `dotnet user-secrets list --project Clinica.Api` y repetí la [sección 5](#5-configurar-los-secretos-locales).
* **`Unable to connect to any of the specified MySQL hosts`:** Verificá que Docker Desktop esté iniciado y que `docker compose ps` muestre MySQL arriba en el puerto 3306.
* **Login devuelve `401` con `Dev12345!`:** La base se inicializó antes de configurar `Seed:Password`. Ejecutá el reinicio de base de datos de la [sección 9.4](#94-reiniciar-la-base-de-datos-desde-cero-si-hubo-cambios-en-el-seeder-o-migraciones).
* **Error de certificado HTTPS o conexión en el frontend:** Confirmá que el backend corra con `--launch-profile https` (puerto `7068`) y volvé a correr `dotnet dev-certs https --trust`.
* **Conflictos en migraciones (`ClinicaDbContextModelSnapshot.cs`):** No edites el snapshot manualmente ni borres migraciones previas; coordiná con el equipo antes de mergear.

---

## 12. Checklist de verificación

- [ ] Docker Desktop abierto y MySQL corriendo (`docker compose ps`).
- [ ] Secretos `Jwt:SecretKey` y `Seed:Password` configurados en `Clinica.Api`.
- [ ] `dotnet test` pasa en verde.
- [ ] Backend escuchando en `https://localhost:7068` y Scalar accesible en `/scalar/v1`.
- [ ] Frontend corriendo en `http://localhost:5173` y permite iniciar sesión con `secretaria@clinica.test`.