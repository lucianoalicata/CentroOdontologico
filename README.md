<div align="center">

**Universidad Tecnológica Nacional — Facultad Regional Tucumán**
Seminario Integrador · Proyecto Final Integrador

### Sistema Integral de Gestión Odontológica

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-8-4479A1?logo=mysql&logoColor=white)
![React](https://img.shields.io/badge/React-TypeScript-61DAFB?logo=react&logoColor=black)
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

SIGO es una plataforma orientada a la centralización, trazabilidad y digitalización de la operación clínica, administrativa y contable de un centro odontológico. Reemplaza los registros en papel y las planillas aisladas por un sistema unificado que integra la atención clínica, la gestión de turnos, la autogestión del paciente y la facturación a prestadores de salud.

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
| **Sprint 0 — Fundaciones** | Arquitectura en capas, núcleo compartido, `DbContext` modular, identidad (usuarios, roles y perfiles) y autenticación JWT. | Backend completado |
| **Sprint 0 — Frontend base** | Rutas, sesión de usuario y cliente HTTP (S0-06). | En curso |
| **Sprint 1 — Módulos en paralelo** | Identidad y pacientes · Turnos y agenda · Historia clínica y prestaciones · Odontograma. | Planificado |
| **Sprint 2 — Integración** | Portal del paciente y flujo de atención completo. | Planificado |
| **Posterior** | Pagos y pasarela, obras sociales (previo relevamiento en el centro), notificaciones. | Planificado |

---

## 5. Arquitectura y tecnologías

El sistema es un **monolito modular** con **arquitectura limpia (Clean Architecture)**: capas con dependencias dirigidas hacia el dominio y módulos de negocio con bajo acoplamiento entre sí.

```
Api  ──►  Aplicacion  ──►  Dominio
 │                           ▲
 └──►  Infraestructura ──────┘
```

| Capa | Responsabilidad |
|---|---|
| **Dominio** | Entidades, reglas de negocio y contratos de persistencia. No depende de ninguna otra capa. |
| **Aplicación** | Servicios y casos de uso, DTOs y puertos técnicos (hash de contraseñas, generación de tokens). |
| **Infraestructura** | Acceso a datos con EF Core, repositorios y servicios externos. |
| **Api** | Controllers, autenticación, autorización y configuración de la aplicación. |

| Componente | Tecnología |
|---|---|
| Backend | C# · .NET 10 · ASP.NET Core Web API |
| Persistencia | Entity Framework Core · Pomelo (proveedor MySQL) |
| Base de datos | MySQL 8 (contenedor Docker en desarrollo) |
| Seguridad | JWT Bearer · BCrypt |
| Documentación de la API | OpenAPI · Scalar |
| Pruebas | xUnit |
| Frontend | React · TypeScript · Vite |
| Integraciones previstas | Mercado Pago (pagos) · mensajería para recordatorios |

---

## 6. Estructura del repositorio

```
.
├── Clinica.Dominio/           Entidades, reglas de negocio, contratos
├── Clinica.Aplicacion/        Servicios, casos de uso, DTOs, puertos
├── Clinica.Infraestructura/   EF Core, repositorios, seguridad
├── Clinica.Api/               Controllers y composición de la aplicación
├── Clinica.Tests/             Pruebas unitarias
├── frontend/                  Aplicación web (en construcción)
├── docs/                      Documentación técnica
├── CONTRIBUTING.md            Guía de trabajo del equipo
└── README.md
```

Cada capa se organiza en carpetas por módulo (`Identidad`, `Turnos`, `HistoriaClinica`, `ObrasSociales`, `Pagos`, `Notificaciones`).

---

## 7. Puesta en marcha

### Requisitos

- SDK de .NET 10
- Docker y Docker Compose
- Node.js (versión LTS), para el frontend

### Base de datos

```bash
docker compose up -d
```

### Configuración local

Los valores sensibles no se almacenan en el repositorio. Cada integrante los configura en su equipo con `user-secrets`:

```bash
cd Clinica.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<cadena de conexión a MySQL>"
dotnet user-secrets set "Jwt:SecretKey" "<clave aleatoria de 32 caracteres o más>"
dotnet user-secrets set "Seed:Password" "<contraseña para los usuarios de prueba>"
```

El emisor, la audiencia y la duración del token (`Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes`) y los orígenes permitidos por CORS (`Cors:Origins`) se definen en `appsettings.Development.json`.

### Ejecución

```bash
dotnet run --project Clinica.Api
```

En el entorno de desarrollo, al iniciar la aplicación se aplican las migraciones pendientes y se cargan los usuarios de prueba. La documentación interactiva de la API está disponible en `/scalar`.

### Usuarios de prueba

Todos comparten la contraseña definida en `Seed:Password`.

| Rol | Correo |
|---|---|
| Administrador | `admin@clinica.test` |
| Secretaría | `secretaria@clinica.test` |
| Odontólogo | `odontologo@clinica.test` |
| Paciente | `paciente@clinica.test` |

### Pruebas

```bash
dotnet test
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

---

## 8. Documentación

| Documento | Contenido |
|---|---|
| [`docs/convenciones-dominio.md`](docs/convenciones-dominio.md) | Convenciones de arquitectura, dominio, persistencia y seguridad. |
| [`CONTRIBUTING.md`](CONTRIBUTING.md) | Flujo de ramas, commits y pull requests. |

---

## 9. Flujo de trabajo

- `main`: versiones estables, probadas y listas para entrega.
- `development`: integración continua del equipo.
- Cada tarea se desarrolla en una rama propia derivada de `development` y se incorpora mediante *Pull Request* revisado por otro integrante.

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
