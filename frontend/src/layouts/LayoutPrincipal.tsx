import { AppShell, Badge, Burger, Button, Group, NavLink, Text, Title } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconLogout } from '@tabler/icons-react'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { menuParaRoles } from './menu'

// Estructura visual de la zona autenticada: cabecera, menú lateral y contenido.
export function LayoutPrincipal() {
  const { usuario, logout } = useAuth()
  const [menuAbierto, { toggle, close }] = useDisclosure()
  const { pathname } = useLocation()

  if (!usuario) return null

  const items = menuParaRoles(usuario.roles)

  // Si hay rutas anidadas, se marca la opción cuya ruta coincide más específicamente
  const activo = items
    .filter((item) => pathname === item.ruta || pathname.startsWith(`${item.ruta}/`))
    .sort((a, b) => b.ruta.length - a.ruta.length)[0]

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 260, breakpoint: 'sm', collapsed: { mobile: !menuAbierto } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Group wrap="nowrap">
            <Burger
              opened={menuAbierto}
              onClick={toggle}
              hiddenFrom="sm"
              size="sm"
              aria-label="Abrir o cerrar el menú"
            />
            <Title order={3} c="teal">
              SIGO
            </Title>
            <Text size="sm" c="dimmed" visibleFrom="md">
              Sistema Integral de Gestión Odontológica
            </Text>
          </Group>

          <Group gap="sm" wrap="nowrap">
            <Text size="sm" fw={500} visibleFrom="sm">
              {usuario.nombre} {usuario.apellido}
            </Text>
            {usuario.roles.map((rol) => (
              <Badge key={rol} variant="light" visibleFrom="sm">
                {rol}
              </Badge>
            ))}
            <Button
              variant="subtle"
              color="gray"
              leftSection={<IconLogout size={16} />}
              onClick={logout}
            >
              Salir
            </Button>
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="md">
        {items.map((item) => (
          <NavLink
            key={item.ruta}
            component={Link}
            to={item.ruta}
            label={item.etiqueta}
            active={item.ruta === activo?.ruta}
            onClick={close}
          />
        ))}
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  )
}
