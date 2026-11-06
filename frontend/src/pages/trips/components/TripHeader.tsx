import { BackIcon } from '../../../components/ui/Icons'

interface TripHeaderProps {
  tripId: string
  isActive: boolean
  onBack: () => void
}

export default function TripHeader({ tripId, isActive, onBack }: TripHeaderProps) {
  return (
    <div className="page-head">
      <div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <button className="btn" onClick={onBack}>
            <BackIcon size={16} /> Viajes
          </button>
          {isActive && <span className="badge green">En curso</span>}
        </div>
        <h2 style={{ marginTop: 14 }}>Viaje #{tripId ? tripId.slice(0, 8) : ''}</h2>
      </div>
    </div>
  )
}