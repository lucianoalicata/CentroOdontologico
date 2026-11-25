# Sprint 1 — Planificación y Backlog

**Período:** [fecha de inicio] a [fecha de cierre] (entre 2 y 3 semanas, según la disponibilidad del equipo)

## Contenido

1. [Objetivo del sprint](#1-objetivo-del-sprint)
2. [Decisiones previas](#2-decisiones-previas)
3. [Resumen de carga](#3-resumen-de-carga)
4. [Backlog por módulo](#4-backlog-por-módulo)
5. [Dependencias y orden de trabajo](#5-dependencias-y-orden-de-trabajo)
6. [Configuración del tablero](#6-configuración-del-tablero)
7. [Cierre del sprint](#7-cierre-del-sprint)
8. [Backlog posterior](#8-backlog-posterior)

---

## 1. Objetivo del sprint

> Cada módulo funciona de forma independiente (API y pantallas básicas), con sus reglas de negocio probadas, y queda listo para integrarse en el Sprint 2.

Módulos y responsables:

| Módulo | Responsable |
|---|---|
| Identidad y pacientes, y tareas transversales | Luciano |
| Turnos y agenda | Ignacio |
| Historia clínica y prestaciones | María Belén Castro |
| Odontograma | María Belén Leyva |

**Resultado esperado para la demostración:** con los usuarios de prueba, la secretaría registra un paciente y programa un turno y un entre-turno; el odontólogo carga su disponibilidad; se puede ver y registrar la historia clínica de un paciente y cargar su odontograma. La atención integrada de punta a punta (iniciar un turno y pasar a la historia clínica y al odontograma) se resuelve en el Sprint 2.

---

## 2. Decisiones previas

Deben acordarse en la reunión de inicio, porque condicionan el trabajo de varios módulos.

1. **El odontograma es un módulo propio** (carpeta `Odontograma` en cada capa). Referencia a la historia clínica únicamente por Id (`EntradaHistoriaClinicaId`). Esto permite trabajar en paralelo sin tocar los mismos archivos. Debe agregarse a `convenciones-dominio.md`.
2. **La historia clínica se crea en el primer acceso** (obtener o crear). El módulo de identidad no conoce la historia clínica.
3. **`EntradaHistoriaClinica` es inmutable:** no se modifica ni se elimina. Una corrección es una entrada nueva que referencia a la anterior.
4. **Turno programado exige paciente. El entre-turno admite datos provisorios** (ya documentado en las convenciones).
5. **Las pantallas de historia clínica y odontograma se prueban con una ruta que recibe el id del paciente** (por ejemplo, `/pacientes/:id/historia`), usando los pacientes del seeder. La integración con la agenda llega en el Sprint 2.
6. **Archivos adjuntos:** almacenamiento local en disco para desarrollo, detrás de una interfaz. El almacenamiento definitivo se decide más adelante.
7. **Migraciones de a una y en este orden:** Identidad (domicilio del paciente), Turnos, Historia clínica, Odontograma. Avisar en el grupo antes de generar cada una.
8. **Estimaciones relativas:** S ≈ 2 h, M ≈ 4 h, L ≈ 8 h de trabajo concentrado. Se recalibran con lo que se complete en la primera semana.
9. **Cobertura:** los circuitos relevados en la clínica (obra social con presentación en papel, obra social con plataforma propia y particular) se modelan en el módulo de obras sociales a partir del Sprint 2. Detalle en `docs/relevamiento-circuitos-cobertura.md`.

---

## 3. Resumen de carga

| Responsable | P1 (imprescindible) | P2 (deseable) | P3 (opcional) |
|---|---|---|---|
| Luciano | 40 h | 10 h | — |
| Ignacio | 56 h | — | 4 h |
| María Belén Castro | 38 h | 18 h | — |
| María Belén Leyva | 44 h | — | 4 h |

- **P1** es lo necesario para cumplir el objetivo del sprint. **P2** y **P3** se incorporan si hay tiempo.
- Ignacio tiene la mayor carga: por eso dos pantallas de turnos (S1-22 y S1-23) están asignadas a Luciano. Quien termine antes toma tareas de quien vaya más cargado.

---

## 4. Backlog por módulo

Los títulos siguen la convención de commits. Al crear la *issue*, el título lleva el identificador: `[S1-12] feat(turnos): dominio del turno`.

### 4.1 Transversal — responsable: Luciano

| ID | Tarea | Criterios de aceptación | Tam. | Prior. | Depende de |
|---|---|---|---|---|---|
| S1-01 | `chore(frontend)`: instalar paquetes de Mantine y registrar notificaciones | Instalados `@mantine/form`, `@mantine/dates`, `@mantine/notifications` y `dayjs`; estilos importados y `<Notifications />` en el proveedor; `build` y `lint` pasan. | S | P1 | — |
| S1-02 | `feat(api)`: excepciones de recurso inexistente y acceso denegado | `NoEncontradoException` (404) y `AccesoDenegadoException` (403) en Aplicación, mapeadas en `ManejadorExcepciones`, con pruebas. | S | P1 | — |
| S1-03 | `feat(api)`: políticas de autorización compartidas | `SoloPersonal` (secretaría, odontólogo, administrador), `SoloOdontologo` y `SoloAdministrador`, definidas en un único lugar y documentadas. | S | P1 | — |
| S1-04 | `chore(ci)`: flujo de GitHub Actions | En cada pull request compila, ejecuta las pruebas del backend, y corre `lint` y `build` del frontend; exigido por la protección de ramas. | M | P2 | — |
| S1-05 | `feat(frontend)`: utilidades compartidas de fechas y avisos | Mostrar fechas UTC en hora local y helpers de aviso de éxito y de error; documentados en el README del frontend. | S | P2 | S1-01 |

### 4.2 Identidad y pacientes — responsable: Luciano

| ID | Tarea | Criterios de aceptación | Tam. | Prior. | Depende de |
|---|---|---|---|---|---|
| S1-06 | `feat(pacientes)`: repositorio y búsqueda paginada | `IPacienteRepository` en Dominio e implementación con EF; búsqueda por DNI, apellido o teléfono con paginación. | M | P1 | S1-02 |
| S1-07 | `feat(pacientes)`: registrar paciente | Alta con validaciones; DNI único; si existe un paciente dado de baja con ese DNI, se reactiva en lugar de duplicarse; incorpora el domicilio al perfil del paciente, con su migración; pruebas. | M | P1 | S1-06 |
| S1-08 | `feat(pacientes)`: obtener, actualizar, dar de baja y reactivar | Operaciones con pruebas; la baja es lógica y no afecta datos de otros módulos. | M | P1 | S1-06 |
| S1-09 | `feat(odontologos)`: listado de odontólogos activos | Devuelve id de perfil, nombre, matrícula y especialidad; lo consume la agenda. | S | P1 | — |
| S1-10 | `feat(api)`: endpoints de pacientes y odontólogos | Documentados en OpenAPI, restringidos al personal, con respuestas 400, 401, 403 y 404 correctas. | M | P1 | S1-03, S1-07, S1-08, S1-09 |
| S1-11 | `feat(frontend)`: pantalla de pacientes | Búsqueda, alta rápida y edición; el formulario admite valores iniciales (para "Crear ficha" desde un entre-turno); errores legibles. | L | P1 | S1-01, S1-05, S1-10 |

### 4.3 Turnos y agenda — responsable: Ignacio (salvo indicación)

| ID | Tarea | Criterios de aceptación | Tam. | Prior. | Depende de |
|---|---|---|---|---|---|
| S1-12 | `feat(turnos)`: dominio del turno | `Turno` con tipos Programado y EntreTurno, estados, fábricas de creación y transiciones (iniciar, finalizar, ausente); cancelación con motivo y autor; vinculación de paciente; iniciar exige paciente vinculado; pruebas. | L | P1 | — |
| S1-13 | `feat(turnos)`: dominio de horarios disponibles | `HorarioDisponible` por odontólogo (día, franja y duración del turno) con validación de solapamientos; pruebas. | M | P1 | — |
| S1-14 | `feat(turnos)`: persistencia | Configuraciones EF, migración y repositorios de turnos y horarios. | M | P1 | S1-12, S1-13 |
| S1-15 | `feat(turnos)`: servicio de disponibilidad | Calcula las franjas libres de un odontólogo en una fecha (horarios menos turnos programados activos); pruebas. | L | P1 | S1-13 |
| S1-16 | `feat(turnos)`: programar turno | Valida disponibilidad y paciente; registra origen y autor con `IUsuarioActual`; un paciente solo programa para sí mismo; pruebas. | M | P1 | S1-03, S1-12, S1-14, S1-15 |
| S1-17 | `feat(turnos)`: crear entre-turno | Orden correlativo por día y odontólogo; datos provisorios (nombre, teléfono y DNI opcional); sugiere la ficha existente si el DNI ya está registrado; pruebas. | M | P1 | S1-12, S1-14 |
| S1-18 | `feat(turnos)`: cancelar, iniciar, finalizar, ausente y vincular paciente | Casos de uso con control de pertenencia por rol; cancelar registra motivo y autor; iniciar sin paciente vinculado se rechaza; pruebas. | M | P1 | S1-12, S1-14 |
| S1-19 | `feat(turnos)`: consultar agenda | Agenda por odontólogo y fecha: programados ordenados por hora y entre-turnos al final, por orden; el odontólogo solo ve la suya. | M | P1 | S1-14 |
| S1-20 | `feat(api)`: endpoints de turnos y horarios | Agenda, programar, entre-turno, transiciones y horarios, con autorización por rol y documentación OpenAPI. | L | P1 | S1-16 a S1-19 |
| S1-21 | `feat(frontend)`: agenda del día de recepción | Selector de odontólogo y fecha; programados y entre-turnos diferenciados; acciones de cancelar y marcar ausente. | L | P1 | S1-09, S1-20 |
| S1-22 | `feat(frontend)`: formularios de turno y entre-turno *(Luciano)* | Alta de turno programado (elige una franja libre) y de entre-turno (nombre, teléfono, DNI opcional), con búsqueda o alta rápida de paciente. | L | P1 | S1-11, S1-20 |
| S1-23 | `feat(frontend)`: disponibilidad del odontólogo *(Luciano)* | Cada odontólogo carga y edita sus horarios semanales; se validan los solapamientos. | M | P2 | S1-20 |
| S1-24 | `feat(frontend)`: agenda del odontólogo con inicio de atención | Vista de su día con el botón "Iniciar atención"; si falta ficha, ofrece vincular o crear; abre una pantalla de atención provisoria. | M | P3 | S1-11, S1-20 |

### 4.4 Historia clínica y prestaciones — responsable: María Belén Castro

| ID | Tarea | Criterios de aceptación | Tam. | Prior. | Depende de |
|---|---|---|---|---|---|
| S1-25 | `feat(historia-clinica)`: dominio de prestaciones | `Prestacion` con categoría, duración estimada, precio particular, indicador de autorización previa y baja lógica; pruebas. | S | P1 | — |
| S1-26 | `feat(historia-clinica)`: dominio de historia clínica y entradas | `HistoriaClinica` por paciente y `EntradaHistoriaClinica` inmutable (una corrección es una entrada nueva que referencia a la anterior); comentarios de texto libre y lista de prestaciones realizadas (`PrestacionRealizada`: código, título y precio tal como se informaron en ese momento, pieza y cantidad); alertas médicas y garantía; pruebas. | L | P1 | — |
| S1-27 | `feat(historia-clinica)`: dominio de adjuntos | `ArchivoAdjunto` con tipo y validaciones de formato y tamaño; puerto `IAlmacenamientoArchivos`; pruebas. | M | P2 | — |
| S1-28 | `feat(historia-clinica)`: persistencia | Configuraciones EF, migración y repositorios. | M | P1 | S1-25, S1-26, S1-27 |
| S1-29 | `feat(historia-clinica)`: almacenamiento local de archivos | Implementación en disco para desarrollo detrás de `IAlmacenamientoArchivos`; los archivos no se versionan en Git. | S | P2 | S1-27 |
| S1-30 | `feat(historia-clinica)`: casos de uso de la historia | Obtener o crear la historia al primer acceso; registrar entrada con sus prestaciones realizadas (solo odontólogo, autor desde `IUsuarioActual`); listar la evolución cronológica y el historial de prestaciones; editar alertas; pruebas. | L | P1 | S1-03, S1-26, S1-28 |
| S1-31 | `feat(historia-clinica)`: casos de uso de adjuntos | Adjuntar y descargar archivos vinculados a una entrada, con control de acceso; pruebas. | M | P2 | S1-29, S1-30 |
| S1-32 | `feat(prestaciones)`: casos de uso y datos de ejemplo | Alta, edición y baja (administrador); listado de activas (personal); seeder de desarrollo con un nomenclador de ejemplo; pruebas. | M | P1 | S1-25, S1-28 |
| S1-33 | `feat(api)`: endpoints de historia clínica, adjuntos y prestaciones | Documentados en OpenAPI y con autorización por rol. | M | P1 | S1-30 a S1-32 |
| S1-34 | `feat(frontend)`: ABM de prestaciones | Listado, alta y edición para el administrador. | M | P2 | S1-33 |
| S1-35 | `feat(frontend)`: historia clínica del paciente | Alertas médicas destacadas, evolución cronológica y alta de entradas con selección de prestaciones del catálogo (código y título) y comentarios; ruta con el id del paciente. | L | P1 | S1-33 |
| S1-36 | `feat(frontend)`: adjuntos en la historia clínica | Subir, listar y descargar archivos de una entrada. | M | P2 | S1-31, S1-35 |

### 4.5 Odontograma — responsable: María Belén Leyva

| ID | Tarea | Criterios de aceptación | Tam. | Prior. | Depende de |
|---|---|---|---|---|---|
| S1-37 | `feat(odontograma)`: dominio | Piezas con notación FDI validada, superficies, condiciones, `EstadoPieza` (con su etapa: existente, planificado o realizado) y `Odontograma` de tipo Inicial o Evolutivo; el inicial queda inmutable al cerrarse; pruebas. | L | P1 | — |
| S1-38 | `feat(odontograma)`: persistencia | Configuraciones EF, migración y repositorio. | M | P1 | S1-37 |
| S1-39 | `feat(odontograma)`: casos de uso | Obtener el odontograma (inicial y evolutivo); registrar estado de pieza (solo odontólogo, con vínculo opcional a la entrada de historia clínica); cerrar el inicial; historial de una pieza; pruebas. | L | P1 | S1-03, S1-37, S1-38 |
| S1-40 | `feat(api)`: endpoints del odontograma | Documentados en OpenAPI y con autorización por rol. | M | P1 | S1-39 |
| S1-41 | `feat(frontend)`: componente de odontograma, dibujo | 32 piezas con sus 5 caras en SVG, colores por condición y leyenda, con datos de prueba; sin llamadas a la API. | L | P1 | — |
| S1-42 | `feat(frontend)`: componente de odontograma, interacción | Selección de pieza y cara, panel para cargar condición y observaciones, modo solo lectura; reutilizable mediante props. | L | P1 | S1-41 |
| S1-43 | `feat(frontend)`: odontograma conectado a la API | Cargar y guardar estados, alternar entre inicial y evolutivo, ver el historial de una pieza. | M | P1 | S1-40, S1-42 |
| S1-44 | `feat(frontend)`: dentición temporal | Soporte de piezas temporales (51 a 85) para pacientes pediátricos. | M | P3 | S1-42 |

---

## 5. Dependencias y orden de trabajo

**Día 1 (desbloquean al resto):** S1-01, S1-02 y S1-03 de Luciano. En paralelo, todo lo que no depende de nadie: los dominios (S1-12, S1-13, S1-25, S1-26, S1-27, S1-37) y el dibujo del odontograma (S1-41).

**Ruta crítica:** pacientes y odontólogos (S1-06 a S1-10) habilitan las pantallas de Turnos (S1-21 y S1-22). Hasta que estén disponibles, el frontend de turnos puede trabajar con datos de prueba.

**Persistencia:** las cuatro migraciones (S1-07, S1-14, S1-28, S1-38) se generan de a una, en el orden acordado.

**Integración:** las pantallas de historia clínica y odontograma se prueban con la ruta por id de paciente. La conexión con la agenda ("Iniciar atención") es el núcleo del Sprint 2.

---

## 6. Configuración del tablero

Herramienta: GitHub Projects, asociado al repositorio.

### Estados (columnas)

`Backlog` · `Pendiente` (sprint actual) · `En curso` · `En revisión` · `Terminada`

### Campos

| Campo | Valores |
|---|---|
| Sprint | Iteración (Sprint 0, Sprint 1, ...) |
| Módulo | Transversal, Identidad, Turnos, Historia clínica, Odontograma |
| Capa | Backend, Frontend, Infraestructura |
| Tamaño | S, M, L |
| Prioridad | P1, P2, P3 |
| Responsable | Integrante asignado |

### Etiquetas

- Tipo: `feat`, `fix`, `chore`, `refactor`, `test`, `docs`.
- Módulo: `mod:identidad`, `mod:turnos`, `mod:historia-clinica`, `mod:odontograma`, `mod:transversal`.

### Reglas de funcionamiento

- Máximo **2 tareas "En curso"** por persona.
- Una tarea pasa a "En revisión" al abrir el pull request, que incluye `Closes #<número>` para que la *issue* se cierre sola al integrarse.
- Un pull request en revisión se atiende dentro de las 24 horas.
- Una tarea bloqueada se comenta en la *issue* con el motivo y se avisa en el grupo.
- Las tarjetas terminadas nunca se eliminan: al cerrar el sprint se archivan.

### Vistas recomendadas

- **Tablero:** columnas por estado, filtrado por el sprint actual.
- **Tabla por módulo:** agrupada por módulo, con tamaño y prioridad.
- **Hoja de ruta:** iteraciones en el tiempo.

### Plantilla de issue

Guardar en `.github/ISSUE_TEMPLATE/tarea.md`:

```markdown
---
name: Tarea
about: Tarea del sprint
---

## Contexto
<!-- Por qué se necesita -->

## Criterios de aceptación
- [ ]

## Dependencias
<!-- IDs o enlaces a otras tareas -->

## Notas técnicas
<!-- Decisiones, archivos o módulos involucrados -->
```

---

## 7. Cierre del sprint

1. **Demostración** de cada módulo con los usuarios de prueba.
2. **Retrospectiva** breve: qué funcionó, qué no y qué se ajusta.
3. **Informe** en `docs/sprints/sprint-1.md` (objetivo, tareas completadas, pendientes y aprendizajes).
4. **Archivar** las tarjetas terminadas y pasar lo no completado al sprint siguiente.
5. **Etiquetar** la versión: `sprint-1-completo`.

---

## 8. Backlog posterior

Candidatas para el Sprint 2 y siguientes, a ordenar según lo aprendido:

- Atención integrada: iniciar un turno y abrir la historia clínica y el odontograma del paciente.
- Activar la cuenta web de un paciente existente.
- Gestión de personal por el administrador (altas, roles y bajas).
- Portal del paciente: turnos, historial y odontogramas.
- Reprogramación de turnos.
- Presupuestos y cuenta corriente para pacientes particulares (subtotal, saldado y deuda).
- Cargos y pagos manuales (señas, saldos y cuotas).
- Obras sociales según los circuitos relevados: registro de obras sociales con su modalidad de presentación, afiliaciones y prestaciones por cobertura.
- Planilla imprimible para presentar al Círculo Odontológico y seguimiento mensual de lo presentado, cobrado y rechazado.
- Pasarela de pago online.
- Notificaciones y recordatorios.
- Reportes y tablero de indicadores.
