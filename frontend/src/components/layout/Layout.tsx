import { useEffect, useState } from 'react'
import { Outlet } from 'react-router-dom'
import Sidebar from './Sidebar'
import Topbar from './Topbar'

const STORAGE_KEY = 'sidebar-collapsed'

export default function Layout() {
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem(STORAGE_KEY) === '1')

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, collapsed ? '1' : '0')
  }, [collapsed])

  return (
    <div
      className={[
        'grid h-full overflow-hidden',
        'grid-rows-[minmax(0,1fr)] max-md:grid-rows-[auto_minmax(0,1fr)]',
        collapsed ? 'grid-cols-[64px_1fr]' : 'grid-cols-[220px_1fr]',
        'max-md:grid-cols-1',
      ].join(' ')}
    >
      <Sidebar collapsed={collapsed} onToggle={() => setCollapsed((c) => !c)} />
      <div className="flex flex-col min-w-0 min-h-0 overflow-hidden">
        <Topbar />
        <main className="flex-1 overflow-y-auto overflow-x-hidden p-4 sm:p-6 max-sm:p-3">
          <Outlet />
        </main>
      </div>
    </div>
  )
}