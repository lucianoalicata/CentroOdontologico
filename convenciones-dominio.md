# Convenciones de Dominio y Arquitectura

Reglas del equipo para mantener el código consistente entre módulos.s

---

## 1. Estructura de la solución

```
/src
  Clinica.Dominio          Entidades, reglas de negocio, interfaces. No depende de nadie.
  Clinica.Aplicacion       Casos de uso, DTOs. Depende de Dominio.
  Clinica.Infraestructura  EF Core, repositorios, servicios externos. Depende de Dominio y Aplicacion.
  Clinica.Api              Controllers, auth, Program.cs. Depende de Aplicacion e Infraestructura.
/tests
  Clinica.Tests            xUnit. Reglas de dominio y casos de uso.
/frontend
/docs
```

### Regla de dependencias

```
Api → Aplicacion → Dominio
Api → Infraestructura → Aplicacion → Dominio
```

- `Dominio` **nunca** referencia a EF Core, ASP.NET ni a ninguna librería de infraestructura.
- Si el dominio necesita algo externo (guardar, enviar un mail), define una **interfaz** (`ITurnoRepository`) y la implementa `Infraestructura`.

### Módulos

Cada capa se organiza en carpetas **por módulo**, no por tipo de archivo:

```
Clinica.Dominio/
  Comun/
  Identidad/
  Turnos/
  HistoriaClinica/
  ObrasSociales/
  Pagos/
  Notificaciones/
```

Cada integrante trabaja dentro de las carpetas de su módulo.

---

## 2. Entidades

### Clase base

Todas las entidades heredan de `Entidad` (`Clinica.Dominio.Comun`), que aporta:

- `Id` (`int`, autoincremental)
- `FechaCreacion` y `FechaModificacion` (UTC, las completa el `DbContext` al guardar)

Excepciones (no heredan): tablas puente con clave compuesta (ej. `UsuarioRol`).

Solo las entidades que heredan de `EntidadConBorradoLogico` se dan de baja. 
Los datos clínicos y financieros nunca se borran: se corrigen con una entrada nueva o se anulan.

### Plantilla

```csharp
public class Turno : Entidad
{
    public int OdontologoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public EstadoTurno Estado { get; private set; }

    protected Turno() { }   // solo para EF Core

    public Turno(int odontologoId, DateOnly fecha)
    {
        OdontologoId = Guard.IdValido(odontologoId, nameof(OdontologoId));
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
2. **Dos constructores:** uno `protected` sin parámetros (EF Core) y uno público que valida.
3. **Las validaciones viven en la entidad** y lanzan `DomainException`. Usar `Guard` para las comunes (`NoVacio`, `IdValido`, `NoNegativo`).
4. **Fechas siempre en UTC** (`DateTime.UtcNow`). La conversión a hora local se hace solo en el frontend. Para fechas sin hora (`Turno.Fecha`) usar `DateOnly`.
5. **Colecciones** expuestas como `IReadOnlyCollection<T>` con campo privado, para no agregar elementos sin pasar por un método de la entidad.
6. **Enums** en su propio archivo, dentro de la carpeta del módulo.
7. **Dinero:** `decimal`, nunca `double` ni `float`.

---

## 3. Relaciones entre módulos

**Un módulo referencia a las entidades de otro solo por Id.**

```csharp
// Correcto
public int PacienteId { get; private set; }

// Incorrecto (acopla Turnos con Identidad)
public Paciente Paciente { get; private set; }
```

- Sin propiedades de navegación entre módulos distintos.
- Dentro de un mismo módulo sí se permiten (`HistoriaClinica` → `EntradaHistoriaClinica`).
- Si un módulo necesita datos de otro, los pide mediante una interfaz de lectura definida en su propia capa de `Aplicacion`.

Esta regla permite que el sistema evolucione (o se separe en servicios) sin reescribir todo.

---

## 4. EF Core

- Un único `ClinicaDbContext`.
- Cada entidad tiene su propia clase `IEntityTypeConfiguration<T>` dentro de la carpeta de su módulo en `Infraestructura`. Se cargan solas con `ApplyConfigurationsFromAssembly`; nadie edita un archivo central.
- No usar atributos de EF (`[Key]`, `[Required]`) en el dominio: se configura con Fluent API en la clase de configuración.
- No usar *lazy loading*.

### Migraciones

1. Hacer `git pull origin development` **antes** de crear una migración.
2. Nombre descriptivo: `AgregarHorariosDisponibles`, no `Migration1`.
3. Si hay conflicto en `ClinicaDbContextModelSnapshot.cs` al hacer PR: **borrar tu migración, traer lo último de `development` y volver a crearla**. No resolver el snapshot a mano.
4. Los PRs que traen migraciones se mergean de a uno.

---

## 5. Nombres

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases, enums, métodos | PascalCase, en español | `HorarioDisponible`, `Cancelar()` |
| Interfaces | Prefijo `I` | `ITurnoRepository` |
| Casos de uso | Verbo + sustantivo + `UseCase` | `SacarTurnoUseCase` |
| DTOs | Sufijo `Request` / `Response` | `CrearTurnoRequest` |
| Configuración EF | Entidad + `Configuration` | `TurnoConfiguration` |
| Tablas | Plural, en español | `Turnos`, `Pacientes` |

El código de infraestructura y los términos técnicos pueden quedar en inglés (`Repository`, `Guard`, `Controller`); el lenguaje del negocio va en español.

---

## 6. Errores

- El dominio lanza `DomainException` con un mensaje legible para el usuario.
- La API la traduce a `400 Bad Request` con un middleware (no capturar excepciones en cada controller).
- Un recurso inexistente es `404`; no se usa `DomainException` para eso.

---

## 7. Tests

- Toda regla de negocio nueva lleva al menos un test en `Clinica.Tests`.
- Nombre: `Metodo_Cuando_Entonces`, por ejemplo `Iniciar_CuandoNoEstaPendiente_EntoncesLanzaDomainException`.
- Los nombres de los tests deben ser legibles y describir la regla que prueban. No usar `Test1`, `Test2`, etc.
- No importa que los nombres sean largos, lo importante es que sean claros.
- Los tests de dominio **no** usan base de datos.

---

## 8. Controllers

- Finos: reciben el request, llaman a un caso de uso, devuelven el resultado.
- Sin lógica de negocio en el controller. Si hay un `if` sobre reglas del negocio, va en el dominio o en el caso de uso.
- Documentados con Swagger (los DTOs ya alcanzan para que se genere solo).