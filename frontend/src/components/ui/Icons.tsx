import {
  ArrowLeft,
  Bell,
  Boxes,
  ChevronLeft,
  ChevronRight,
  Clock,
  Eye,
  Fence,
  Map,
  Pencil,
  Plus,
  RefreshCw,
  Route,
  Search,
  Trash2,
  Truck,
  Wifi,
  X,
} from 'lucide-react'
import type { LucideProps } from 'lucide-react'

const DEFAULT_SIZE = 18

export const MapIcon = (p: LucideProps) => <Map {...p} size={DEFAULT_SIZE} />
export const TruckIcon = (p: LucideProps) => <Truck {...p} size={DEFAULT_SIZE} />
export const TripsIcon = (p: LucideProps) => <Clock {...p} size={DEFAULT_SIZE} />
export const FenceIcon = (p: LucideProps) => <Fence {...p} size={DEFAULT_SIZE} />
export const MaterialsIcon = (p: LucideProps) => <Boxes {...p} size={DEFAULT_SIZE} />
export const RouteIcon = (p: LucideProps) => <Route {...p} size={DEFAULT_SIZE} />
export const BellIcon = (p: LucideProps) => <Bell {...p} size={DEFAULT_SIZE} />
export const PlusIcon = (p: LucideProps) => <Plus {...p} size={DEFAULT_SIZE} />
export const PencilIcon = (p: LucideProps) => <Pencil {...p} size={DEFAULT_SIZE} />
export const EyeIcon = (p: LucideProps) => <Eye {...p} size={DEFAULT_SIZE} />
export const TrashIcon = (p: LucideProps) => <Trash2 {...p} size={DEFAULT_SIZE} />
export const RefreshIcon = (p: LucideProps) => <RefreshCw {...p} size={DEFAULT_SIZE} />
export const CloseIcon = (p: LucideProps) => <X {...p} size={DEFAULT_SIZE} />
export const BackIcon = (p: LucideProps) => <ArrowLeft {...p} size={DEFAULT_SIZE} />
export const SignalIcon = (p: LucideProps) => <Wifi {...p} size={DEFAULT_SIZE} />
export const ChevronLeftIcon = (p: LucideProps) => <ChevronLeft {...p} size={DEFAULT_SIZE} />
export const ChevronRightIcon = (p: LucideProps) => <ChevronRight {...p} size={DEFAULT_SIZE} />
export const SearchIcon = (p: LucideProps) => <Search {...p} size={DEFAULT_SIZE} />