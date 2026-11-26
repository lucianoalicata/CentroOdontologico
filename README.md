<div align="center">

**Universidad Tecnológica Nacional — Facultad Regional Tucumán**
Seminario Integrador · Proyecto Final Integrador

### Sistema Integral de Gestión Odontológica (SIGO)

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-8-4479A1?logo=mysql&logoColor=white)
![React](https://img.shields.io/badge/React-19%20%2B%20TypeScript-61DAFB?logo=react&logoColor=black)
![Mantine](https://img.shields.io/badge/UI-Mantine%20v9-339AF0?logo=mantine&logoColor=white)
![Arquitectura](https://img.shields.io/badge/Arquitectura-Monolito%20Modular-2E7D32)

</div>

---

## Contenido

1. [Descripción del proyecto](#1-descripción-del-proyecto)
2. [Objetivos](#2-objetivos)
3. [Alcance funcional](#3-alcance-funcional)
4. [Estado del proyecto](#4-estado-del-proyecto)
5. [Arquitectura y tecnologías](#5-arquitectura-y-tecnologías)
6. [Estructura del repositorio](#6-estructura-del-repositorio)
7. [Puesta en marcha](#7-puesta-en-marcha)
8. [Documentación](#8-documentación)
9. [Flujo de trabajo](#9-flujo-de-trabajo)
10. [Equipo](#10-equipo)

---

## 1. Descripción del proyecto

Es una plataforma orientada a la centralización, trazabilidad y digitalización de la operación clínica, administrativa y contable de un centro odontológico. Reemplaza los registros en papel y las planillas aisladas por un sistema unificado que integra la atención clínica, la gestión de turnos, la autogestión del paciente y la facturación a prestadores de salud.

### Problemática

El circuito tradicional de un consultorio odontológico depende de soporte físico y herramientas fragmentadas, lo que genera:

- Ausentismo de pacientes por falta de recordatorios sistemáticos.
- Demoras en sala de espera y dificultad para registrar atenciones espontáneas (entre-turnos).
- Riesgo de pérdida o alteración de fichas clínicas y evoluciones.
- Inconsistencias en el cobro particular y débitos en las liquidaciones a obras sociales por falta de control del nomenclador.

---

## 2. Objetivos

| Objetivo | Descripción |
|---|---|
| **Gestión de turnos** | Agenda por profesional con disponibilidad configurable, turnos programados y atención espontánea (entre-turnos), solicitables por el propio paciente o cargados por recepción. |
| **Registro clínico digital** | Historia clínica con odontograma interactivo (notación FDI), diferenciando el estado inicial del evolutivo y conservando la trazabilidad de cada intervención. |
| **Trazabilidad económica** | Separación entre deuda (cargo) y cobro efectivo: señas, pagos parciales, cuotas y pago online mediante pasarela. |
| **Obras sociales** | Nomenclador de prestaciones, coberturas, autorizaciones previas y conformación de liquidaciones mensuales. |
| **Seguridad y auditoría** | Autenticación por roles, registro de quién y cuándo realizó cada operación, y borrado lógico en las entidades administrativas. |

---

## 3. Alcance funcional

| Perfil | Funcionalidades principales |
|---|---|
| **Odontólogo** | Configuración de su disponibilidad semanal. Visualización de su agenda diaria. Atención con odontograma interactivo, historia clínica, diagnósticos y adjuntos (radiografías y estudios). |
| **Secretaría** | Registro y búsqueda de pacientes. Alta de turnos y entre-turnos. Consulta de disponibilidad. Seguimiento de pagos y gestión de obras sociales. |
| **Paciente** | Solicitud de turnos en línea. Consulta de turnos anteriores, odontogramas e informes. Estado de cuenta y pago de saldos con billetera virtual o tarjeta. |
| **Administrador** | Alta y baja de personal, asignación de roles, mantenimiento del nomenclador y de las obras sociales. |

### Entre-turnos

Un entre-turno permite incorporar a un paciente sin ocupar una franja horaria de la grilla, por contacto telefónico o presencial con recepción o con el profesional. Se registra con datos provisorios (nombre, teléfono y, opcionalmente, DNI), se lista al final de la agenda del día y, al iniciar la atención, se vincula a una ficha existente o se crea una nueva con los datos precargados. La atención no puede iniciarse sin un paciente vinculado.

---

## 4. Estado del proyecto

| Etapa | Contenido | Estado |
|---|---|---|
| **Sprint 0 — Fundaciones** | Arquitectura en capas, núcleo compartido, `DbContext` modular, identidad (usuarios, roles y perfiles) y autenticación JWT (`POST /api/auth/login`, `GET /api/auth/me`). | Completado |
| **Sprint 0 — Frontend base** | Rutas protegidas por rol, sesión de usuario, Mantine v9 y cliente HTTP con manejo unificado de errores. | Completado |
| **Sprint 1 — Identidad y Pacientes** | CRUD completo de pacientes con domicilio y reactivación por DNI (`GET/POST/PUT/DELETE/PATCH /api/pacientes`), listado de odontólogos activos (`GET /api/odontologos`) y pantalla de pacientes (`/pacientes`). | Completado |
| **Sprint 1 — Turnos y Horarios** | Formularios frontend de turno/entre-turno (`TurnoFormModal`) y disponibilidad semanal (`/disponibilidad`) completados. Pendiente backend de turnos (`S1-12` a `S1-20`) y agenda del día (`S1-21`). | En progreso |
| **Sprint 2 — Integración** | Portal del paciente y flujo de atención completo (historia clínica y odontograma). | Planificado |
| **Posterior** | Pagos y pasarela (Mercado Pago), obras sociales, notificaciones. | Planificado |

---

## 5. Arquitectura y tecnologías

### Backend: Monolito Modular + Clean Architecture

```
Api  ──►  Aplicacion  ──►  Dominio
 │                           ▲
 └──►  Infraestructura ──────┘
```

| Capa | Responsabilidad |
|---|---|
| **Dominio** | Entidades, reglas de negocio (`Guard`, borrado lógico) y contratos de repositorio. Sin dependencias externas. |
| **Aplicación** | Servicios, casos de uso, DTOs, validaciones y puertos técnicos. |
| **Infraestructura** | Acceso a datos con EF Core (MySQL), migraciones, repositorios y seguridad. |
| **Api** | Controllers REST, políticas de autorización (`SoloPersonal`), `ProblemDetails` y documentación Scalar. |

### Frontend: Clean Architecture + Atomic Design

```
features (Pages)  ──►  ui (organisms / molecules / atoms)  ──►  application (hooks)  ──►  infrastructure (repositories)  ──►  domain (types)
```

| Capa | Responsabilidad |
|---|---|
| **`domain/`** | Tipos e interfaces puras de negocio (sin dependencias de React ni Mantine). |
| **`infrastructure/`** | Cliente Axios (`apiClient`), parser de errores y repositories por módulo (`pacientes`, `odontologos`, `turnos`, `horarios`). |
| **`application/`** | Custom hooks que encapsulan casos de uso de UI (`usePacientes`, `useOdontologos`, `useDisponibilidadOdontologo`, etc.). |
| **`ui/`** | Componentes organizados por Atomic Design (`atoms/`, `molecules/`, `organisms/`, `templates/`). |
| **`features/`** | Páginas delgadas que componen organismos y conectan rutas. |

### Stack tecnológico

| Componente | Tecnología |
|---|---|
| Backend | C# · .NET 10 · ASP.NET Core Web API |
| Persistencia | Entity Framework Core · Pomelo (proveedor MySQL) |
| Base de datos | MySQL 8 (contenedor Docker en desarrollo) |
| Seguridad | JWT Bearer · BCrypt |
| Documentación de la API | OpenAPI · Scalar (`/scalar/v1`) |
| Pruebas | xUnit |
| Frontend | React 19 · TypeScript · Vite · Mantine v9 |
| Integraciones previstas | Mercado Pago (pagos) · mensajería para recordatorios |

---

## 6. Estructura del repositorio

```
.
├── Clinica.Dominio/           Entidades, reglas de negocio, contratos
├── Clinica.Aplicacion/        Servicios, casos de uso, DTOs, puertos
├── Clinica.Infraestructura/   EF Core, migraciones, repositorios, seguridad
├── Clinica.Api/               Controllers y composición de la aplicación
├── Clinica.Tests/             Pruebas unitarias (xUnit)
├── frontend/
│   └── src/
│       ├── domain/            Tipos puros de negocio
│       ├── infrastructure/    Cliente HTTP, errorParser y repositories
│       ├── application/       Custom hooks (casos de uso de UI)
│       ├── ui/                Atoms, molecules, organisms y templates
│       ├── features/          Páginas por módulo (identidad, odontologos, turnos)
│       ├── auth/              Contexto y proveedor de sesión
│       ├── routes/            Enrutador y protección por roles
│       └── shared/            Páginas de error y utilidades
├── docs/                      Documentación técnica y planificación de sprints
├── LEVANTAR-PROYECTO.md       Guía paso a paso para levantar el entorno local
├── CONTRIBUTING.md            Guía de trabajo del equipo
└── README.md
```

---

## 7. Puesta en marcha

Para la guía detallada paso a paso (con solución de errores frecuentes), consultá **[`LEVANTAR-PROYECTO.md`](LEVANTAR-PROYECTO.md)**.

### Resumen rápido

1. **Requisitos:** SDK de .NET 10, Docker Desktop (iniciado) y Node.js 20+ LTS.
2. **Base de datos:**
   ```bash
   docker compose up -d
   ```
3. **Secretos locales (una sola vez):**
   ```bash
   cd Clinica.Api
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:SecretKey" "<clave aleatoria de 32 caracteres o más>"
   dotnet user-secrets set "Seed:Password" "Dev12345!"
   cd ..
   ```
4. **Backend (en HTTPS, puerto 7068):**
   ```bash
   dotnet run --project Clinica.Api --launch-profile https
   ```
   Documentación interactiva disponible en `https://localhost:7068/scalar/v1`.
5. **Frontend (puerto 5173):**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

### Usuarios de prueba

Todos comparten la contraseña configurada en `Seed:Password` (`Dev12345!`).

| Rol | Correo | Ruta inicial |
|---|---|---|
| Administrador | `admin@clinica.test` | `/administrador` |
| Secretaría | `secretaria@clinica.test` | `/secretaria` |
| Odontólogo | `odontologo@clinica.test` | `/odontologo` |
| Paciente | `paciente@clinica.test` | `/paciente` |

---

## 8. Documentación

| Documento | Contenido |
|---|---|
| [`LEVANTAR-PROYECTO.md`](LEVANTAR-PROYECTO.md) | Guía completa de instalación, configuración y resolución de problemas. |
| [`convenciones-dominio.md`](CONVENCIONES-DOMINIO.md) | Convenciones de arquitectura, dominio, persistencia y seguridad. |
| [`docs/sprints/sprint-1.md`](docs/sprints/sprint-1.md) | Planificación y alcance del Sprint 1. |
| [`CONTRIBUTING.md`](CONTRIBUTING.md) | Flujo de ramas, commits y pull requests. |

---

## 9. Flujo de trabajo

- `main`: versiones estables, probadas y listas para entrega.
- `development`: rama principal de integración continua del equipo.
- Cada tarea se desarrolla en una rama propia (`feat/s1-XX-...`) derivada de `development` y se incorpora mediante *Pull Request* o merge controlado.

El detalle completo se encuentra en [`CONTRIBUTING.md`](CONTRIBUTING.md).

---

## 10. Equipo de Desarrollo

| Integrantes |
|---|
| Alicata, Luciano Antonio |
| Castro, María Belén |
| Fernández, Ignacio |
| Leyva, María Belén |

Universidad Tecnológica Nacional — Facultad Regional Tucumán · Seminario Integrador · 2026
