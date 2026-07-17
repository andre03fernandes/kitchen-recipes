import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ConfirmDialog } from '../../components/dialogs/ConfirmDialog'
import { TableToolbar } from '../../components/table/TableToolbar'
import { t } from '../../i18n/text'
import { deletePantryItem, getPantryItems } from '../../services/pantryApi'
import type { PantryItemDto } from '../../types/pantry'
import {
  DEFAULT_TABLE_PAGE_SIZE,
  applySimpleFilter,
  exportItemsToDocx,
  exportItemsToExcel,
  exportItemsToPdf,
  paginateItems,
} from '../../utils/tableTools'

type PantryListPageProps = {
  canManage?: boolean
}

type ExpiryFilter = 'all' | 'noExpiry' | 'expired' | 'expiringSoon' | 'fresh'
type ExpiryCategory = Exclude<ExpiryFilter, 'all'>
type PantryListViewMode = 'catalog' | 'table'

function getExpiryCategory(expirationDate: string | null): ExpiryCategory {
  if (!expirationDate) {
    return 'noExpiry'
  }

  const today = new Date()
  today.setHours(0, 0, 0, 0)

  const expiry = new Date(expirationDate)
  expiry.setHours(0, 0, 0, 0)

  const daysUntilExpiry = Math.round((expiry.getTime() - today.getTime()) / 86400000)

  if (daysUntilExpiry < 0) {
    return 'expired'
  }

  if (daysUntilExpiry <= 3) {
    return 'expiringSoon'
  }

  return 'fresh'
}

function getExpiryBadge(expirationDate: string | null) {
  const category = getExpiryCategory(expirationDate)

  if (category === 'noExpiry') {
    return { label: t('pantry.catalog.badges.noExpiry'), className: 'pantry-expiry-badge pantry-expiry-badge-no-expiry' }
  }

  if (category === 'expired') {
    return { label: t('pantry.catalog.badges.expired'), className: 'pantry-expiry-badge pantry-expiry-badge-expired' }
  }

  if (category === 'expiringSoon') {
    return { label: t('pantry.catalog.badges.expiringSoon'), className: 'pantry-expiry-badge pantry-expiry-badge-expiring' }
  }

  return { label: t('pantry.catalog.badges.fresh'), className: 'pantry-expiry-badge pantry-expiry-badge-fresh' }
}

export function PantryListPage({ canManage = false }: PantryListPageProps) {
  const [items, setItems] = useState<PantryItemDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isExporting, setIsExporting] = useState(false)
  const [filterText, setFilterText] = useState('')
  const [expiryFilter, setExpiryFilter] = useState<ExpiryFilter>('all')
  const [viewMode, setViewMode] = useState<PantryListViewMode>('catalog')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(DEFAULT_TABLE_PAGE_SIZE)
  const [feedback, setFeedback] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<PantryItemDto | null>(null)

  async function loadItems() {
    setIsLoading(true)
    try {
      const data = await getPantryItems()
      setItems(data)
    } catch {
      setFeedback(t('pantry.errors.loadFailed'))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadItems()
  }, [])

  async function onDelete(item: PantryItemDto) {
    try {
      await deletePantryItem(item.id)
      setFeedback(t('pantry.messages.deleted'))
      await loadItems()
    } catch {
      setFeedback(t('pantry.errors.deleteFailed'))
    }
  }

  async function exportTable(format: 'pdf' | 'docx' | 'excel') {
    const timestamp = new Date().toISOString().replaceAll(':', '-')
    const columns = [
      { header: t('pantry.fields.name'), value: (item: PantryItemDto) => item.name },
      { header: t('pantry.fields.availableQuantity'), value: (item: PantryItemDto) => item.availableQuantity },
      { header: t('pantry.fields.unit'), value: (item: PantryItemDto) => item.unit },
      { header: t('pantry.fields.expirationDate'), value: (item: PantryItemDto) => item.expirationDate ?? t('pantry.messages.noExpirationDate') },
    ]

    setIsExporting(true)

    try {
      if (format === 'pdf') {
        exportItemsToPdf(filteredItems, columns, t('pantry.list.title'), `pantry-items-${timestamp}.pdf`)
      } else if (format === 'docx') {
        await exportItemsToDocx(filteredItems, columns, t('pantry.list.title'), `pantry-items-${timestamp}.docx`)
      } else {
        exportItemsToExcel(filteredItems, columns, 'PantryItems', `pantry-items-${timestamp}.xlsx`)
      }

      setFeedback(t('tables.exportSuccess'))
    } catch {
      setFeedback(t('tables.exportError'))
    } finally {
      setIsExporting(false)
    }
  }

  const textFilteredItems = useMemo(
    () =>
      applySimpleFilter(items, filterText, (item) => [
        item.name,
        item.availableQuantity,
        item.unit,
        item.expirationDate,
      ]),
    [filterText, items],
  )

  const filteredItems = useMemo(() => {
    if (expiryFilter === 'all') {
      return textFilteredItems
    }

    return textFilteredItems.filter((item) => getExpiryCategory(item.expirationDate) === expiryFilter)
  }, [expiryFilter, textFilteredItems])

  const sortedItems = useMemo(() => {
    const priority: Record<ExpiryCategory, number> = {
      expiringSoon: 0,
      expired: 1,
      fresh: 2,
      noExpiry: 3,
    }

    return [...filteredItems].sort((left, right) => {
      const leftCategory = getExpiryCategory(left.expirationDate)
      const rightCategory = getExpiryCategory(right.expirationDate)
      const priorityDelta = priority[leftCategory] - priority[rightCategory]

      if (priorityDelta !== 0) {
        return priorityDelta
      }

      if (left.expirationDate && right.expirationDate) {
        const dateDelta = new Date(left.expirationDate).getTime() - new Date(right.expirationDate).getTime()
        if (dateDelta !== 0) {
          return dateDelta
        }
      }

      return left.name.localeCompare(right.name)
    })
  }, [filteredItems])

  const pagination = useMemo(() => paginateItems(sortedItems, page, pageSize), [sortedItems, page, pageSize])

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{t('pantry.list.title')}</h2>
          <p className="mt-2 text-slate/80">{t('pantry.list.description')}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {canManage && (
            <>
              <button
                className={`rounded-xl border px-3 py-2 text-sm font-semibold transition ${viewMode === 'catalog' ? 'border-pine bg-pine text-white' : 'border-pine/30 bg-white text-pine hover:bg-pine/5'}`}
                onClick={() => setViewMode('catalog')}
                type="button"
              >
                {t('tables.viewCatalog')}
              </button>
              <button
                className={`rounded-xl border px-3 py-2 text-sm font-semibold transition ${viewMode === 'table' ? 'border-pine bg-pine text-white' : 'border-pine/30 bg-white text-pine hover:bg-pine/5'}`}
                onClick={() => setViewMode('table')}
                type="button"
              >
                {t('tables.viewTable')}
              </button>
            </>
          )}
          <Link className="rounded-xl border border-pine/30 bg-white px-4 py-2 font-semibold text-pine transition hover:bg-pine/5" to="/assistant">
            {t('pantry.actions.assistant')}
          </Link>
          {canManage && (
            <Link className="rounded-xl bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90" to="/pantry/new">
              {t('pantry.actions.create')}
            </Link>
          )}
        </div>
      </div>

      {feedback && <p className="mt-4 rounded-lg bg-amber-50 px-3 py-2 text-sm font-semibold text-ember">{feedback}</p>}

      {isLoading ? (
        <div className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: Math.min(pageSize, 6) }).map((_, index) => (
            <div key={index} className="overflow-hidden rounded-2xl border border-amber-200 bg-white shadow-sm">
              <div className="h-44 w-full animate-pulse bg-amber-100/70" />
              <div className="space-y-3 p-4">
                <div className="h-6 w-2/3 animate-pulse rounded bg-amber-100/70" />
                <div className="h-4 w-full animate-pulse rounded bg-amber-100/70" />
                <div className="h-10 w-full animate-pulse rounded bg-amber-100/70" />
                <div className="h-9 w-2/3 animate-pulse rounded bg-amber-100/70" />
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="mt-6 space-y-4">
          <TableToolbar
            filterPlaceholder={t('pantry.fields.name')}
            filterText={filterText}
            isExporting={isExporting}
            onExportDocx={() => {
              void exportTable('docx')
            }}
            onExportExcel={() => {
              void exportTable('excel')
            }}
            onExportPdf={() => {
              void exportTable('pdf')
            }}
            onFilterTextChange={(value) => {
              setFilterText(value)
              setPage(1)
            }}
            onNextPage={() => setPage((current) => Math.min(pagination.totalPages, current + 1))}
            onPageSizeChange={(value) => {
              setPageSize(value)
              setPage(1)
            }}
            onPreviousPage={() => setPage((current) => Math.max(1, current - 1))}
            page={pagination.currentPage}
            pageSize={pageSize}
            totalPages={pagination.totalPages}
          />

          <div className="flex flex-wrap gap-2">
            <button
              className={`rounded-full border px-3 py-1 text-xs font-bold uppercase tracking-wide ${expiryFilter === 'all' ? 'border-pine bg-pine text-white' : 'border-amber-300 bg-white text-slate hover:bg-amber-50'}`}
              onClick={() => {
                setExpiryFilter('all')
                setPage(1)
              }}
              type="button"
            >
              {t('pantry.catalog.filters.all')}
            </button>
            <button
              className={`rounded-full border px-3 py-1 text-xs font-bold uppercase tracking-wide ${expiryFilter === 'expiringSoon' ? 'border-amber-400 bg-amber-100 text-amber-900' : 'border-amber-300 bg-white text-slate hover:bg-amber-50'}`}
              onClick={() => {
                setExpiryFilter('expiringSoon')
                setPage(1)
              }}
              type="button"
            >
              {t('pantry.catalog.filters.expiringSoon')}
            </button>
            <button
              className={`rounded-full border px-3 py-1 text-xs font-bold uppercase tracking-wide ${expiryFilter === 'expired' ? 'border-rose-400 bg-rose-100 text-rose-900' : 'border-amber-300 bg-white text-slate hover:bg-amber-50'}`}
              onClick={() => {
                setExpiryFilter('expired')
                setPage(1)
              }}
              type="button"
            >
              {t('pantry.catalog.filters.expired')}
            </button>
            <button
              className={`rounded-full border px-3 py-1 text-xs font-bold uppercase tracking-wide ${expiryFilter === 'fresh' ? 'border-emerald-400 bg-emerald-100 text-emerald-900' : 'border-amber-300 bg-white text-slate hover:bg-amber-50'}`}
              onClick={() => {
                setExpiryFilter('fresh')
                setPage(1)
              }}
              type="button"
            >
              {t('pantry.catalog.filters.fresh')}
            </button>
            <button
              className={`rounded-full border px-3 py-1 text-xs font-bold uppercase tracking-wide ${expiryFilter === 'noExpiry' ? 'border-slate-400 bg-slate-100 text-slate-800' : 'border-amber-300 bg-white text-slate hover:bg-amber-50'}`}
              onClick={() => {
                setExpiryFilter('noExpiry')
                setPage(1)
              }}
              type="button"
            >
              {t('pantry.catalog.filters.noExpiry')}
            </button>
          </div>

          {viewMode === 'table' ? (
            <div className="overflow-x-auto rounded-xl border border-amber-200 bg-white">
              <table className="min-w-full divide-y divide-amber-200">
                <thead>
                  <tr className="text-left text-sm uppercase tracking-wide text-slate/65">
                    <th className="py-2 pl-3 pr-3">{t('pantry.fields.image')}</th>
                    <th className="py-2 pr-3">{t('pantry.fields.name')}</th>
                    <th className="py-2 pr-3">{t('pantry.fields.availableQuantity')}</th>
                    <th className="py-2 pr-3">{t('pantry.fields.unit')}</th>
                    <th className="py-2 pr-3">{t('pantry.fields.expirationDate')}</th>
                    <th className="py-2 pr-3">{t('pantry.fields.actions')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-amber-100">
                  {pagination.items.length === 0 ? (
                    <tr>
                      <td className="py-8 text-center text-sm text-slate/70" colSpan={6}>
                        {t('pantry.messages.empty')}
                      </td>
                    </tr>
                  ) : (
                    pagination.items.map((item) => (
                      <tr key={item.id} className="text-sm text-slate/90">
                        <td className="py-3 pl-3 pr-3">
                          {item.imageUrl ? (
                            <img alt={item.name} className="h-12 w-12 rounded-lg object-cover" src={item.imageUrl} />
                          ) : (
                            <span className="text-xs text-slate/60">{t('pantry.messages.noImage')}</span>
                          )}
                        </td>
                        <td className="py-3 pr-3 font-semibold">{item.name}</td>
                        <td className="py-3 pr-3">{item.availableQuantity}</td>
                        <td className="py-3 pr-3">{item.unit}</td>
                        <td className="py-3 pr-3">{item.expirationDate ?? t('pantry.messages.noExpirationDate')}</td>
                        <td className="py-3 pr-3">
                          <div className="flex flex-wrap gap-2">
                            <Link className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-ember hover:bg-amber-50" to={`/pantry/${item.id}`}>
                              {t('pantry.actions.details')}
                            </Link>
                            {canManage && (
                              <>
                                <Link className="rounded-lg border border-pine/30 px-3 py-1 font-semibold text-pine hover:bg-pine/5" to={`/pantry/${item.id}/edit`}>
                                  {t('pantry.actions.edit')}
                                </Link>
                                <button
                                  className="rounded-lg border border-red-300 px-3 py-1 font-semibold text-red-700 hover:bg-red-50"
                                  onClick={() => {
                                      setDeleteTarget(item)
                                  }}
                                  type="button"
                                >
                                  {t('pantry.actions.delete')}
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
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {pagination.items.length === 0 ? (
                <div className="rounded-2xl border border-amber-200 bg-white/70 p-6 text-sm text-slate/70">
                  {t('pantry.messages.empty')}
                </div>
              ) : pagination.items.map((item, index) => (
              <article
                key={item.id}
                className="catalog-card-enter group flex h-full flex-col overflow-hidden rounded-2xl border border-amber-200 bg-white shadow-sm transition duration-200 hover:-translate-y-1 hover:shadow-xl"
                style={{ animationDelay: `${index * 70}ms` }}
              >
                <div className="relative h-44 w-full bg-amber-100/60">
                  {item.imageUrl ? (
                    <img alt={item.name} className="h-full w-full object-cover" src={item.imageUrl} />
                  ) : (
                    <div className="flex h-full items-center justify-center px-4 text-center text-sm font-semibold text-slate/60">{t('pantry.messages.noImage')}</div>
                  )}
                  <div className="absolute inset-x-0 bottom-0 flex translate-y-2 justify-center gap-2 bg-gradient-to-t from-black/40 via-black/10 to-transparent p-3 opacity-0 transition duration-200 group-hover:translate-y-0 group-hover:opacity-100">
                    <Link className="rounded-lg border border-white/70 bg-white/90 px-3 py-1 text-xs font-bold uppercase tracking-wide text-slate hover:bg-white" to={`/pantry/${item.id}`}>
                      {t('pantry.actions.details')}
                    </Link>
                    {canManage && (
                      <Link className="rounded-lg border border-white/70 bg-pine/90 px-3 py-1 text-xs font-bold uppercase tracking-wide text-white hover:bg-pine" to={`/pantry/${item.id}/edit`}>
                        {t('pantry.actions.edit')}
                      </Link>
                    )}
                  </div>
                  <div className="absolute right-3 top-3 rounded-full bg-white/95 px-3 py-1 text-xs font-bold uppercase tracking-wide text-slate">
                    {item.availableQuantity} {item.unit}
                  </div>
                </div>

                <div className="flex flex-1 flex-col space-y-3 p-4">
                  <div>
                    <span className={`inline-flex rounded-full px-2.5 py-1 text-[11px] font-bold uppercase tracking-wide ${getExpiryBadge(item.expirationDate).className}`}>
                      {getExpiryBadge(item.expirationDate).label}
                    </span>
                    <h3 className="line-clamp-1 font-heading text-2xl text-slate">{item.name}</h3>
                    <p className="mt-1 text-sm text-slate/75">
                      {t('pantry.fields.expirationDate')}: {item.expirationDate ?? t('pantry.messages.noExpirationDate')}
                    </p>
                  </div>

                  <div className="grid grid-cols-2 gap-2 rounded-lg border border-amber-200 bg-amber-50/50 p-2 text-sm">
                    <div>
                      <p className="text-xs uppercase tracking-wide text-slate/60">{t('pantry.fields.availableQuantity')}</p>
                      <p className="font-bold text-slate">{item.availableQuantity}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wide text-slate/60">{t('pantry.fields.unit')}</p>
                      <p className="font-bold text-slate">{item.unit}</p>
                    </div>
                  </div>

                  <div className="mt-auto flex flex-wrap gap-2 pt-1">
                    <Link className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-ember hover:bg-amber-50" to={`/pantry/${item.id}`}>
                      {t('pantry.actions.details')}
                    </Link>
                    {canManage && (
                      <>
                        <Link className="rounded-lg border border-pine/30 px-3 py-1 font-semibold text-pine hover:bg-pine/5" to={`/pantry/${item.id}/edit`}>
                          {t('pantry.actions.edit')}
                        </Link>
                        <button
                          className="rounded-lg border border-red-300 px-3 py-1 font-semibold text-red-700 hover:bg-red-50"
                          onClick={() => {
                            setDeleteTarget(item)
                          }}
                          type="button"
                        >
                          {t('pantry.actions.delete')}
                        </button>
                      </>
                    )}
                  </div>
                </div>
              </article>
              ))}
            </div>
          )}
        </div>
      )}

      <ConfirmDialog
        cancelLabel={t('common.cancel')}
        confirmLabel={t('common.delete')}
        description={t('pantry.messages.confirmDelete')}
        isOpen={deleteTarget !== null}
        onCancel={() => setDeleteTarget(null)}
        onConfirm={() => {
          if (!deleteTarget) {
            return
          }

          void onDelete(deleteTarget)
          setDeleteTarget(null)
        }}
        title={t('pantry.actions.delete')}
      />
    </section>
  )
}
