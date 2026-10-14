# Frontend — SIGO

Aplicación web de SIGO. Stack: React, TypeScript, Vite, React Router, Axios y Mantine.

## Requisitos

- Node.js (versión LTS).
- La API en ejecución (`dotnet run --project Clinica.Api`).

## Comandos

| Comando | Descripción |
|---|---|
| `npm install` | Instala las dependencias |
| `npm run dev` | Servidor de desarrollo en `http://localhost:5173` |
| `npm run build` | Compila para producción y verifica los tipos |
| `npm run lint` | Revisa el código con ESLint |
| `npm run format` | Da formato con Prettier |

## Configuración

`.env.development` define `VITE_API_URL`, la dirección de la API. El puerto `https` debe coincidir con el de `Clinica.Api/Properties/launchSettings.json`.

El servidor de desarrollo usa siempre el puerto 5173 (si está ocupado, falla en lugar de cambiar de puerto), porque es el origen permitido por CORS en la API.

## Estructura

```
src/
  api/        Cliente HTTP único y manejo de errores
  auth/       Sesión: contexto, almacenamiento del token y tipos
  routes/     Rutas, rutas protegidas por rol y pantalla inicial de cada rol
  layouts/    Estructura visual (cabecera y menú lateral) y menú por rol
  features/   Una carpeta por módulo (identidad, turnos, ...), espejo del backend
  shared/     Componentes y páginas comunes
```

## Cómo agregar una pantalla

1. Crear el componente en `src/features/<módulo>/`.
2. Registrar la ruta en `src/routes/AppRouter.tsx`, dentro del bloque `RutaProtegida` del rol que corresponda (si la pantalla es para varios roles, se pasan todos en `roles`):

   ```tsx
   <Route element={<RutaProtegida roles={['Secretaria']} />}>
     <Route path="/secretaria/pacientes" element={<PacientesPage />} />
   </Route>
   ```

3. Agregar la opción al menú en `src/layouts/menu.ts`, en la sección del rol:

   ```ts
   Secretaria: [
     { etiqueta: 'Recepción', ruta: RUTAS.secretaria },
     { etiqueta: 'Pacientes', ruta: '/secretaria/pacientes' },
   ],
   ```

## Convenciones

- Las llamadas a la API se hacen únicamente con `apiClient`; nunca con `fetch` ni `axios` directamente. Los errores se muestran con `mensajeDeError`.
- La sesión y los roles se consultan con `useAuth()`. Los componentes no leen el token.
- Los componentes de interfaz son de Mantine; no se mezclan otras bibliotecas.
- Los textos se escriben en español formal (usted).
- Las fechas llegan de la API en UTC y se convierten a hora local únicamente en la interfaz.
