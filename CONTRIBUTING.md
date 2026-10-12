# Guía de Contribución

Este documento establece las pautas de trabajo en equipo para el desarrollo de **Sistema Integral de Gestión Odontológica**. Su objetivo es mantener un historial limpio, reducir los conflictos de integración y asegurar que cada cambio sea pequeño, revisado y verificable.

## Contenido

1. [Estrategia de ramas](#1-estrategia-de-ramas)
2. [Convención de commits](#2-convención-de-commits)
3. [Pull requests](#3-pull-requests)
4. [Revisión de código](#4-revisión-de-código)
5. [Definición de terminado](#5-definición-de-terminado)
6. [Migraciones de base de datos](#6-migraciones-de-base-de-datos)
7. [Configuración local y secretos](#7-configuración-local-y-secretos)
8. [Tablero de tareas](#8-tablero-de-tareas)
9. [Versiones y entregas](#9-versiones-y-entregas)
10. [Plantilla de pull request](#10-plantilla-de-pull-request)

---

## 1. Estrategia de ramas

| Rama | Propósito |
|---|---|
| `main` | Código estable, probado y documentado. Solo recibe cambios desde `development` en cada entrega o hito. |
| `development` | Rama de integración. Todo cambio aprobado ingresa primero aquí. |
| `<tipo>/<descripción>` | Ramas de trabajo. Se crean siempre a partir de `development` y tienen vida corta (idealmente de 1 a 3 días). |

### Nombres de ramas de trabajo

El prefijo coincide con el tipo de commit. El nombre se escribe en minúsculas, con guiones, e incluye el identificador de la tarea cuando existe.

| Prefijo | Uso | Ejemplo |
|---|---|---|
| `feat/` | Funcionalidad nueva | `feat/s1-pacientes-registro` |
| `fix/` | Corrección de errores | `fix/s1-orden-entre-turno` |
| `chore/` | Configuración, estructura, dependencias | `chore/sprint-0-fundaciones` |
| `refactor/` | Cambios internos sin alterar el comportamiento | `refactor/s1-repositorio-turnos` |
| `test/` | Pruebas | `test/s1-reglas-agenda` |
| `docs/` | Documentación | `docs/guia-contribucion` |

### Reglas

- No se realizan *push* directos a `main` ni a `development`. Ambas ramas deben tener protección activa en el repositorio y exigir *pull request* con al menos una aprobación.
- No se reescribe el historial (`force push`) en ramas compartidas.
- Una rama contiene una sola tarea. Si surge trabajo adicional, se crea otra rama.

---

## 2. Convención de commits

Se utiliza [Conventional Commits](https://www.conventionalcommits.org/lang/es/). Formato:

```
<tipo>[(<módulo>)]: <descripción breve> [(#<issue>)]
```

- Descripción en minúsculas, en modo imperativo, sin punto final y de hasta 72 caracteres.
- El módulo es opcional y refiere al área afectada (`auth`, `turnos`, `pacientes`, `odontograma`).
- Si el commit resuelve una tarea del tablero, se agrega el número al final.

### Tipos admitidos

| Tipo | Descripción | Ejemplo |
|---|---|---|
| `feat` | Funcionalidad nueva visible en la aplicación o en la API | `feat(turnos): registrar entre-turno con orden correlativo (#12)` |
| `fix` | Corrección de un error | `fix(turnos): corregir solapamiento en la agenda del odontólogo (#18)` |
| `chore` | Mantenimiento, dependencias, estructura o configuración | `chore: separar la solución en capas de arquitectura limpia (#1)` |
| `refactor` | Cambio interno que no agrega funcionalidad ni corrige errores | `refactor(identidad): reemplazar rol único por composición de roles (#4)` |
| `test` | Alta o modificación de pruebas | `test(auth): agregar casos de prueba del servicio de autenticación (#7)` |
| `docs` | Cambios exclusivos de documentación | `docs: actualizar convenciones de dominio (#9)` |

---

## 3. Pull requests

1. **Sincronizar la rama** con los últimos cambios de `development`:

   ```bash
   git checkout development
   git pull origin development
   git checkout <tu-rama>
   git merge development
   ```

2. **Verificar localmente** que la solución compila y que las pruebas pasan:

   ```bash
   dotnet build
   dotnet test
   ```

3. **Abrir el pull request** hacia `development` completando la [plantilla](#10-plantilla-de-pull-request). El título sigue la convención de commits.
4. **Solicitar revisión** a otro integrante. El autor no aprueba su propio *pull request*.
5. **Resolver los comentarios** con nuevos commits en la misma rama.
6. **Integrar** mediante *Squash and merge*, de modo que cada tarea quede como un único commit en `development`. Al finalizar, se elimina la rama.

### Tamaño de los cambios

Se prefieren *pull requests* pequeños y enfocados, con menos de 400 líneas modificadas cuando sea posible. Los cambios extensos dificultan la revisión y aumentan el riesgo de conflictos.

### Integración hacia `main`

La incorporación de `development` a `main` se realiza únicamente al cierre de un sprint o ante una entrega, mediante *pull request* con *merge commit*, seguido de una etiqueta de versión (ver [sección 9](#9-versiones-y-entregas)).

---

## 4. Revisión de código

Quien revisa verifica, como mínimo:

- El cambio resuelve la tarea indicada y nada más.
- Se respetan las [convenciones de dominio y arquitectura](docs/convenciones-dominio.md): capas, dependencias entre módulos, visibilidad `public`, setters privados y validaciones en el dominio.
- Las reglas de negocio nuevas cuentan con pruebas.
- Los controllers no contienen lógica de negocio.
- No se incluyen secretos, contraseñas ni cadenas de conexión reales.
- Los nombres son claros y consistentes con el resto del código.
- Si hay migraciones, se cumple el [protocolo correspondiente](#6-migraciones-de-base-de-datos).

Los comentarios deben ser concretos y respetuosos, y se limitan al código, no a la persona.

---

## 5. Definición de terminado

Una tarea se considera terminada cuando se cumplen todos los puntos siguientes:

- [ ] La solución compila sin errores y las pruebas pasan.
- [ ] Las reglas de negocio nuevas tienen pruebas con nombres descriptivos.
- [ ] El *pull request* fue revisado y aprobado por otro integrante.
- [ ] No se rompe ninguna funcionalidad existente.
- [ ] Los endpoints nuevos están documentados en OpenAPI y se probaron manualmente con los usuarios de prueba.
- [ ] La documentación afectada (README, convenciones) está actualizada.
- [ ] La tarea está integrada en `development` y cerrada en el tablero.

---

## 6. Migraciones de base de datos

Las migraciones son la principal fuente de conflictos en un equipo. Se aplica el siguiente protocolo:

1. Ejecutar `git pull origin development` **antes** de crear una migración.
2. Usar un nombre descriptivo, por ejemplo `AgregarHorariosDisponibles`.
3. Si al abrir el *pull request* aparece un conflicto en `ClinicaDbContextModelSnapshot.cs`: eliminar la migración propia, actualizar la rama con `development` y volver a generarla. El archivo *snapshot* no se resuelve a mano.
4. Los *pull requests* que incluyen migraciones se integran de a uno.
5. Una migración ya integrada en `development` nunca se modifica: se crea una nueva.

Más detalles en [`docs/convenciones-dominio.md`](docs/convenciones-dominio.md#4-persistencia-con-ef-core).

---

## 7. Configuración local y secretos

- Ningún secreto se versiona: claves de firma, contraseñas y cadenas de conexión se guardan con `dotnet user-secrets` o variables de entorno.
- `appsettings.json` y `appsettings.Development.json` solo contienen valores no sensibles.
- Los pasos de configuración del entorno están en el [README](README.md#7-puesta-en-marcha).

---

## 8. Tablero de tareas

- Cada tarea tiene una *issue* con un identificador de sprint y número (por ejemplo, `S1-03`), un responsable y una descripción con criterios de aceptación.
- Las *issues* se etiquetan por sprint y por módulo.
- Estados del tablero: `Pendiente` · `En curso` · `En revisión` · `Terminada`.
- El *pull request* referencia la *issue* correspondiente.

---

## 9. Versiones y entregas

Los hitos se marcan con etiquetas sobre `main`:

| Etiqueta | Significado |
|---|---|
| `sprint-<n>-completo` | Cierre de un sprint |
| `entrega-<n>` | Presentación ante la cátedra |

```bash
git tag -a sprint-0-completo -m "Cierre del Sprint 0"
git push origin sprint-0-completo
```

---

## 10. Plantilla de pull request

Se recomienda guardarla en `.github/pull_request_template.md` para que se cargue automáticamente.

```markdown
## Descripción
<!-- Qué cambia y por qué -->

## Tarea relacionada
Cierra #

## Cambios principales
-

## Cómo probarlo
1.

## Lista de verificación
- [ ] Compila y las pruebas pasan
- [ ] Incluye pruebas de las reglas nuevas
- [ ] Respeta las convenciones del proyecto
- [ ] Incluye migración (si aplica) siguiendo el protocolo
- [ ] Documentación actualizada (si aplica)
```
