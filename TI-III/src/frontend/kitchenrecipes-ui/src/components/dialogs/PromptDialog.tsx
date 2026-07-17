import { useEffect, useState } from 'react'

type PromptDialogProps = {
  isOpen: boolean
  title: string
  description: string
  label: string
  value: string
  placeholder?: string
  confirmLabel: string
  cancelLabel: string
  onSubmit: (value: string) => void
  onCancel: () => void
}

export function PromptDialog({
  isOpen,
  title,
  description,
  label,
  value,
  placeholder,
  confirmLabel,
  cancelLabel,
  onSubmit,
  onCancel,
}: PromptDialogProps) {
  const [draft, setDraft] = useState(value)

  useEffect(() => {
    if (isOpen) {
      setDraft(value)
    }
  }, [isOpen, value])

  if (!isOpen) {
    return null
  }

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-3xl border border-slate-600 bg-slate-900/96 p-5 shadow-[0_28px_90px_rgba(0,0,0,0.58)]">
        <div className="flex items-center gap-3">
          <span className="flex h-10 w-10 items-center justify-center rounded-2xl border border-sky-300 bg-sky-500/15 text-sky-200">
            ✎
          </span>
          <div>
            <h2 className="font-heading text-2xl text-slate-100">{title}</h2>
            <p className="text-xs uppercase tracking-[0.18em] text-slate-400">Edit details</p>
          </div>
        </div>

        <p className="mt-4 text-sm leading-6 text-slate-300">{description}</p>

        <label className="mt-4 block text-sm font-semibold text-slate-200">
          {label}
          <input
            autoFocus
            className="mt-2 w-full rounded-xl border border-slate-600 bg-slate-800 px-4 py-3 text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-sky-400 focus:ring-2 focus:ring-sky-400/20"
            onChange={(event) => setDraft(event.target.value)}
            placeholder={placeholder}
            value={draft}
          />
        </label>

        <div className="mt-5 flex flex-wrap justify-end gap-3">
          <button
            className="rounded-xl border border-slate-600 bg-slate-800 px-4 py-2 font-semibold text-slate-100 transition hover:bg-slate-700"
            onClick={onCancel}
            type="button"
          >
            {cancelLabel}
          </button>
          <button
            className="rounded-xl border border-sky-400 bg-sky-500/15 px-4 py-2 font-semibold text-sky-200 transition hover:bg-sky-500/25"
            onClick={() => onSubmit(draft)}
            type="button"
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}