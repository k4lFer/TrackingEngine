import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

const conn = new HubConnectionBuilder()
  .withUrl('http://localhost:8080/hubs/tracking')
  .configureLogging(LogLevel.Warning)
  .build()

conn.on('connected', () => console.log('EVENT connected'))
conn.on('vehicleMoved', (v) => console.log('vehicleMoved', v.vehicleCode, Math.round(v.speedKmh)))

try {
  await conn.start()
  console.log('DIRECT_CONNECT_OK')
} catch (e) {
  console.log('DIRECT_CONNECT_FAIL:', e.message)
  if (e?.innerError?.message) console.log('INNER:', e.innerError.message)
}
setTimeout(() => process.exit(0), 8000)