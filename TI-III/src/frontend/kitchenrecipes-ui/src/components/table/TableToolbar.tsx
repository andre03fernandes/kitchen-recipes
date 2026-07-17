import { t } from '../../i18n/text'
import { TABLE_PAGE_SIZE_OPTIONS } from '../../utils/tableTools'

type TableToolbarProps = {
  filterText: string
  filterPlaceholder: string
  onFilterTextChange: (value: string) => void
  page: number
  totalPages: number
  pageSize: number
  onPageSizeChange: (value: number) => void
  onPreviousPage: () => void
  onNextPage: () => void
  onExportPdf: () => void
  onExportDocx: () => void
  onExportExcel: () => void
  isExporting?: boolean
}

export function TableToolbar({
  filterText,
  filterPlaceholder,
  onFilterTextChange,
  page,
  totalPages,
  pageSize,
  onPageSizeChange,
  onPreviousPage,
  onNextPage,
  onExportPdf,
  onExportDocx,
  onExportExcel,
  isExporting = false,
}: TableToolbarProps) {
  return (
    <div className="mb-4 grid gap-3 rounded-xl border border-amber-200 bg-amber-50/40 p-3">
      <div className="grid gap-3 md:grid-cols-2">
        <label className="grid gap-2 text-sm font-semibold text-slate">
          {t('tables.filterLabel')}
          <input
            className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
            onChange={(event) => onFilterTextChange(event.target.value)}
            placeholder={filterPlaceholder}
            type="text"
            value={filterText}
          />
        </label>

        <label className="grid gap-2 text-sm font-semibold text-slate">
          {t('tables.rowsPerPage')}
          <select
            className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
            onChange={(event) => onPageSizeChange(Number(event.target.value))}
            value={pageSize}
          >
            {TABLE_PAGE_SIZE_OPTIONS.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <button
          className="rounded-lg border border-pine/30 px-3 py-1.5 text-sm font-semibold text-pine hover:bg-pine/5"
          disabled={isExporting}
          onClick={onExportPdf}
          type="button"
        >
          PDF
        </button>
        <button
          className="rounded-lg border border-pine/30 px-3 py-1.5 text-sm font-semibold text-pine hover:bg-pine/5"
          disabled={isExporting}
          onClick={onExportDocx}
          type="button"
        >
          DOCX
        </button>
        <button
          className="rounded-lg border border-pine/30 px-3 py-1.5 text-sm font-semibold text-pine hover:bg-pine/5"
          disabled={isExporting}
          onClick={onExportExcel}
          type="button"
        >
          Excel
        </button>
        {isExporting && <span className="text-sm text-slate/70">{t('tables.exporting')}</span>}
      </div>

      <div className="flex items-center justify-end gap-2 text-sm">
        <button
          className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-amber-700 hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-60"
          disabled={page <= 1}
          onClick={onPreviousPage}
          type="button"
        >
          {t('tables.previous')}
        </button>
        <span className="font-semibold text-slate/80">
          {t('tables.page')} {page} {t('tables.of')} {totalPages}
        </span>
        <button
          className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-amber-700 hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-60"
          disabled={page >= totalPages}
          onClick={onNextPage}
          type="button"
        >
          {t('tables.next')}
        </button>
      </div>
    </div>
  )
}
