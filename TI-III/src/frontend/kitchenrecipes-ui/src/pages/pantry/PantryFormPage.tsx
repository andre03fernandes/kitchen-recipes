import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { t } from '../../i18n/text'
import { createPantryItem, getPantryItemById, updatePantryItem } from '../../services/pantryApi'
import { readImageFileAsDataUrl } from '../../utils/imageUpload'

type FormState = {
  name: string
  imageUrl: string
  availableQuantity: string
  unit: string
  expirationDate: string
}

const emptyState: FormState = {
  name: '',
  imageUrl: '',
  availableQuantity: '1',
  unit: 'g',
  expirationDate: '',
}

export function PantryFormPage() {
  const params = useParams<{ id: string }>()
  const navigate = useNavigate()
  const itemId = useMemo(() => Number(params.id), [params.id])
  const isEditMode = Number.isInteger(itemId) && itemId > 0

  const [form, setForm] = useState<FormState>(emptyState)
  const [isLoading, setIsLoading] = useState(isEditMode)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [feedback, setFeedback] = useState('')

  useEffect(() => {
    if (!isEditMode) {
      return
    }

    async function loadPantryItem() {
      try {
        const pantryItem = await getPantryItemById(itemId)
        setForm({
          name: pantryItem.name,
          imageUrl: pantryItem.imageUrl ?? '',
          availableQuantity: pantryItem.availableQuantity.toString(),
          unit: pantryItem.unit,
          expirationDate: pantryItem.expirationDate ?? '',
        })
      } catch {
        setFeedback(t('pantry.errors.loadDetailsFailed'))
      } finally {
        setIsLoading(false)
      }
    }

    void loadPantryItem()
  }, [isEditMode, itemId])

  function updateField<K extends keyof FormState>(key: K, value: FormState[K]) {
    setForm((current) => ({
      ...current,
      [key]: value,
    }))
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setFeedback('')

    const payload = {
      name: form.name,
      imageUrl: form.imageUrl.trim() ? form.imageUrl : null,
      availableQuantity: Number(form.availableQuantity),
      unit: form.unit,
      expirationDate: form.expirationDate.trim() ? form.expirationDate : null,
    }

    try {
      if (isEditMode) {
        await updatePantryItem(itemId, payload)
      } else {
        await createPantryItem(payload)
      }

      navigate('/pantry')
    } catch {
      setFeedback(t('pantry.errors.saveFailed'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{isEditMode ? t('pantry.form.editTitle') : t('pantry.form.createTitle')}</h2>
          <p className="mt-2 text-slate/80">{t('pantry.form.description')}</p>
        </div>
        <Link className="rounded-xl border border-pine/30 px-4 py-2 font-semibold text-pine hover:bg-pine/5" to="/pantry">
          {t('pantry.actions.backToList')}
        </Link>
      </div>

      {feedback && <p className="mb-4 rounded-lg bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">{feedback}</p>}

      {isLoading ? (
        <p className="text-slate/70">{t('pantry.messages.loading')}</p>
      ) : (
        <form className="grid gap-4" onSubmit={onSubmit}>
          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('pantry.fields.name')}
            <input
              className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
              maxLength={100}
              onChange={(event) => updateField('name', event.target.value)}
              required
              type="text"
              value={form.name}
            />
          </label>

          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('pantry.fields.image')}
            <input
              accept="image/*"
              className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
              onChange={(event) => {
                const file = event.target.files?.[0]
                if (!file) {
                  return
                }

                void (async () => {
                  try {
                    const dataUrl = await readImageFileAsDataUrl(file)
                    updateField('imageUrl', dataUrl)
                    setFeedback('')
                  } catch {
                    setFeedback(t('pantry.errors.invalidImageFile'))
                  }
                })()
              }}
              type="file"
            />
            {form.imageUrl && (
              <div className="flex flex-wrap items-center gap-3 rounded-lg border border-amber-200 bg-amber-50/40 p-3">
                <img alt={form.name || t('pantry.details.title')} className="h-20 w-20 rounded-lg object-cover" src={form.imageUrl} />
                <button
                  className="rounded-lg border border-slate-300 px-3 py-1 text-sm font-semibold text-slate hover:bg-slate-50"
                  onClick={() => updateField('imageUrl', '')}
                  type="button"
                >
                  {t('pantry.actions.removeImage')}
                </button>
              </div>
            )}
          </label>

          <div className="grid gap-4 sm:grid-cols-2">
            <label className="grid gap-2 text-sm font-semibold text-slate">
              {t('pantry.fields.availableQuantity')}
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                min={0}
                onChange={(event) => updateField('availableQuantity', event.target.value)}
                required
                step="0.01"
                type="number"
                value={form.availableQuantity}
              />
            </label>

            <label className="grid gap-2 text-sm font-semibold text-slate">
              {t('pantry.fields.unit')}
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                maxLength={24}
                onChange={(event) => updateField('unit', event.target.value)}
                required
                type="text"
                value={form.unit}
              />
            </label>
          </div>

          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('pantry.fields.expirationDate')}
            <input
              className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
              onChange={(event) => updateField('expirationDate', event.target.value)}
              type="date"
              value={form.expirationDate}
            />
          </label>

          <div className="mt-2 flex flex-wrap gap-3">
            <button
              className="rounded-xl bg-pine px-5 py-2 font-semibold text-white transition hover:bg-pine/90 disabled:cursor-not-allowed disabled:bg-pine/60"
              disabled={isSubmitting}
              type="submit"
            >
              {isSubmitting ? t('pantry.actions.saving') : t('pantry.actions.save')}
            </button>
            <Link className="rounded-xl border border-slate-300 px-5 py-2 font-semibold text-slate hover:bg-slate-50" to="/pantry">
              {t('pantry.actions.cancel')}
            </Link>
          </div>
        </form>
      )}
    </section>
  )
}
