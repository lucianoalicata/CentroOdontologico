# Convenciones de Dominio y Arquitectura

Reglas del equipo para mantener la consistencia del código entre módulos. Si una regla resulta inconveniente, se discute y se modifica en este documento; no se incumple de forma aislada.

## Contenido

1. [Estructura de la solución](#1-estructura-de-la-solución)
2. [Entidades](#2-entidades)
3. [Relaciones entre módulos](#3-relaciones-entre-módulos)
4. [Persistencia con EF Core](#4-persistencia-con-ef-core)
5. [Identidad, autenticación y autorización](#5-identidad-autenticación-y-autorización)
6. [Capa de aplicación y API](#6-capa-de-aplicación-y-api)
7. [Nombres](#7-nombres)
8. [Pruebas](#8-pruebas)
9. [Frontend](#9-frontend)
10. [Decisiones de modelado vigentes](#10-decisiones-de-modelado-vigentes)

---

## 1. Estructura de la solución

```
.
├── Clinica.Dominio/           Entidades, reglas de negocio y contratos de persistencia
├── Clinica.Aplicacion/        Servicios, casos de uso, DTOs y puertos técnicos
├── Clinica.Infraestructura/   EF Core, repositorios, seguridad y servicios externos
├── Clinica.Api/               Controllers, autenticación y composición de la aplicación
├── Clinica.Tests/             Pruebas unitarias (xUnit)
├── frontend/                  Aplicación web
└── docs/                      Documentación
```

### Dependencias entre proyectos

```
Api  ──►  Aplicacion  ──►  Dominio
 │                           ▲
 └──►  Infraestructura ──────┘      (Infraestructura también referencia a Aplicacion)
```

- **Dominio** no referencia a EF Core, ASP.NET ni a ninguna librería de infraestructura.
- **Aplicación** depende solo de Dominio.
- **Infraestructura** implementa los contratos definidos en Dominio y Aplicación.
- **Api** es la raíz de composición: registra las dependencias y expone los endpoints.

### Dónde vive cada contrato

| Tipo de contrato | Ubicación | Ejemplo |
|---|---|---|
| Persistencia (repositorios) | Dominio | `IUsuarioRepository`, `ITurnoRepository` |
| Capacidades técnicas | Aplicación | `IPasswordHasher`, `IJwtProvider` |
| Contexto del usuario autenticado | Aplicación | `IUsuarioActual` |

### Organización por módulos

Cada capa se organiza en carpetas por módulo de negocio, no por tipo de archivo:

```
Comun/  Identidad/  Turnos/  HistoriaClinica/  ObrasSociales/  Pagos/  Notificaciones/
```

Cada integrante trabaja dentro de las carpetas de su módulo.

### Visibilidad

Todo tipo de `Dominio` y `Aplicacion` se declara `public` de forma explícita. Visual Studio genera `internal` por defecto: debe corregirse al crear el archivo. La única excepción justificada es el constructor `internal` de `UsuarioRol`.

---

## 2. Entidades

### Clase base

Todas las entidades heredan de `Entidad` (`Clinica.Dominio.Comun`), que aporta:

- `Id` (`int`, autoincremental).
- `FechaCreacion` y `FechaModificacion` en UTC, completadas automáticamente por el `DbContext` al guardar.

Las tablas puente con clave compuesta (por ejemplo, `UsuarioRol`) no heredan de `Entidad`.

### Borrado lógico

Solo las entidades que heredan de `EntidadConBorradoLogico` se dan de baja. Aporta `Activo`, `FechaBaja`, `Desactivar()` y `Reactivar()`.

- Aplica a: `Usuario`, los perfiles, `ObraSocial`, `Prestacion`, `HorarioDisponible`, `Afiliacion`, `Especialidad`.
- **No** aplica a datos clínicos ni financieros (`EntradaHistoriaClinica`, `EstadoPieza`, `ArchivoAdjunto`, `Cargo`, `Pago`): nunca se borran. Un error se corrige con una entrada nueva o se anula con un estado.
- El `DbContext` aplica un filtro global `Activo == true` y convierte los `Remove()` en bajas lógicas. Para consultar registros dados de baja se usa `IgnoreQueryFilters()` de forma explícita.
- Si una persona dada de baja vuelve a registrarse, **se reactiva** el registro existente en lugar de crear uno nuevo. Esto evita duplicados y conserva su historia.

### Plantilla de entidad

```csharp
public class Turno : Entidad
{
    public int OdontologoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public EstadoTurno Estado { get; private set; }

    protected Turno() { }   // solo para EF Core

    public Turno(int odontologoId, DateOnly fecha)
    {
        OdontologoId = Guard.IdValido(odontologoId, "Odontólogo");
        Fecha = fecha;
        Estado = EstadoTurno.Pendiente;
    }

    public void Iniciar()
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new DomainException("Solo se puede iniciar un turno pendiente.");
        Estado = EstadoTurno.EnCurso;
    }
}
```

### Reglas

1. **Setters `private`.** El estado cambia solo mediante métodos del dominio (`Iniciar()`, `Cancelar(motivo, usuarioId)`), nunca desde un controller.
2. **Dos constructores:** uno `protected` sin parámetros (para EF Core) y uno público que valida.
3. **Las validaciones viven en la entidad** y lanzan `DomainException`. Se usa `Guard` para las comunes (`NoVacio`, `IdValido`, `NoNegativo`).
4. **Mensajes legibles:** los mensajes de error son visibles para el usuario. Se pasan etiquetas como `"Nombre"` o `"Teléfono"` a `Guard`, no `nameof(parametro)`.
5. **Fechas en UTC** (`DateTime.UtcNow`). La conversión a hora local se hace solo en el frontend. Para fechas sin hora se usa `DateOnly`.
6. **Colecciones** expuestas como `IReadOnlyCollection<T>` con un campo privado, para que los elementos se agreguen únicamente mediante métodos de la entidad.
7. **Enums** en su propio archivo, dentro de la carpeta del módulo.
8. **Dinero:** `decimal`, nunca `double` ni `float`.

---

## 3. Relaciones entre módulos

**Un módulo referencia a las entidades de otro únicamente por Id.**

```csharp
// Correcto
public int PacienteId { get; private set; }

// Incorrecto: acopla Turnos con Identidad
public PacientePerfil Paciente { get; private set; }
```

- No hay propiedades de navegación entre módulos distintos. Dentro de un mismo módulo sí se permiten (`HistoriaClinica` → `EntradaHistoriaClinica`).
- Si un módulo necesita datos de otro, los obtiene mediante una interfaz de lectura definida en su propia capa de Aplicación.
- **Los identificadores externos apuntan al perfil, no al usuario:**

| Campo | Referencia a |
|---|---|
| `PacienteId` | `PacientePerfil.Id` |
| `OdontologoId` | `OdontologoPerfil.Id` |
| `SecretariaId` | `SecretariaPerfil.Id` |

De este modo, la clave foránea garantiza que el identificador corresponda a un paciente o a un profesional real.

---

## 4. Persistencia con EF Core

- Un único `ClinicaDbContext`, con todas las entidades configuradas mediante `IEntityTypeConfiguration<T>`.
- Cada entidad tiene su clase de configuración en `Clinica.Infraestructura/Configuraciones/<Módulo>/`, con el nombre `<Entidad>Config`. Se cargan automáticamente con `ApplyConfigurationsFromAssembly`; nadie edita un archivo central.
- No se usan atributos de EF (`[Key]`, `[Required]`) en el dominio: todo se configura con Fluent API.
- No se usa *lazy loading*. Las colecciones necesarias se cargan con `Include`.
- Las relaciones sobre datos con borrado lógico usan `DeleteBehavior.Restrict`.

### Convenciones para MySQL

| Tema | Regla |
|---|---|
| Proveedor | Pomelo. La versión debe corresponder a la versión mayor de EF Core instalada. |
| Codificación | `utf8mb4`. |
| Enums | Se guardan como texto (`HasConversion<string>()`) con longitud máxima definida. |
| Strings | Siempre con `HasMaxLength`; evitar columnas `longtext`. |
| Decimales | Precisión explícita, `decimal(18,2)` por defecto. |
| Índices únicos | Índice único normal. MySQL no admite índices filtrados: la unicidad convive con la regla de reactivación del borrado lógico. |

### Migraciones

1. Ejecutar `git pull origin development` **antes** de crear una migración.
2. Nombre descriptivo: `AgregarHorariosDisponibles`, no `Migration1`.
3. Si hay conflicto en `ClinicaDbContextModelSnapshot.cs` al abrir el pull request: eliminar la migración propia, actualizar la rama con `development` y volver a generarla. El *snapshot* no se resuelve a mano.
4. Los pull requests que incluyen migraciones se integran de a uno.
5. Una migración ya integrada nunca se modifica: se crea una nueva.

En el entorno de desarrollo, la API aplica las migraciones pendientes y carga los datos de prueba al iniciar.

---

## 5. Identidad, autenticación y autorización

### Modelo de identidad

- `Usuario` representa una cuenta de acceso. Se relaciona con `UsuarioRol` (uno a muchos) y con perfiles opcionales (uno a cero o uno): `PacientePerfil`, `OdontologoPerfil`, `SecretariaPerfil`.
- Roles disponibles (`RolUsuario`): `Administrador`, `Secretaria`, `Odontologo`, `Paciente`. El administrador es un rol, no un indicador adicional: una persona puede acumular varios.
- `PacientePerfil` existe de forma independiente: tiene nombre, apellido y DNI propios, y su `UsuarioId` es opcional. Un paciente puede estar registrado sin cuenta web y obtenerla más adelante.
- Un usuario siempre conserva al menos un rol.
- La coherencia entre rol y perfil (por ejemplo, "un usuario con rol Odontólogo debe tener su `OdontologoPerfil`") se garantiza en la capa de aplicación, dentro de una transacción.

### Autenticación

- Tokens JWT firmados con HMAC-SHA256. Contienen el identificador del usuario (`sub`), el correo, los roles y los identificadores de perfil (`pacienteId`, `odontologoId`, `secretariaId`) cuando existen.
- Los nombres de los claims están centralizados en `ClaimNombres`. No se escriben textos literales en otras partes del código.
- Las contraseñas se almacenan con BCrypt, mediante `IPasswordHasher`. El dominio solo recibe el hash.
- La configuración JWT (`JwtSettings`) se valida al iniciar: la aplicación no arranca si falta un dato o si la clave tiene menos de 32 caracteres.
- Un token no puede revocarse antes de expirar; por ello su duración es corta y configurable.
- Un único mensaje de error para credenciales inválidas, usuario inexistente o dado de baja, para no revelar qué cuentas existen.

### Autorización

- Los casos de uso reciben `IUsuarioActual` por inyección. **Nunca** leen claims ni `HttpContext` directamente.
- La autorización por rol se declara con `[Authorize(Roles = "...")]`. Las políticas compartidas (`SoloPersonal`, `SoloAdministrador`) se definen en un único lugar.
- Las reglas de pertenencia ("un paciente solo ve sus propios turnos", "un odontólogo solo ve su agenda") se aplican en los casos de uso usando `IUsuarioActual`.

---

## 6. Capa de aplicación y API

### Servicios y casos de uso

- La lógica de un flujo (qué se valida, qué errores se producen, qué se devuelve) vive en la capa de **Aplicación**, nunca en Infraestructura.
- Un servicio agrupa las operaciones de un mismo ámbito (`AuthService`). Una operación compleja puede ser un caso de uso individual con el formato `<Verbo><Sustantivo>UseCase` (`CrearEntreTurnoUseCase`).
- Infraestructura solo implementa lo técnico: persistencia, hash, tokens, servicios externos.

### DTOs

- Se definen como `record` con el sufijo `Dto` (`LoginDto`, `UsuarioSesionDto`).
- Los controllers y los servicios no exponen entidades del dominio.

### Controllers

- Son finos: reciben el request, invocan un servicio o caso de uso y devuelven el resultado.
- No contienen lógica de negocio. Si hay un `if` sobre una regla del negocio, pertenece al dominio o a la capa de aplicación.
- Se documentan con OpenAPI (Scalar). Los endpoints protegidos aparecen con soporte de token Bearer.

### Manejo de errores

El `ManejadorExcepciones` es el único punto donde las excepciones se convierten en respuestas HTTP (formato `ProblemDetails`):

| Excepción | Respuesta |
|---|---|
| `DomainException` | 400 Bad Request |
| `CredencialesInvalidasException` | 401 Unauthorized |
| Recurso inexistente | 404 Not Found (se incorpora al manejador al primer uso) |
| Falta de permisos | 403 Forbidden |
| Cualquier otra | 500, con registro en el log y sin exponer detalles internos |

Los controllers no usan `try/catch`.

---

## 7. Nombres

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases, enums y métodos | PascalCase, en español para el lenguaje del negocio | `HorarioDisponible`, `Cancelar()` |
| Interfaces | Prefijo `I` | `ITurnoRepository` |
| Servicios | `<Ámbito>Service` | `AuthService` |
| Casos de uso | `<Verbo><Sustantivo>UseCase` | `SacarTurnoUseCase` |
| DTOs | Sufijo `Dto` | `CrearTurnoDto` |
| Configuración EF | `<Entidad>Config` | `TurnoConfig` |
| Tablas | Plural, en español | `Turnos`, `Pacientes` |

Los términos técnicos pueden permanecer en inglés (`Repository`, `Guard`, `Controller`).

---

## 8. Pruebas

- Toda regla de negocio nueva incluye al menos una prueba en `Clinica.Tests`.
- Nombre: `Metodo_Cuando_Entonces`, por ejemplo `Iniciar_CuandoNoEstaPendiente_EntoncesLanzaDomainException`.
- Los nombres deben ser legibles y describir la regla verificada. No se usan `Test1`, `Test2`, etc. No importa que sean largos: importa que sean claros.
- Las pruebas de dominio y de servicios **no** usan base de datos. Los repositorios y servicios externos se reemplazan por implementaciones falsas escritas a mano.
- Como `Id` tiene un setter protegido, las pruebas lo asignan por reflexión mediante un auxiliar.

---

## 9. Frontend

- React con TypeScript, organizado por módulos (`features/<módulo>`), reflejando la estructura del backend.
- Los componentes no invocan `fetch` ni `axios` directamente: usan el cliente HTTP único, que agrega el token y traduce los errores.
- La sesión se administra en un único contexto de autenticación. Las rutas protegidas declaran los roles que las pueden visitar.
- Las fechas se reciben en UTC y se convierten a hora local únicamente en la interfaz.
- Una sola biblioteca de componentes de interfaz para todo el equipo.

---

## 10. Decisiones de modelado vigentes

### Turnos y entre-turnos

- `TipoTurno`: `Programado` o `EntreTurno`.
- **Programado:** tiene fecha y franja horaria, exige `PacienteId` y no puede solaparse con otro turno programado activo del mismo odontólogo. Se valida contra la disponibilidad cargada por el profesional.
- **Entre-turno:** no ocupa una franja de la grilla. Se ordena por `OrdenEntreTurno` (máximo del día y del odontólogo más uno) y se muestra al final de la agenda del día.
- **Paciente provisorio:** cuando aún no hay ficha, el turno guarda `PacienteWalkInNombre`, `PacienteWalkInTelefono` y, de forma opcional, `PacienteWalkInDni`. Por teléfono se solicita el DNI, pero puede quedar pendiente. Si el DNI informado ya existe, el sistema sugiere vincular la ficha existente.
- **Inicio de la atención:** requiere paciente vinculado. Si falta, el sistema ofrece "Vincular paciente existente" o "Crear ficha", esta última con los datos provisorios precargados. Solo después se habilita la pantalla de atención.
- Trazabilidad: `OrigenCreacion` (Web, TelefonoSecretaria, PresencialSecretaria, Odontologo), `CreadoPor`, y en las cancelaciones `MotivoCancelacion` y `CanceladoPor`.

### Historia clínica y odontograma

- El odontograma distingue `Inicial` (estado de ingreso, con valor pericial) y `Evolutivo` (estado actual).
- Cada `EstadoPieza` referencia opcionalmente la `EntradaHistoriaClinica` que originó el cambio, para conservar la trazabilidad.

### Pagos

- `Cargo` representa la deuda y se asocia opcionalmente a un `Turno`. Un turno puede generar varios cargos.
- `Pago` registra cada movimiento de dinero contra un cargo y referencia opcionalmente a la `Cuota` que cancela.
- `TransaccionPago` registra la operación con la pasarela externa.

### Obras sociales y liquidaciones

- `DetalleLiquidacion` separa `MontoFacturado`, `PorcentajeRetencionClinica`, `MontoLiquidadoProfesional` y `MontoComisionAdministrativa`.
- Invariante: `MontoFacturado = MontoLiquidadoProfesional + retención + comisión`. La distribución se calcula en el dominio y no se carga manualmente.
