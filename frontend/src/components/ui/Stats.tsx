import type { ReactNode } from 'react'

interface StatDef {
  label: string
  value: ReactNode
  tone?: 'ok' | 'warn' | 'alert'
}

export default function Stats({ items }: { items: StatDef[] }) {
  const toneColor: Record<string, string | undefined> = {
    ok: 'text-ok',
    warn: 'text-warn',
    alert: 'text-alert',
  }
  return (
    <div className="grid grid-cols-4 gap-4 mb-4 max-xl:grid-cols-2 max-sm:grid-cols-2">
      {items.map((s) => (
        <div className="bg-surface border border-line rounded-panel p-4" key={s.label}>
          <div className={`font-mono text-[26px] font-semibold text-ink leading-[1.1] ${s.tone ? toneColor[s.tone] : ''}`}>
            {s.value}
          </div>
          <div className="mt-1.5 text-xs text-ink-dim">{s.label}</div>
        </div>
      ))}
    </div>
  )
}