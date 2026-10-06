# UNIVERSIDAD TECNOLÓGICA NACIONAL
## Facultad Regional Tucumán
### Proyecto Final Integrador — Seminario Integrador

---

# Sistema Integral de Gestión Odontológica

Plataforma integral orientada a la centralización, trazabilidad y digitalización operativa, clínica y contable de centros odontológicos. El sistema sustituye los registros físicos y planillas aisladas por un ecosistema unificado que conecta la atención médica, la autogestión de turnos y la liquidación con prestadores de salud.

---

## 📌 Contexto y Problemática

El flujo tradicional en clínicas y consultorios odontológicos suele depender de soporte en papel o sistemas fragmentados. Esta modalidad acarrea:
* Ausentismo de pacientes por falta de recordatorios sistemáticos.
* Demoras en sala de espera y fricción en la asignación de sobreturnos.
* Riesgo de pérdida o manipulación de fichas clínicas y evoluciones médicas.
* Inconsistencias en el cobro particular y débitos en liquidaciones con obras sociales por falta de validación del nomenclador odontológico.

El **Sistema** aborda estas problemáticas mediante una solución robusta, trazable y auditable para todas las partes intervinientes.

---

## 🎯 Objetivos del Sistema

* **Centralizar la administración de turnos:** Implementar un motor de reservas inteligente que sincronice en tiempo real las agendas de los odontólogos, la demanda espontánea (*walk-in*) y la autogestión del paciente.
* **Modernizar el registro clínico:** Digitalizar la historia clínica con un odontograma interactivo bajo notación internacional FDI, garantizando inmutabilidad y auditoría de diagnósticos.
* **Asegurar la trazabilidad económica:** Separar el concepto de deuda clínica del cobro efectivo, permitiendo señas, copagos, planes de cuotas e integración con pasarelas de pago digitales.
* **Optimizar la gestión con Obras Sociales:** Sistematizar el nomenclador de prestaciones, control de autorizaciones y conformación de lotes de liquidación profesional.

---

## 👥 Perfiles y Módulos Principales

### 🧑‍⚕️ Módulo Odontológico (Clínico)
* **Odontograma Interactivo:** Registro visual por pieza dental y cara (oclusal, mesial, distal, vestibular, lingual), diferenciando tratamientos existentes, patologías activas y procedimientos finalizados.
* **Historia Clínica Digital:** Evolución médica cronológica, registro de diagnósticos, carga de radiografías/estudios complementarios y control de garantías de tratamientos.
* **Gestión de Agenda Profesional:** Configuración de disponibilidad semanal, bloqueo por ausencias y visualización de pacientes en espera.

### 📋 Módulo de Secretaría y Recepción
* **Control de Recepción:** Admisión ágil de pacientes citados, atención de sobreturnos espontáneos y control de salas.
* **Gestión de Obras Sociales:** Consulta de coberturas médicas, verificación de requisitos de autorización previa y generación de lotes para cobro mensual.
* **Auditoría Administrativa:** Seguimiento del circuito de cobranzas y liquidación de comisiones u honorarios médicos.

### 👤 Portal del Paciente (Autogestión)
* Solicitud y reprogramación de turnos online según especialidad y profesional.
* Acceso seguro al historial de tratamientos e informes médicos en PDF.
* Consulta del estado de cuenta corriente, pago de señas y saldos vía pasarela electrónica.

---

## 🛠️ Arquitectura y Tecnologías

El sistema sigue una arquitectura desacoplada **Cliente-Servidor** estructurada bajo los principios de **Clean Architecture** (Arquitectura Limpia) en un enfoque de **Monolito Modular**, asegurando alta cohesión interna, bajo acoplamiento entre dominios y escalabilidad a futuro.

* **Backend:** C# / .NET (ASP.NET Core Web API)
* **Persistencia:** Entity Framework Core
* **Base de Datos:** SQL Server / PostgreSQL (Esquema relacional con integridad transaccional estricta)
* **Frontend:** Single Page Application (React / Web Responsive)
* **Integraciones Externas:** Pasarelas de pago online (Mercado Pago SDK) y servicios de mensajería para confirmación de turnos.

---

## 🚀 Pautas de Trabajo para el Equipo

Para garantizar un desarrollo coordinado entre los integrantes del proyecto:

1. **Ramas Principales:**
   * `main`: Código probado, documentado y listo para entregas a la cátedra.
   * `develop`: Integración activa del avance semanal.
2. **Convención de Ramas:**
   * Cada nueva funcionalidad se desarrolla a partir de una rama propia derivada de `develop`:  
     `feature/turnos`, `feature/odontograma`, `feature/obras-sociales`, `feature/pagos`.
3. **Control de Cambios:**
   * Toda incorporación a la rama principal se realiza mediante *Pull Request* revisado por al menos un integrante del equipo.

---

## 👥 Equipo de Proyecto
+ Alicata, Luciano Antonio
+ Castro, María Belén
+ Fernández, Ignacio
+ Leyva, María Belén
* Universidad Tecnológica Nacional — Facultad Regional Tucumán
* Seminario Integrador - 2026