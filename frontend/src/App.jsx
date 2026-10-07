import { useState, useEffect } from 'react';

// URL base de tu backend .NET
const API_URL = 'http://localhost:5169/api';
export default function App() {
    const [usuarios, setUsuarios] = useState([]);
    const [odontologos, setOdontologos] = useState([]);
    const [usuarioActual, setUsuarioActual] = useState(null);

    // Estados comunes
    const hoyStr = new Date().toISOString().split('T')[0];
    const [fechaSeleccionada, setFechaSeleccionada] = useState(hoyStr);
    const [odontologoSeleccionadoId, setOdontologoSeleccionadoId] = useState('');
    const [agenda, setAgenda] = useState({ programados: [], entreTurnos: [] });

    // Formulario secretaria
    const [form, setForm] = useState({
        pacienteNombre: '',
        pacienteTelefono: '',
        horaInicio: '10:00',
        horaFin: '10:30',
        esEntreTurno: false,
        notas: ''
    });

    // Cargar usuarios y odontólogos al iniciar
    useEffect(() => {
        fetch(`${API_URL}/usuarios`)
            .then(res => res.json())
            .then(data => {
                setUsuarios(data);
                if (data.length > 0) setUsuarioActual(data[0]);
            })
            .catch(err => console.error("Error al cargar usuarios:", err));

        fetch(`${API_URL}/odontologos`)
            .then(res => res.json())
            .then(data => {
                setOdontologos(data);
                if (data.length > 0) setOdontologoSeleccionadoId(data[0].id);
            })
            .catch(err => console.error("Error al cargar odontólogos:", err));
    }, []);

    // Recargar turnos
    const cargarTurnos = (odontoId, fecha) => {
        if (!odontoId) return;
        fetch(`${API_URL}/turnos?odontologoId=${odontoId}&fecha=${fecha}`)
            .then(res => res.json())
            .then(data => {
                setAgenda({
                    programados: data.programados || [],
                    entreTurnos: data.entreTurnos || []
                });
            })
            .catch(err => console.error("Error al cargar turnos:", err));
    };

    useEffect(() => {
        const targetOdontoId = usuarioActual?.rol === 'Odontologo'
            ? usuarioActual.id
            : odontologoSeleccionadoId;

        if (targetOdontoId) {
            cargarTurnos(targetOdontoId, fechaSeleccionada);
        }
    }, [usuarioActual, odontologoSeleccionadoId, fechaSeleccionada]);

    // Manejar creación de turno
    const handleCrearTurno = async (e) => {
        e.preventDefault();
        if (!form.pacienteNombre) return alert("Ingrese el nombre del paciente");

        const payload = {
            pacienteNombre: form.pacienteNombre,
            pacienteTelefono: form.pacienteTelefono,
            odontologoId: parseInt(odontologoSeleccionadoId),
            fecha: fechaSeleccionada,
            horaInicio: form.esEntreTurno ? null : form.horaInicio,
            horaFin: form.esEntreTurno ? null : form.horaFin,
            esEntreTurno: form.esEntreTurno,
            creadoPor: usuarioActual.id,
            notas: form.notas
        };

        const res = await fetch(`${API_URL}/turnos`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            alert("Turno registrado exitosamente");
            setForm({ ...form, pacienteNombre: '', pacienteTelefono: '', notas: '' });
            cargarTurnos(odontologoSeleccionadoId, fechaSeleccionada);
        } else {
            const err = await res.text();
            alert(`Error: ${err}`);
        }
    };

    // Cambiar estado de turno
    const cambiarEstado = async (turnoId, accion) => {
        const res = await fetch(`${API_URL}/turnos/${turnoId}/${accion}`, { method: 'PATCH' });
        if (res.ok) {
            const targetOdontoId = usuarioActual.rol === 'Odontologo' ? usuarioActual.id : odontologoSeleccionadoId;
            cargarTurnos(targetOdontoId, fechaSeleccionada);
        } else {
            const err = await res.text();
            alert(`Error: ${err}`);
        }
    };

    const getEstadoBadge = (estado) => {
        switch (estado) {
            case 1: return <span style={{ color: '#d97706', fontWeight: 'bold' }}>Pendiente</span>;
            case 2: return <span style={{ color: '#2563eb', fontWeight: 'bold' }}>En Curso</span>;
            case 3: return <span style={{ color: '#16a34a', fontWeight: 'bold' }}>Finalizado</span>;
            case 4: return <span style={{ color: '#dc2626', fontWeight: 'bold' }}>Cancelado</span>;
            default: return null;
        }
    };

    return (
        <div style={{ fontFamily: 'system-ui, sans-serif', padding: '24px', maxWidth: '1000px', margin: '0 auto', color: '#1f2937' }}>
            <header style={{ borderBottom: '2px solid #e5e7eb', paddingBottom: '16px', marginBottom: '24px' }}>
                <h1 style={{ margin: 0, color: '#111827' }}>Centro Odontológico — Dra. Marta Zamudio</h1>
                <p style={{ margin: '4px 0 16px 0', color: '#6b7280' }}>Demostración de flujo operativo: Recepción y Consultorio</p>

                {/* Selector de Rol/Usuario simulado */}
                <div style={{ background: '#f3f4f6', padding: '12px', borderRadius: '8px', display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <strong>Sesión activa:</strong>
                    <select
                        value={usuarioActual?.id || ''}
                        onChange={(e) => setUsuarioActual(usuarios.find(u => u.id === parseInt(e.target.value)))}
                        style={{ padding: '6px 12px', borderRadius: '6px', border: '1px solid #d1d5db' }}
                    >
                        {usuarios.map(u => (
                            <option key={u.id} value={u.id}>
                                {u.nombre} {u.apellido} ({u.rol})
                            </option>
                        ))}
                    </select>
                </div>
            </header>

            {/* VISTA SECRETARÍA */}
            {usuarioActual?.rol === 'Secretaria' && (
                <div>
                    <h2>Panel de Secretaría</h2>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px' }}>

                        {/* Formulario nuevo turno */}
                        <form onSubmit={handleCrearTurno} style={{ background: '#fafafa', padding: '20px', borderRadius: '8px', border: '1px solid #e5e7eb' }}>
                            <h3 style={{ marginTop: 0 }}>Asignar Turno</h3>

                            <div style={{ marginBottom: '12px' }}>
                                <label style={{ display: 'block', marginBottom: '4px' }}>Odontólogo:</label>
                                <select
                                    value={odontologoSeleccionadoId}
                                    onChange={(e) => setOdontologoSeleccionadoId(e.target.value)}
                                    style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                                >
                                    {odontologos.map(o => (
                                        <option key={o.id} value={o.id}>Dr/a. {o.nombre} {o.apellido}</option>
                                    ))}
                                </select>
                            </div>

                            <div style={{ marginBottom: '12px' }}>
                                <label style={{ display: 'block', marginBottom: '4px' }}>Fecha:</label>
                                <input
                                    type="date"
                                    value={fechaSeleccionada}
                                    onChange={(e) => setFechaSeleccionada(e.target.value)}
                                    style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                                />
                            </div>

                            <div style={{ marginBottom: '12px' }}>
                                <label style={{ display: 'block', marginBottom: '4px' }}>Nombre del Paciente:</label>
                                <input
                                    type="text"
                                    value={form.pacienteNombre}
                                    onChange={(e) => setForm({ ...form, pacienteNombre: e.target.value })}
                                    style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                                    placeholder="Ej. Juan Pérez"
                                />
                            </div>

                            <div style={{ marginBottom: '12px' }}>
                                <label style={{ display: 'block', marginBottom: '4px' }}>Teléfono:</label>
                                <input
                                    type="text"
                                    value={form.pacienteTelefono}
                                    onChange={(e) => setForm({ ...form, pacienteTelefono: e.target.value })}
                                    style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                                />
                            </div>

                            <div style={{ marginBottom: '12px' }}>
                                <label style={{ display: 'flex', alignItems: 'center', gap: '8px', cursor: 'pointer' }}>
                                    <input
                                        type="checkbox"
                                        checked={form.esEntreTurno}
                                        onChange={(e) => setForm({ ...form, esEntreTurno: e.target.checked })}
                                    />
                                    <strong>¿Es un Entre-Turno (Demanda espontánea)?</strong>
                                </label>
                            </div>

                            {!form.esEntreTurno && (
                                <div style={{ display: 'flex', gap: '12px', marginBottom: '12px' }}>
                                    <div style={{ flex: 1 }}>
                                        <label style={{ display: 'block', marginBottom: '4px' }}>Inicio:</label>
                                        <input
                                            type="time"
                                            value={form.horaInicio}
                                            onChange={(e) => setForm({ ...form, horaInicio: e.target.value })}
                                            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                                        />
                                    </div>
                                    <div style={{ flex: 1 }}>
                                        <label style={{ display: 'block', marginBottom: '4px' }}>Fin:</label>
                                        <input
                                            type="time"
                                            value={form.horaFin}
                                            onChange={(e) => setForm({ ...form, horaFin: e.target.value })}
                                            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                                        />
                                    </div>
                                </div>
                            )}

                            <button type="submit" style={{ width: '100%', padding: '10px', background: '#0284c7', color: 'white', border: 'none', borderRadius: '6px', fontWeight: 'bold', cursor: 'pointer' }}>
                                Guardar Turno
                            </button>
                        </form>

                        {/* Vista previa de turnos del día */}
                        <div>
                            <h3>Agenda del Día</h3>
                            <h4>Turnos Programados</h4>
                            {agenda.programados.length === 0 ? <p style={{ color: '#9ca3af' }}>No hay turnos programados.</p> : (
                                <ul style={{ listStyle: 'none', padding: 0 }}>
                                    {agenda.programados.map(t => (
                                        <li key={t.id} style={{ border: '1px solid #e5e7eb', padding: '10px', borderRadius: '6px', marginBottom: '8px' }}>
                                            <strong>{t.horaInicio?.substring(0, 5)} - {t.horaFin?.substring(0, 5)}</strong> | {t.pacienteWalkInNombre} — {getEstadoBadge(t.estado)}
                                        </li>
                                    ))}
                                </ul>
                            )}

                            <h4>Entre-Turnos (En Espera)</h4>
                            {agenda.entreTurnos.length === 0 ? <p style={{ color: '#9ca3af' }}>No hay entre-turnos.</p> : (
                                <ul style={{ listStyle: 'none', padding: 0 }}>
                                    {agenda.entreTurnos.map(t => (
                                        <li key={t.id} style={{ border: '1px solid #e5e7eb', padding: '10px', borderRadius: '6px', marginBottom: '8px', background: '#fffbeb' }}>
                                            <strong>Orden #{t.ordenEntreTurno}</strong> | {t.pacienteWalkInNombre} — {getEstadoBadge(t.estado)}
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </div>
                    </div>
                </div>
            )}

            {/* VISTA ODONTÓLOGO */}
            {usuarioActual?.rol === 'Odontologo' && (
                <div>
                    <h2>Consultorio — Dr/a. {usuarioActual.nombre} {usuarioActual.apellido}</h2>
                    <div style={{ marginBottom: '16px' }}>
                        <label>Fecha: </label>
                        <input
                            type="date"
                            value={fechaSeleccionada}
                            onChange={(e) => setFechaSeleccionada(e.target.value)}
                            style={{ padding: '6px 12px', borderRadius: '4px', border: '1px solid #ccc' }}
                        />
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '24px' }}>
                        {/* Pacientes Programados */}
                        <div>
                            <h3>Turnos Programados</h3>
                            {agenda.programados.map(t => (
                                <div key={t.id} style={{ border: '1px solid #e5e7eb', padding: '12px', borderRadius: '6px', marginBottom: '10px', background: '#ffffff' }}>
                                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                                        <strong>{t.horaInicio?.substring(0, 5)} hs</strong>
                                        {getEstadoBadge(t.estado)}
                                    </div>
                                    <div>Paciente: <strong>{t.pacienteWalkInNombre}</strong></div>
                                    <div style={{ color: '#6b7280', fontSize: '14px' }}>Tel: {t.pacienteWalkInTelefono || 'S/D'}</div>

                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px' }}>
                                        {t.estado === 1 && (
                                            <button onClick={() => cambiarEstado(t.id, 'iniciar')} style={{ padding: '6px 12px', background: '#2563eb', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                                                Atender
                                            </button>
                                        )}
                                        {t.estado === 2 && (
                                            <button onClick={() => cambiarEstado(t.id, 'finalizar')} style={{ padding: '6px 12px', background: '#16a34a', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                                                Finalizar Atención
                                            </button>
                                        )}
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Entre-turnos */}
                        <div>
                            <h3>Entre-Turnos (Sobreturnos)</h3>
                            {agenda.entreTurnos.map(t => (
                                <div key={t.id} style={{ border: '1px solid #fde68a', padding: '12px', borderRadius: '6px', marginBottom: '10px', background: '#fffbeb' }}>
                                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                                        <span style={{ fontWeight: 'bold', color: '#b45309' }}>Orden de llegada #{t.ordenEntreTurno}</span>
                                        {getEstadoBadge(t.estado)}
                                    </div>
                                    <div>Paciente: <strong>{t.pacienteWalkInNombre}</strong></div>
                                    <div style={{ color: '#6b7280', fontSize: '14px' }}>Tel: {t.pacienteWalkInTelefono || 'S/D'}</div>

                                    <div style={{ marginTop: '10px', display: 'flex', gap: '8px' }}>
                                        {t.estado === 1 && (
                                            <button onClick={() => cambiarEstado(t.id, 'iniciar')} style={{ padding: '6px 12px', background: '#2563eb', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                                                Atender
                                            </button>
                                        )}
                                        {t.estado === 2 && (
                                            <button onClick={() => cambiarEstado(t.id, 'finalizar')} style={{ padding: '6px 12px', background: '#16a34a', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                                                Finalizar Atención
                                            </button>
                                        )}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}