import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Toaster } from 'sonner'
import Layout from './components/layout/Layout'
import { MapBaseProvider } from './components/BaseMap'
import { ConfirmProvider } from './core/hooks/useConfirm'
import { AuthProvider } from './core/providers/AuthProvider'
import Dashboard from './pages/dashboard'
import Events from './pages/events'
import Fleet from './pages/fleet'
import Geofences from './pages/geofences'
import Materials from './pages/materials'
import RoutesPage from './pages/routes'
import TripDetail from './pages/trips/TripDetail'
import Trips from './pages/trips'
import Vehicles from './pages/vehicles'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <MapBaseProvider>
          <ConfirmProvider>
            <Routes>
              <Route element={<Layout />}>
                <Route index element={<Dashboard />} />
                <Route path="fleet" element={<Fleet />} />
                <Route path="vehicles" element={<Vehicles />} />
                <Route path="trips" element={<Trips />} />
                <Route path="trips/:id" element={<TripDetail />} />
                <Route path="events" element={<Events />} />
                <Route path="geofences" element={<Geofences />} />
                <Route path="routes" element={<RoutesPage />} />
                <Route path="materials" element={<Materials />} />
                <Route path="*" element={<Navigate to="/" replace />} />
              </Route>
            </Routes>
            <Toaster position="bottom-right" richColors />
          </ConfirmProvider>
        </MapBaseProvider>
      </AuthProvider>
    </BrowserRouter>
  )
}