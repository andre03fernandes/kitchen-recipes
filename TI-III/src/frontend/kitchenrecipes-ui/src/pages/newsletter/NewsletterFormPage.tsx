import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { t } from '../../i18n/text'
import {
  createNewsletterSubscriber,
  getNewsletterSubscriberById,
  updateNewsletterSubscriber,
} from '../../services/newsletterApi'

type FormState = {
  email: string
  isConfirmed: boolean
}

const emptyState: FormState = {
  email: '',
  isConfirmed: false,
}

export function NewsletterFormPage() {
  const params = useParams<{ id: string }>()
  const navigate = useNavigate()
  const subscriberId = useMemo(() => Number(params.id), [params.id])
  const isEditMode = Number.isInteger(subscriberId) && subscriberId > 0

  const [form, setForm] = useState<FormState>(emptyState)
  const [isLoading, setIsLoading] = useState(isEditMode)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [feedback, setFeedback] = useState('')

  useEffect(() => {
    if (!isEditMode) {
      return
    }

    async function loadSubscriber() {
      try {
        const subscriber = await getNewsletterSubscriberById(subscriberId)
        setForm({
          email: subscriber.email,
          isConfirmed: subscriber.isConfirmed,
        })
      } catch {
        setFeedback(t('newsletter.errors.loadDetailsFailed'))
      } finally {
        setIsLoading(false)
      }
    }

    void loadSubscriber()
  }, [isEditMode, subscriberId])

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setFeedback('')

    const payload = {
      email: form.email,
      isConfirmed: form.isConfirmed,
    }

    try {
      if (isEditMode) {
        await updateNewsletterSubscriber(subscriberId, payload)
      } else {
        await createNewsletterSubscriber(payload)
      }

      navigate('/newsletter')
    } catch {
      setFeedback(t('newsletter.errors.saveFailed'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{isEditMode ? t('newsletter.form.editTitle') : t('newsletter.form.createTitle')}</h2>
          <p className="mt-2 text-slate/80">{t('newsletter.form.description')}</p>
        </div>
        <Link className="rounded-xl border border-pine/30 px-4 py-2 font-semibold text-pine hover:bg-pine/5" to="/newsletter">
          {t('newsletter.actions.backToList')}
        </Link>
      </div>

      {feedback && <p className="mb-4 rounded-lg bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">{feedback}</p>}

      {isLoading ? (
        <p className="text-slate/70">{t('newsletter.messages.loading')}</p>
      ) : (
        <form className="grid gap-4" onSubmit={onSubmit}>
          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('newsletter.fields.email')}
            <input
              className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
              maxLength={256}
              onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
              required
              type="email"
              value={form.email}
            />
          </label>

          <label className="flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-50/40 px-3 py-3 text-sm font-semibold text-slate">
            <input
              checked={form.isConfirmed}
              onChange={(event) => setForm((current) => ({ ...current, isConfirmed: event.target.checked }))}
              type="checkbox"
            />
            {t('newsletter.fields.isConfirmed')}
          </label>

          <div className="mt-2 flex flex-wrap gap-3">
            <button
              className="rounded-xl bg-pine px-5 py-2 font-semibold text-white transition hover:bg-pine/90 disabled:cursor-not-allowed disabled:bg-pine/60"
              disabled={isSubmitting}
              type="submit"
            >
              {isSubmitting ? t('newsletter.actions.saving') : t('newsletter.actions.save')}
            </button>
            <Link className="rounded-xl border border-slate-300 px-5 py-2 font-semibold text-slate hover:bg-slate-50" to="/newsletter">
              {t('newsletter.actions.cancel')}
            </Link>
          </div>
        </form>
      )}
    </section>
  )
}
