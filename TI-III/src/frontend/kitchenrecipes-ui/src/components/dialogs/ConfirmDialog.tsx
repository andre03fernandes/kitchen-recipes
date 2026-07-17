type ConfirmDialogProps = {
  isOpen: boolean
  title: string
  description: string
  confirmLabel: string
  cancelLabel: string
  confirmTone?: 'danger' | 'primary'
  isLoading?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function ConfirmDialog({
  isOpen,
  title,
  description,
  confirmLabel,
  cancelLabel,
  confirmTone = 'danger',
  isLoading = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  if (!isOpen) {
    return null
  }

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-3xl border border-slate-600 bg-slate-900/96 p-5 shadow-[0_28px_90px_rgba(0,0,0,0.58)]">
        <div className="flex items-center gap-3">
          <span className={`flex h-10 w-10 items-center justify-center rounded-2xl border ${confirmTone === 'danger' ? 'border-rose-300 bg-rose-500/15 text-rose-200' : 'border-sky-300 bg-sky-500/15 text-sky-200'}`}>
            !
          </span>
          <div>
            <h2 className="font-heading text-2xl text-slate-100">{title}</h2>
            <p className="text-xs uppercase tracking-[0.18em] text-slate-400">{confirmTone === 'danger' ? 'Action required' : 'Confirmation'}</p>
          </div>
        </div>

        <p className="mt-4 text-sm leading-6 text-slate-300">{description}</p>

        <div className="mt-5 flex flex-wrap justify-end gap-3">
          <button
            className="rounded-xl border border-slate-600 bg-slate-800 px-4 py-2 font-semibold text-slate-100 transition hover:bg-slate-700"
            onClick={onCancel}
            type="button"
          >
            {cancelLabel}
          </button>
          <button
            className={`rounded-xl border px-4 py-2 font-semibold transition ${confirmTone === 'danger' ? 'border-rose-400 bg-rose-500/15 text-rose-200 hover:bg-rose-500/25' : 'border-sky-400 bg-sky-500/15 text-sky-200 hover:bg-sky-500/25'}`}
            disabled={isLoading}
            onClick={onConfirm}
            type="button"
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}