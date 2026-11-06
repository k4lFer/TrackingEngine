import { NavLink } from 'react-router-dom'
import {
  BellIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  FenceIcon,
  MapIcon,
  MaterialsIcon,
  RouteIcon,
  TripsIcon,
  TruckIcon,
} from '../ui/Icons'

const NAV_ITEMS = [
  {
    to: '/',
    label: 'Panel',
    icon: <MapIcon />,
    end: true,
  },
  {
    to: '/fleet',
    label: 'Flota',
    icon: <TruckIcon />,
  },
  {
    to: '/trips',
    label: 'Viajes',
    icon: <TripsIcon />,
  },
  {
    to: '/events',
    label: 'Eventos',
    icon: <BellIcon />,
  },
  {
    to: '/geofences',
    label: 'Geocercas',
    icon: <FenceIcon />,
  },
  {
    to: '/routes',
    label: 'Rutas',
    icon: <RouteIcon />,
  },
  {
    to: '/materials',
    label: 'Materiales',
    icon: <MaterialsIcon />,
  },
]

interface SidebarProps {
  collapsed: boolean
  onToggle: () => void
}

export default function Sidebar({ collapsed, onToggle }: SidebarProps) {
  return (
    <aside
      className={[
        'flex flex-col bg-surface border-r border-line py-4',
        collapsed ? 'px-2 items-center' : 'px-3',
        'max-lg:flex-row max-lg:py-2 max-lg:px-2 max-lg:items-center max-lg:overflow-x-auto max-lg:border-r-0 max-lg:border-b',
      ].join(' ')}
    >
      <div
        className={[
          'flex items-center gap-2.5 pb-5 border-b border-line mb-4 max-lg:hidden',
          collapsed ? 'flex-col gap-2 px-0 border-b max-lg:border-b-0' : 'px-2',
        ].join(' ')}
      >
        <div className="w-8 h-8 shrink-0 rounded-lg bg-gradient-to-br from-accent to-accent-strong grid place-items-center text-white font-display font-bold text-[15px]">
          TE
        </div>
        {!collapsed && (
          <div className="max-lg:hidden">
            <div className="text-sm font-bold tracking-[0.02em] whitespace-nowrap">Tracking Engine</div>
            <div className="text-[11px] text-ink-faint">Mineral · GPS</div>
          </div>
        )}
      </div>

      <nav className="flex flex-col gap-0.5 flex-1 max-lg:flex-row max-lg:max-w-full">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            title={collapsed ? item.label : undefined}
            className={({ isActive }) =>
              [
                'flex items-center gap-2.5 px-2.5 py-1.5 rounded-[6px] text-[13px] font-medium transition-colors',
                collapsed ? 'justify-center w-full px-0' : '',
                isActive
                  ? 'bg-accent-dim text-accent-strong'
                  : 'text-ink-dim hover:bg-canvas hover:text-ink',
              ].join(' ')
            }
          >
            <span className="w-[18px] h-[18px] shrink-0 grid place-items-center opacity-90">{item.icon}</span>
            {!collapsed && item.label}
          </NavLink>
        ))}
      </nav>

      <div
        className={[
          'pt-3 pb-0 border-t border-line max-lg:hidden',
          collapsed ? 'flex flex-col items-center gap-2 px-0' : 'px-2',
        ].join(' ')}
      >
        <button
          type="button"
          onClick={onToggle}
          title={collapsed ? 'Expandir menú' : 'Colapsar menú'}
          className="flex items-center justify-center w-7 h-7 rounded-[6px] text-ink-dim hover:bg-canvas hover:text-ink transition-colors"
        >
          {collapsed ? <ChevronRightIcon size={16} /> : <ChevronLeftIcon size={16} />}
        </button>
        {!collapsed && <div className="mt-1 text-[11px] text-ink-faint whitespace-nowrap">API REST + React · Demo</div>}
      </div>
    </aside>
  )
}