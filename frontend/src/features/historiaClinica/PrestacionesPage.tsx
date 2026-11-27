import { useState } from 'react'
import { IconPlus } from '@tabler/icons-react'
import { Stack } from '@mantine/core'
import { usePrestacionesAdmin } from '../../application/prestaciones/usePrestacionesAdmin'
import { CabeceraPagina } from '../../ui/molecules/CabeceraPagina/CabeceraPagina'
import { PrestacionFormModal } from '../../ui/organisms/PrestacionFormModal/PrestacionFormModal'
import { TablaPrestaciones } from '../../ui/organisms/TablaPrestaciones/TablaPrestaciones'
import type { PrestacionDto } from '../../domain/prestaciones/types'

/**
 * Page: ABM del catálogo de prestaciones (solo administrador).
 * Compone organisms y los conecta con los hooks de caso de uso.
 */
export function PrestacionesPage() {
  const {
    prestaciones,
    totalActivas,
    cargando,
    filtro,
    categoria,
    mostrarInactivas,
    cambiarFiltro,
    cambiarCategoria,
    cambiarMostrarInactivas,
    forzarRecarga,
    darDeBaja,
    reactivar,
  } = usePrestacionesAdmin()

  const [modalAbierto, setModalAbierto] = useState(false)
  const [prestacionEditar, setPrestacionEditar] = useState<PrestacionDto | null>(null)

  function abrirNueva() {
    setPrestacionEditar(null)
    setModalAbierto(true)
  }

  function abrirEditar(prestacion: PrestacionDto) {
    setPrestacionEditar(prestacion)
    setModalAbierto(true)
  }

  async function confirmarBaja(prestacion: PrestacionDto) {
    const confirmado = window.confirm(
      `¿Confirma que desea dar de baja la prestación ${prestacion.codigo} - ${prestacion.titulo}? ` +
        'Dejará de ofrecerse al registrar atenciones, pero el historial no se modifica.',
    )
    if (confirmado) await darDeBaja(prestacion)
  }

  return (
    <Stack>
      <CabeceraPagina
        titulo="Prestaciones"
        descripcion={`Nomenclador de la clínica (${totalActivas} activas)`}
        etiquetaAccion="Nueva prestación"
        iconoAccion={<IconPlus size={16} />}
        onAccion={abrirNueva}
      />

      <TablaPrestaciones
        prestaciones={prestaciones}
        cargando={cargando}
        filtro={filtro}
        categoria={categoria}
        mostrarInactivas={mostrarInactivas}
        onCambiarFiltro={cambiarFiltro}
        onCambiarCategoria={cambiarCategoria}
        onCambiarMostrarInactivas={cambiarMostrarInactivas}
        onEditar={abrirEditar}
        onDarDeBaja={confirmarBaja}
        onReactivar={reactivar}
      />

      <PrestacionFormModal
        abierto={modalAbierto}
        onCerrar={() => setModalAbierto(false)}
        onGuardado={forzarRecarga}
        prestacionEditar={prestacionEditar}
      />
    </Stack>
  )
}
