import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { t } from '../../i18n/text'
import { getNewsletterSubscriberById } from '../../services/newsletterApi'
import type { NewsletterSubscriberDto } from '../../types/newsletter'

type NewsletterDetailsPageProps = {
  canManage?: boolean
}

export function NewsletterDetailsPage({ canManage = false }: NewsletterDetailsPageProps) {
  const params = useParams<{ id: string }>()
  const subscriberId = useMemo(() => Number(params.id), [params.id])

  const [subscriber, setSubscriber] = useState<NewsletterSubscriberDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [feedback, setFeedback] = useState('')

  useEffect(() => {
    async function loadSubscriber() {
      if (!Number.isInteger(subscriberId) || subscriberId <= 0) {
        setFeedback(t('newsletter.errors.loadDetailsFailed'))
        setIsLoading(false)
        return
      }

      try {
        const data = await getNewsletterSubscriberById(subscriberId)
        setSubscriber(data)
      } catch {
        setFeedback(t('newsletter.errors.loadDetailsFailed'))
      } finally {
        setIsLoading(false)
      }
    }

    void loadSubscriber()
  }, [subscriberId])

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{t('newsletter.details.title')}</h2>
          <p className="mt-2 text-slate/80">{t('newsletter.details.description')}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {canManage && subscriber && (
            <Link className="rounded-xl border border-pine/30 px-4 py-2 font-semibold text-pine hover:bg-pine/5" to={`/newsletter/${subscriber.id}/edit`}>
              {t('newsletter.actions.edit')}
            </Link>
          )}
          <Link className="rounded-xl border border-slate-300 px-4 py-2 font-semibold text-slate hover:bg-slate-50" to="/newsletter">
            {t('newsletter.actions.backToList')}
          </Link>
        </div>
      </div>

      {feedback && <p className="mb-4 rounded-lg bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">{feedback}</p>}

      {isLoading ? (
        <p className="text-slate/70">{t('newsletter.messages.loading')}</p>
      ) : !subscriber ? (
        <p className="text-slate/70">{t('newsletter.errors.loadDetailsFailed')}</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="rounded-xl border border-amber-200 bg-amber-50/40 p-4 sm:col-span-2">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('newsletter.fields.email')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{subscriber.email}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('newsletter.fields.isConfirmed')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{subscriber.isConfirmed ? t('newsletter.messages.confirmed') : t('newsletter.messages.pending')}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('newsletter.details.createdAtUtc')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{new Date(subscriber.createdAtUtc).toLocaleString('en-US')}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4 sm:col-span-2">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('newsletter.details.updatedAtUtc')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{subscriber.updatedAtUtc ? new Date(subscriber.updatedAtUtc).toLocaleString('en-US') : t('newsletter.details.notUpdated')}</p>
          </div>
        </div>
      )}
    </section>
  )
}
