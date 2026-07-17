import { Document, Packer, Paragraph, Table, TableCell, TableRow, TextRun, WidthType } from 'docx'
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'
import * as XLSX from 'xlsx'

export const DEFAULT_TABLE_PAGE_SIZE = 5
export const TABLE_PAGE_SIZE_OPTIONS = [5, 10, 20, 50]

type ExportCellValue = string | number | boolean | null | undefined

export type ExportColumn<TItem> = {
  header: string
  value: (item: TItem) => ExportCellValue
}

export type TablePagination<TItem> = {
  currentPage: number
  pageSize: number
  totalPages: number
  totalItems: number
  items: TItem[]
}

function normalizeCellValue(value: ExportCellValue): string | number | boolean {
  if (value == null) {
    return ''
  }

  return value
}

function downloadBlob(blob: Blob, filename: string) {
  const fileUrl = window.URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = fileUrl
  anchor.download = filename
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
  window.URL.revokeObjectURL(fileUrl)
}

function buildTableMatrix<TItem>(items: TItem[], columns: ExportColumn<TItem>[]) {
  const rows = items.map((item) => columns.map((column) => normalizeCellValue(column.value(item))))
  return {
    headers: columns.map((column) => column.header),
    rows,
  }
}

export function applySimpleFilter<TItem>(
  items: TItem[],
  query: string,
  getSearchTokens: (item: TItem) => Array<string | number | null | undefined>,
): TItem[] {
  const normalizedQuery = query.trim().toLowerCase()

  if (!normalizedQuery) {
    return items
  }

  return items.filter((item) =>
    getSearchTokens(item)
      .filter((token) => token != null)
      .some((token) => String(token).toLowerCase().includes(normalizedQuery)),
  )
}

export function paginateItems<TItem>(items: TItem[], page: number, pageSize: number): TablePagination<TItem> {
  const safePageSize = Math.max(1, pageSize)
  const totalItems = items.length
  const totalPages = Math.max(1, Math.ceil(totalItems / safePageSize))
  const currentPage = Math.min(Math.max(1, page), totalPages)
  const startIndex = (currentPage - 1) * safePageSize

  return {
    currentPage,
    pageSize: safePageSize,
    totalPages,
    totalItems,
    items: items.slice(startIndex, startIndex + safePageSize),
  }
}

export function exportItemsToExcel<TItem>(
  items: TItem[],
  columns: ExportColumn<TItem>[],
  worksheetName: string,
  filename: string,
) {
  const { headers, rows } = buildTableMatrix(items, columns)
  const worksheet = XLSX.utils.aoa_to_sheet([headers, ...rows])
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, worksheetName)
  XLSX.writeFile(workbook, filename)
}

export function exportItemsToPdf<TItem>(items: TItem[], columns: ExportColumn<TItem>[], title: string, filename: string) {
  const { headers, rows } = buildTableMatrix(items, columns)
  const doc = new jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a4' })

  doc.setFontSize(14)
  doc.text(title, 40, 30)

  autoTable(doc, {
    head: [headers],
    body: rows,
    startY: 45,
    styles: { fontSize: 9, cellPadding: 4 },
    headStyles: { fillColor: [26, 93, 68] },
  })

  doc.save(filename)
}

export async function exportItemsToDocx<TItem>(
  items: TItem[],
  columns: ExportColumn<TItem>[],
  title: string,
  filename: string,
) {
  const { headers, rows } = buildTableMatrix(items, columns)

  const tableRows = [
    new TableRow({
      children: headers.map(
        (header) =>
          new TableCell({
            children: [new Paragraph({ children: [new TextRun({ text: header, bold: true })] })],
          }),
      ),
    }),
    ...rows.map(
      (row) =>
        new TableRow({
          children: row.map(
            (cell) =>
              new TableCell({
                children: [new Paragraph(String(cell))],
              }),
          ),
        }),
    ),
  ]

  const doc = new Document({
    sections: [
      {
        children: [
          new Paragraph({
            children: [new TextRun({ text: title, bold: true, size: 28 })],
            spacing: { after: 300 },
          }),
          new Table({
            width: { size: 100, type: WidthType.PERCENTAGE },
            rows: tableRows,
          }),
        ],
      },
    ],
  })

  const blob = await Packer.toBlob(doc)
  downloadBlob(blob, filename)
}
