import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ConfirmDialog } from '../../components/dialogs/ConfirmDialog'
import { TableToolbar } from '../../components/table/TableToolbar'
import { t } from '../../i18n/text'
import {
  deleteNewsletterSubscriber,
  getNewsletterCampaignHistory,
  getNewsletterCampaignStats,
  getNewsletterSubscribers,
  getNewsletterTemplates,
  sendNewsletterCampaign,
} from '../../services/newsletterApi'
import type {
  NewsletterCampaignHistoryItemDto,
  NewsletterCampaignStatsDto,
  NewsletterSubscriberDto,
  NewsletterTemplateDto,
} from '../../types/newsletter'
import {
  DEFAULT_TABLE_PAGE_SIZE,
  applySimpleFilter,
  exportItemsToDocx,
  exportItemsToExcel,
  exportItemsToPdf,
  paginateItems,
} from '../../utils/tableTools'

type NewsletterListPageProps = {
  canManage?: boolean
}

export function NewsletterListPage({ canManage = false }: NewsletterListPageProps) {
  const [subscribers, setSubscribers] = useState<NewsletterSubscriberDto[]>([])
  const [templates, setTemplates] = useState<NewsletterTemplateDto[]>([])
  const [stats, setStats] = useState<NewsletterCampaignStatsDto | null>(null)
  const [history, setHistory] = useState<NewsletterCampaignHistoryItemDto[]>([])
  const [selectedTemplateKey, setSelectedTemplateKey] = useState('')
  const [subscribersFilterText, setSubscribersFilterText] = useState('')
  const [subscribersPage, setSubscribersPage] = useState(1)
  const [subscribersPageSize, setSubscribersPageSize] = useState(DEFAULT_TABLE_PAGE_SIZE)
  const [historyFilterText, setHistoryFilterText] = useState('')
  const [historyPage, setHistoryPage] = useState(1)
  const [historyPageSize, setHistoryPageSize] = useState(DEFAULT_TABLE_PAGE_SIZE)
  const [isLoading, setIsLoading] = useState(true)
  const [isSendingCampaign, setIsSendingCampaign] = useState(false)
  const [isExportingSubscribers, setIsExportingSubscribers] = useState(false)
  const [isExportingHistoryTable, setIsExportingHistoryTable] = useState(false)
  const [feedback, setFeedback] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<NewsletterSubscriberDto | null>(null)

  async function loadSubscribers() {
    setIsLoading(true)
    try {
      const data = await getNewsletterSubscribers()
      setSubscribers(data)
    } catch {
      setFeedback(t('newsletter.errors.loadFailed'))
    } finally {
      setIsLoading(false)
    }
  }

  async function loadTemplates() {
    try {
      const data = await getNewsletterTemplates()
      setTemplates(data)

      if (!selectedTemplateKey && data.length > 0) {
        setSelectedTemplateKey(data[0].key)
      }
    } catch {
      setFeedback(t('newsletter.errors.loadTemplatesFailed'))
    }
  }

  async function loadStats() {
    try {
      const data = await getNewsletterCampaignStats()
      setStats(data)
    } catch {
      setStats(null)
    }
  }

  async function loadHistory() {
    try {
      const data = await getNewsletterCampaignHistory({ limit: 100 })
      setHistory(data)
    } catch {
      setHistory([])
    }
  }

  useEffect(() => {
    void loadSubscribers()
    void loadTemplates()
    void loadStats()
    void loadHistory()
  }, [])

  async function onDelete(subscriber: NewsletterSubscriberDto) {
    try {
      await deleteNewsletterSubscriber(subscriber.id)
      setFeedback(t('newsletter.messages.deleted'))
      await loadSubscribers()
    } catch {
      setFeedback(t('newsletter.errors.deleteFailed'))
    }
  }

  async function onSendCampaign() {
    if (!selectedTemplateKey) {
      setFeedback(t('newsletter.errors.templateRequired'))
      return
    }

    setIsSendingCampaign(true)

    try {
      const result = await sendNewsletterCampaign({ templateKey: selectedTemplateKey })
      setFeedback(t('newsletter.messages.campaignSent').replace('{count}', result.recipientsCount.toString()))
      await loadStats()
      await loadHistory()
    } catch {
      setFeedback(t('newsletter.errors.sendCampaignFailed'))
    } finally {
      setIsSendingCampaign(false)
    }
  }

  async function exportSubscribersTable(format: 'pdf' | 'docx' | 'excel') {
    const timestamp = new Date().toISOString().replaceAll(':', '-')
    const columns = [
      { header: t('newsletter.fields.email'), value: (item: NewsletterSubscriberDto) => item.email },
      { header: t('newsletter.fields.isConfirmed'), value: (item: NewsletterSubscriberDto) => (item.isConfirmed ? t('newsletter.messages.confirmed') : t('newsletter.messages.pending')) },
    ]

    setIsExportingSubscribers(true)

    try {
      if (format === 'pdf') {
        exportItemsToPdf(filteredSubscribers, columns, t('newsletter.list.title'), `newsletter-subscribers-${timestamp}.pdf`)
      } else if (format === 'docx') {
        await exportItemsToDocx(filteredSubscribers, columns, t('newsletter.list.title'), `newsletter-subscribers-${timestamp}.docx`)
      } else {
        exportItemsToExcel(filteredSubscribers, columns, 'NewsletterSubscribers', `newsletter-subscribers-${timestamp}.xlsx`)
      }

      setFeedback(t('tables.exportSuccess'))
    } catch {
      setFeedback(t('tables.exportError'))
    } finally {
      setIsExportingSubscribers(false)
    }
  }

  async function exportHistoryTable(format: 'pdf' | 'docx' | 'excel') {
    const timestamp = new Date().toISOString().replaceAll(':', '-')
    const columns = [
      { header: t('newsletter.history.templateKey'), value: (item: NewsletterCampaignHistoryItemDto) => item.templateKey },
      { header: t('newsletter.history.recipients'), value: (item: NewsletterCampaignHistoryItemDto) => item.recipientsCount },
      { header: t('newsletter.history.sentAtUtc'), value: (item: NewsletterCampaignHistoryItemDto) => new Date(item.sentAtUtc).toLocaleString('en-US') },
    ]

    setIsExportingHistoryTable(true)

    try {
      if (format === 'pdf') {
        exportItemsToPdf(filteredHistory, columns, t('newsletter.history.title'), `newsletter-history-${timestamp}.pdf`)
      } else if (format === 'docx') {
        await exportItemsToDocx(filteredHistory, columns, t('newsletter.history.title'), `newsletter-history-${timestamp}.docx`)
      } else {
        exportItemsToExcel(filteredHistory, columns, 'NewsletterHistory', `newsletter-history-${timestamp}.xlsx`)
      }

      setFeedback(t('tables.exportSuccess'))
    } catch {
      setFeedback(t('tables.exportError'))
    } finally {
      setIsExportingHistoryTable(false)
    }
  }

  const filteredSubscribers = useMemo(
    () => applySimpleFilter(subscribers, subscribersFilterText, (subscriber) => [subscriber.email, subscriber.isConfirmed ? t('newsletter.messages.confirmed') : t('newsletter.messages.pending')]),
    [subscribers, subscribersFilterText],
  )

  const subscribersPagination = useMemo(
    () => paginateItems(filteredSubscribers, subscribersPage, subscribersPageSize),
    [filteredSubscribers, subscribersPage, subscribersPageSize],
  )

  const filteredHistory = useMemo(
    () =>
      applySimpleFilter(history, historyFilterText, (item) => [
        item.templateKey,
        item.recipientsCount,
        new Date(item.sentAtUtc).toLocaleString('en-US'),
      ]),
    [history, historyFilterText],
  )

  const historyPagination = useMemo(
    () => paginateItems(filteredHistory, historyPage, historyPageSize),
    [filteredHistory, historyPage, historyPageSize],
  )

  const selectedTemplate = templates.find((template) => template.key === selectedTemplateKey)

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{t('newsletter.list.title')}</h2>
          <p className="mt-2 text-slate/80">{t('newsletter.list.description')}</p>
        </div>
        {canManage && (
          <Link className="rounded-xl bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90" to="/newsletter/new">
            {t('newsletter.actions.create')}
          </Link>
        )}
      </div>

      {feedback && <p className="mt-4 rounded-lg bg-amber-50 px-3 py-2 text-sm font-semibold text-ember">{feedback}</p>}

      {isLoading ? (
        <p className="mt-6 text-slate/70">{t('newsletter.messages.loading')}</p>
      ) : (
        <div className="mt-6 overflow-x-auto">
          <TableToolbar
            filterPlaceholder={t('newsletter.fields.email')}
            filterText={subscribersFilterText}
            isExporting={isExportingSubscribers}
            onExportDocx={() => {
              void exportSubscribersTable('docx')
            }}
            onExportExcel={() => {
              void exportSubscribersTable('excel')
            }}
            onExportPdf={() => {
              void exportSubscribersTable('pdf')
            }}
            onFilterTextChange={(value) => {
              setSubscribersFilterText(value)
              setSubscribersPage(1)
            }}
            onNextPage={() => setSubscribersPage((current) => Math.min(subscribersPagination.totalPages, current + 1))}
            onPageSizeChange={(value) => {
              setSubscribersPageSize(value)
              setSubscribersPage(1)
            }}
            onPreviousPage={() => setSubscribersPage((current) => Math.max(1, current - 1))}
            page={subscribersPagination.currentPage}
            pageSize={subscribersPageSize}
            totalPages={subscribersPagination.totalPages}
          />

          <table className="min-w-full divide-y divide-amber-200">
            <thead>
              <tr className="text-left text-sm uppercase tracking-wide text-slate/65">
                <th className="py-2 pr-3">{t('newsletter.fields.email')}</th>
                <th className="py-2 pr-3">{t('newsletter.fields.isConfirmed')}</th>
                <th className="py-2 pr-3">{t('newsletter.fields.actions')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-amber-100">
              {subscribersPagination.items.length === 0 ? (
                <tr>
                  <td className="py-8 text-center text-sm text-slate/70" colSpan={3}>
                    {t('newsletter.messages.empty')}
                  </td>
                </tr>
              ) : (
                subscribersPagination.items.map((subscriber) => (
                  <tr key={subscriber.id} className="text-sm text-slate/90">
                    <td className="py-3 pr-3 font-semibold">{subscriber.email}</td>
                    <td className="py-3 pr-3">
                      {subscriber.isConfirmed ? t('newsletter.messages.confirmed') : t('newsletter.messages.pending')}
                    </td>
                    <td className="py-3 pr-3">
                      <div className="flex flex-wrap gap-2">
                        <Link className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-ember hover:bg-amber-50" to={`/newsletter/${subscriber.id}`}>
                          {t('newsletter.actions.details')}
                        </Link>
                        {canManage && (
                          <>
                            <Link className="rounded-lg border border-pine/30 px-3 py-1 font-semibold text-pine hover:bg-pine/5" to={`/newsletter/${subscriber.id}/edit`}>
                              {t('newsletter.actions.edit')}
                            </Link>
                            <button
                              className="rounded-lg border border-red-300 px-3 py-1 font-semibold text-red-700 hover:bg-red-50"
                              onClick={() => {
                                setDeleteTarget(subscriber)
                              }}
                              type="button"
                            >
                              {t('newsletter.actions.delete')}
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      <div className="mt-8 rounded-xl border border-pine/20 bg-white p-4">
        <h3 className="font-heading text-2xl text-slate">{t('newsletter.campaign.title')}</h3>
        <p className="mt-1 text-sm text-slate/75">{t('newsletter.campaign.description')}</p>

        {stats && (
          <div className="mt-4 grid gap-3 sm:grid-cols-3">
            <div className="rounded-lg border border-amber-200 bg-amber-50/40 p-3">
              <p className="text-xs uppercase tracking-wide text-slate/70">{t('newsletter.stats.totalSubscribers')}</p>
              <p className="mt-1 text-xl font-semibold text-slate">{stats.totalSubscribers}</p>
            </div>
            <div className="rounded-lg border border-amber-200 bg-amber-50/40 p-3">
              <p className="text-xs uppercase tracking-wide text-slate/70">{t('newsletter.stats.confirmedSubscribers')}</p>
              <p className="mt-1 text-xl font-semibold text-slate">{stats.confirmedSubscribers}</p>
            </div>
            <div className="rounded-lg border border-amber-200 bg-amber-50/40 p-3">
              <p className="text-xs uppercase tracking-wide text-slate/70">{t('newsletter.stats.totalCampaignsSent')}</p>
              <p className="mt-1 text-xl font-semibold text-slate">{stats.totalCampaignsSent}</p>
            </div>
          </div>
        )}

        <div className="mt-4 grid gap-3">
          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('newsletter.campaign.templateLabel')}
            <select
              className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
              onChange={(event) => setSelectedTemplateKey(event.target.value)}
              value={selectedTemplateKey}
            >
              <option value="">{t('newsletter.campaign.selectPlaceholder')}</option>
              {templates.map((template) => (
                <option key={template.key} value={template.key}>
                  {template.name}
                </option>
              ))}
            </select>
          </label>

          {selectedTemplate && (
            <div className="rounded-lg border border-amber-200 bg-amber-50/40 p-3">
              <p className="text-sm font-semibold text-slate">{selectedTemplate.subject}</p>
              <p className="mt-1 text-sm text-slate/80">{selectedTemplate.description}</p>
            </div>
          )}

          {canManage && (
            <button
              className="w-fit rounded-lg bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90 disabled:cursor-not-allowed disabled:bg-pine/60"
              disabled={isSendingCampaign || templates.length === 0}
              onClick={() => {
                void onSendCampaign()
              }}
              type="button"
            >
              {isSendingCampaign ? t('newsletter.actions.sendingCampaign') : t('newsletter.actions.sendCampaign')}
            </button>
          )}

        </div>

        <div className="mt-6">
          <h4 className="font-heading text-xl text-slate">{t('newsletter.history.title')}</h4>
          <p className="mt-1 text-sm text-slate/75">{t('newsletter.history.description')}</p>

          <div className="mt-3 overflow-x-auto">
              <TableToolbar
                filterPlaceholder={t('newsletter.history.templateKey')}
                filterText={historyFilterText}
                isExporting={isExportingHistoryTable}
                onExportDocx={() => {
                  void exportHistoryTable('docx')
                }}
                onExportExcel={() => {
                  void exportHistoryTable('excel')
                }}
                onExportPdf={() => {
                  void exportHistoryTable('pdf')
                }}
                onFilterTextChange={(value) => {
                  setHistoryFilterText(value)
                  setHistoryPage(1)
                }}
                onNextPage={() => setHistoryPage((current) => Math.min(historyPagination.totalPages, current + 1))}
                onPageSizeChange={(value) => {
                  setHistoryPageSize(value)
                  setHistoryPage(1)
                }}
                onPreviousPage={() => setHistoryPage((current) => Math.max(1, current - 1))}
                page={historyPagination.currentPage}
                pageSize={historyPageSize}
                totalPages={historyPagination.totalPages}
              />

              <table className="min-w-full divide-y divide-amber-200">
                <thead>
                  <tr className="text-left text-xs uppercase tracking-wide text-slate/65">
                    <th className="py-2 pr-3">{t('newsletter.history.templateKey')}</th>
                    <th className="py-2 pr-3">{t('newsletter.history.recipients')}</th>
                    <th className="py-2 pr-3">{t('newsletter.history.sentAtUtc')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-amber-100">
                  {historyPagination.items.length === 0 ? (
                    <tr>
                      <td className="py-8 text-center text-sm text-slate/70" colSpan={3}>
                        {t('newsletter.history.empty')}
                      </td>
                    </tr>
                  ) : (
                    historyPagination.items.map((item) => (
                      <tr key={item.id} className="text-sm text-slate/90">
                        <td className="py-2 pr-3 font-semibold">{item.templateKey}</td>
                        <td className="py-2 pr-3">{item.recipientsCount}</td>
                        <td className="py-2 pr-3">{new Date(item.sentAtUtc).toLocaleString('en-US')}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
          </div>
        </div>
      </div>

      <ConfirmDialog
        cancelLabel={t('common.cancel')}
        confirmLabel={t('common.delete')}
        description={t('newsletter.messages.confirmDelete')}
        isOpen={deleteTarget !== null}
        onCancel={() => setDeleteTarget(null)}
        onConfirm={() => {
          if (!deleteTarget) {
            return
          }

          void onDelete(deleteTarget)
          setDeleteTarget(null)
        }}
        title={t('newsletter.actions.delete')}
      />
    </section>
  )
}
