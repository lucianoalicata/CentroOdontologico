# Guía de Contribución y Flujo de Trabajo (Git Workflow)

Este documento establece las pautas obligatorias de trabajo en equipo para el desarrollo de **SIGO (Centro Odontológico)**. El objetivo es mantener el historial limpio, evitar conflictos de integración y desacoplar las tareas mediante ramas cortas y verificables.

---

## 1. Estrategia de Ramas (Git Branching)

El repositorio utiliza una estrategia basada en ramas troncales y de características:

* **`master` / `main`:** Código de producción estable, probado y listo para demostración.
* **`development`:** Rama principal de integración continua. Todo cambio aprobado entra primero acá.
* **Ramas de trabajo:** Se desprenden siempre de `development` y deben nombrarse según el tipo de tarea:
  * `feature/<nombre-tarea>`: Nuevas funcionalidades (ej. `feature/agenda-turnos-secretaria`).
  * `chore/<nombre-tarea>`: Tareas de arquitectura, configuración o tooling (ej. `chore/sprint-0-fundaciones`).
  * `fix/<nombre-error>`: Correcciones de bugs sobre `development` (ej. `fix/calculo-entre-turno`).
  * `refactor/<nombre-mejora>`: Reestructuración de código sin alterar comportamiento.

---

## 2. Convención de Commits (Conventional Commits)

Cada mensaje de commit debe comenzar con un prefijo semántico en minúsculas, seguido de dos puntos y un resumen imperativo claro. Si resuelve un Issue del tablero, se añade el número al final.

Formato: `<tipo>: <descripción breve en presente> (#<numero_issue>)`

### Tipos de commit admitidos:

* **`feat:`** Una funcionalidad nueva visible para el usuario o la API.
  * *Ejemplo:* `feat: registrar nuevo entre-turno con orden correlativo (#12)`
* **`fix:`** Corrección de un error o comportamiento defectuoso.
  * *Ejemplo:* `fix: corregir solapamiento de horarios en agenda de odontologo (#18)`
* **`chore:`** Tareas de mantenimiento, actualización de dependencias, configuración de build o estructura de carpetas que no modifican la lógica de negocio.
  * *Ejemplo:* `chore: separar solucion en proyectos de clean architecture (#1)`
* **`refactor:`** Cambio de código interno que no agrega funcionalidad ni arregla bugs (renombrar clases, simplificar métodos, extraer abstracciones).
  * *Ejemplo:* `refactor: desacoplar usuario en entidad base y perfiles de rol (#4)`
* **`test:`** Añadir, corregir o refactorizar pruebas unitarias o de integración.
  * *Ejemplo:* `test: agregar casos de prueba unitarios para reglas de turnos (#7)`
* **`docs:`** Cambios exclusivos en documentación (Markdown, Swagger annotations, diagramas).
  * *Ejemplo:* `docs: documentar guia de contribucion y ramas (#9)`

---

## 3. Protocolo de Pull Requests (PRs)

1. **Sincronización previa:** Antes de solicitar revisión, actualizá tu rama local con los últimos cambios de `development`:
   ```bash
   git checkout development
   git pull origin development
   git checkout tu-rama
   git merge development