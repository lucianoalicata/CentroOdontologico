import { useState } from 'react'
import { IconPlus } from '@tabler/icons-react'
import { Stack } from '@mantine/core'
import { usePacientes } from '../../application/pacientes/usePacientes'
import { CabeceraPagina } from '../../ui/molecules/CabeceraPagina/CabeceraPagina'
import { TablaPacientes } from '../../ui/organisms/TablaPacientes/TablaPacientes'
import { PacienteFormModal } from '../../ui/organisms/PacienteFormModal/PacienteFormModal'
import type { PacienteDto } from '../../domain/pacientes/types'

/**
 * Page: listado de pacientes.
 * Responsabilidad única: componer organisms y conectar con el hook de caso de uso.
 * No contiene lógica de negocio ni llamadas HTTP directas.
 */
export function PacientesPage() {
  const {
    pacientes,
    cargando,
    filtro,
    pagina,
    totalPaginas,
    totalItems,
    cambiarFiltro,
    cambiarPagina,
    forzarRecarga,
    darDeBaja,
  } = usePacientes()

  const [modalAbierto, setModalAbierto] = useState(false)
  const [pacienteEditar, setPacienteEditar] = useState<PacienteDto | null>(null)

  function abrirNuevo() {
    setPacienteEditar(null)
    setModalAbierto(true)
  }

  function abrirEditar(paciente: PacienteDto) {
    setPacienteEditar(paciente)
    setModalAbierto(true)
  }

  async function confirmarBaja(paciente: PacienteDto) {
    const confirmado = window.confirm(
      `¿Confirma que desea dar de baja al paciente ${paciente.apellido}, ${paciente.nombre} (DNI ${paciente.dni})?`,
    )
    if (confirmado) await darDeBaja(paciente)
  }

  return (
    <Stack>
      <CabeceraPagina
        titulo="Pacientes"
        descripcion={`Padrón de pacientes registrados (${totalItems} en total)`}
        etiquetaAccion="Nuevo paciente"
        iconoAccion={<IconPlus size={16} />}
        onAccion={abrirNuevo}
      />

      <TablaPacientes
        pacientes={pacientes}
        cargando={cargando}
        filtro={filtro}
        pagina={pagina}
        totalPaginas={totalPaginas}
        onCambiarFiltro={cambiarFiltro}
        onCambiarPagina={cambiarPagina}
        onEditar={abrirEditar}
        onDarDeBaja={confirmarBaja}
      />

      <PacienteFormModal
        abierto={modalAbierto}
        onCerrar={() => setModalAbierto(false)}
        onGuardado={forzarRecarga}
        pacienteEditar={pacienteEditar}
      />
    </Stack>
  )
}
