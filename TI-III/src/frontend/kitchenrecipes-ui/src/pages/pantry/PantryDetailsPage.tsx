import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { t } from '../../i18n/text'
import { getPantryItemById } from '../../services/pantryApi'
import type { PantryItemDto } from '../../types/pantry'

type PantryDetailsPageProps = {
  canManage?: boolean
}

export function PantryDetailsPage({ canManage = false }: PantryDetailsPageProps) {
  const params = useParams<{ id: string }>()
  const itemId = useMemo(() => Number(params.id), [params.id])

  const [item, setItem] = useState<PantryItemDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [feedback, setFeedback] = useState('')

  useEffect(() => {
    async function loadItem() {
      if (!Number.isInteger(itemId) || itemId <= 0) {
        setFeedback(t('pantry.errors.loadDetailsFailed'))
        setIsLoading(false)
        return
      }

      try {
        const data = await getPantryItemById(itemId)
        setItem(data)
      } catch {
        setFeedback(t('pantry.errors.loadDetailsFailed'))
      } finally {
        setIsLoading(false)
      }
    }

    void loadItem()
  }, [itemId])

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{t('pantry.details.title')}</h2>
          <p className="mt-2 text-slate/80">{t('pantry.details.description')}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {canManage && item && (
            <Link className="rounded-xl border border-pine/30 px-4 py-2 font-semibold text-pine hover:bg-pine/5" to={`/pantry/${item.id}/edit`}>
              {t('pantry.actions.edit')}
            </Link>
          )}
          <Link className="rounded-xl border border-slate-300 px-4 py-2 font-semibold text-slate hover:bg-slate-50" to="/pantry">
            {t('pantry.actions.backToList')}
          </Link>
        </div>
      </div>

      {feedback && <p className="mb-4 rounded-lg bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">{feedback}</p>}

      {isLoading ? (
        <p className="text-slate/70">{t('pantry.messages.loading')}</p>
      ) : !item ? (
        <p className="text-slate/70">{t('pantry.errors.loadDetailsFailed')}</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="rounded-xl border border-amber-200 bg-white p-4 sm:col-span-2">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('pantry.fields.image')}</p>
            {item.imageUrl ? (
              <img alt={item.name} className="mt-2 h-40 w-40 rounded-lg object-cover" src={item.imageUrl} />
            ) : (
              <p className="mt-1 text-slate/70">{t('pantry.messages.noImage')}</p>
            )}
          </div>

          <div className="rounded-xl border border-amber-200 bg-amber-50/40 p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('pantry.fields.name')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{item.name}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('pantry.fields.availableQuantity')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{item.availableQuantity}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('pantry.fields.unit')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{item.unit}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('pantry.fields.expirationDate')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{item.expirationDate ?? t('pantry.messages.noExpirationDate')}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('pantry.details.createdAtUtc')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{new Date(item.createdAtUtc).toLocaleString('en-US')}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('pantry.details.updatedAtUtc')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{item.updatedAtUtc ? new Date(item.updatedAtUtc).toLocaleString('en-US') : t('pantry.details.notUpdated')}</p>
          </div>
        </div>
      )}
    </section>
  )
}
