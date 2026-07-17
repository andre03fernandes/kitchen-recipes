import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ConfirmDialog } from '../../components/dialogs/ConfirmDialog'
import { TableToolbar } from '../../components/table/TableToolbar'
import { deleteRecipe, getRecipes } from '../../services/recipesApi'
import { t } from '../../i18n/text'
import type { RecipeDto } from '../../types/recipes'
import {
  DEFAULT_TABLE_PAGE_SIZE,
  applySimpleFilter,
  exportItemsToDocx,
  exportItemsToExcel,
  exportItemsToPdf,
  paginateItems,
} from '../../utils/tableTools'

type RecipeListPageProps = {
  canManage?: boolean
}

type RecipeListViewMode = 'catalog' | 'table'

function getRecipePrepBadge(preparationMinutes: number) {
  if (preparationMinutes <= 20) {
    return { label: t('recipes.catalog.badges.fast'), className: 'bg-emerald-100 text-emerald-800' }
  }

  if (preparationMinutes <= 45) {
    return { label: t('recipes.catalog.badges.standard'), className: 'bg-sky-100 text-sky-800' }
  }

  return { label: t('recipes.catalog.badges.slowCook'), className: 'bg-orange-100 text-orange-800' }
}

function getRecipeRelevanceScore(recipe: RecipeDto) {
  let score = 0

  if (recipe.imageUrl) {
    score += 40
  }

  if (recipe.preparationMinutes <= 20) {
    score += 30
  } else if (recipe.preparationMinutes <= 45) {
    score += 20
  } else {
    score += 10
  }

  if (recipe.description.trim().length > 0) {
    score += 5
  }

  return score
}

export function RecipeListPage({ canManage = false }: RecipeListPageProps) {
  const [recipes, setRecipes] = useState<RecipeDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isExporting, setIsExporting] = useState(false)
  const [filterText, setFilterText] = useState('')
  const [viewMode, setViewMode] = useState<RecipeListViewMode>('catalog')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(DEFAULT_TABLE_PAGE_SIZE)
  const [feedback, setFeedback] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<RecipeDto | null>(null)

  async function loadRecipes() {
    setIsLoading(true)
    try {
      const data = await getRecipes()
      setRecipes(data)
    } catch {
      setFeedback(t('recipes.errors.loadFailed'))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadRecipes()
  }, [])

  async function onDelete(recipe: RecipeDto) {
    try {
      await deleteRecipe(recipe.id)
      setFeedback(t('recipes.messages.deleted'))
      await loadRecipes()
    } catch {
      setFeedback(t('recipes.errors.deleteFailed'))
    }
  }

  async function exportTable(format: 'pdf' | 'docx' | 'excel') {
    const timestamp = new Date().toISOString().replaceAll(':', '-')
    const columns = [
      { header: t('recipes.fields.name'), value: (item: RecipeDto) => item.name },
      { header: t('recipes.fields.description'), value: (item: RecipeDto) => item.description || t('recipes.messages.noDescription') },
      { header: t('recipes.fields.preparationMinutes'), value: (item: RecipeDto) => item.preparationMinutes },
      { header: t('recipes.fields.servings'), value: (item: RecipeDto) => item.servings },
    ]

    setIsExporting(true)

    try {
      if (format === 'pdf') {
        exportItemsToPdf(filteredRecipes, columns, t('recipes.list.title'), `recipes-${timestamp}.pdf`)
      } else if (format === 'docx') {
        await exportItemsToDocx(filteredRecipes, columns, t('recipes.list.title'), `recipes-${timestamp}.docx`)
      } else {
        exportItemsToExcel(filteredRecipes, columns, 'Recipes', `recipes-${timestamp}.xlsx`)
      }

      setFeedback(t('tables.exportSuccess'))
    } catch {
      setFeedback(t('tables.exportError'))
    } finally {
      setIsExporting(false)
    }
  }

  const filteredRecipes = useMemo(
    () =>
      applySimpleFilter(recipes, filterText, (recipe) => [
        recipe.name,
        recipe.description,
        recipe.preparationMinutes,
        recipe.servings,
      ]),
    [filterText, recipes],
  )

  const sortedRecipes = useMemo(
    () =>
      [...filteredRecipes].sort((left, right) => {
        const scoreDelta = getRecipeRelevanceScore(right) - getRecipeRelevanceScore(left)
        if (scoreDelta !== 0) {
          return scoreDelta
        }

        return left.name.localeCompare(right.name)
      }),
    [filteredRecipes],
  )

  const pagination = useMemo(() => paginateItems(sortedRecipes, page, pageSize), [sortedRecipes, page, pageSize])

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{t('recipes.list.title')}</h2>
          <p className="mt-2 text-slate/80">{t('recipes.list.description')}</p>
        </div>
        {canManage && (
          <div className="flex flex-wrap gap-2">
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
            <Link className="rounded-xl bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90" to="/recipes/new">
              {t('recipes.actions.create')}
            </Link>
          </div>
        )}
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
                <div className="h-4 w-5/6 animate-pulse rounded bg-amber-100/70" />
                <div className="h-10 w-full animate-pulse rounded bg-amber-100/70" />
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="mt-6 space-y-4">
          <TableToolbar
            filterPlaceholder={t('recipes.fields.name')}
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

          {viewMode === 'table' ? (
            <div className="overflow-x-auto rounded-xl border border-amber-200 bg-white">
              <table className="min-w-full divide-y divide-amber-200">
                <thead>
                  <tr className="text-left text-sm uppercase tracking-wide text-slate/65">
                    <th className="py-2 pl-3 pr-3">{t('recipes.fields.image')}</th>
                    <th className="py-2 pr-3">{t('recipes.fields.name')}</th>
                    <th className="py-2 pr-3">{t('recipes.fields.description')}</th>
                    <th className="py-2 pr-3">{t('recipes.fields.preparationMinutes')}</th>
                    <th className="py-2 pr-3">{t('recipes.fields.servings')}</th>
                    <th className="py-2 pr-3">{t('recipes.fields.actions')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-amber-100">
                  {pagination.items.length === 0 ? (
                    <tr>
                      <td className="py-8 text-center text-sm text-slate/70" colSpan={6}>
                        {t('recipes.messages.empty')}
                      </td>
                    </tr>
                  ) : (
                    pagination.items.map((recipe) => (
                      <tr key={recipe.id} className="text-sm text-slate/90">
                        <td className="py-3 pl-3 pr-3">
                          {recipe.imageUrl ? (
                            <img alt={recipe.name} className="h-12 w-12 rounded-lg object-cover" src={recipe.imageUrl} />
                          ) : (
                            <span className="text-xs text-slate/60">{t('recipes.messages.noImage')}</span>
                          )}
                        </td>
                        <td className="py-3 pr-3 font-semibold">{recipe.name}</td>
                        <td className="max-w-xs py-3 pr-3">{recipe.description || t('recipes.messages.noDescription')}</td>
                        <td className="py-3 pr-3">{recipe.preparationMinutes}</td>
                        <td className="py-3 pr-3">{recipe.servings}</td>
                        <td className="py-3 pr-3">
                          <div className="flex flex-wrap gap-2">
                            <Link className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-ember hover:bg-amber-50" to={`/recipes/${recipe.id}`}>
                              {t('recipes.actions.details')}
                            </Link>
                            {canManage && (
                              <>
                                <Link className="rounded-lg border border-pine/30 px-3 py-1 font-semibold text-pine hover:bg-pine/5" to={`/recipes/${recipe.id}/edit`}>
                                  {t('recipes.actions.edit')}
                                </Link>
                                <button
                                  className="rounded-lg border border-red-300 px-3 py-1 font-semibold text-red-700 hover:bg-red-50"
                                  onClick={() => {
                                    setDeleteTarget(recipe)
                                  }}
                                  type="button"
                                >
                                  {t('recipes.actions.delete')}
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
                  {t('recipes.messages.empty')}
                </div>
              ) : pagination.items.map((recipe, index) => (
              <article
                key={recipe.id}
                className="catalog-card-enter group flex h-full flex-col overflow-hidden rounded-2xl border border-amber-200 bg-white shadow-sm transition duration-200 hover:-translate-y-1 hover:shadow-xl"
                style={{ animationDelay: `${index * 70}ms` }}
              >
                <div className="relative h-44 w-full bg-amber-100/60">
                  {recipe.imageUrl ? (
                    <img alt={recipe.name} className="h-full w-full object-cover" src={recipe.imageUrl} />
                  ) : (
                    <div className="flex h-full items-center justify-center px-4 text-center text-sm font-semibold text-slate/60">{t('recipes.messages.noImage')}</div>
                  )}
                  <div className="absolute inset-x-0 bottom-0 flex translate-y-2 justify-center gap-2 bg-gradient-to-t from-black/40 via-black/10 to-transparent p-3 opacity-0 transition duration-200 group-hover:translate-y-0 group-hover:opacity-100">
                    <Link className="rounded-lg border border-white/70 bg-white/90 px-3 py-1 text-xs font-bold uppercase tracking-wide text-slate hover:bg-white" to={`/recipes/${recipe.id}`}>
                      {t('recipes.actions.details')}
                    </Link>
                    {canManage && (
                      <Link className="rounded-lg border border-white/70 bg-pine/90 px-3 py-1 text-xs font-bold uppercase tracking-wide text-white hover:bg-pine" to={`/recipes/${recipe.id}/edit`}>
                        {t('recipes.actions.edit')}
                      </Link>
                    )}
                  </div>
                  <div className="absolute right-3 top-3 rounded-full bg-white/95 px-3 py-1 text-xs font-bold uppercase tracking-wide text-slate">
                    {recipe.preparationMinutes} min
                  </div>
                </div>

                <div className="flex flex-1 flex-col space-y-3 p-4">
                  <div>
                    <span className={`inline-flex rounded-full px-2.5 py-1 text-[11px] font-bold uppercase tracking-wide ${getRecipePrepBadge(recipe.preparationMinutes).className}`}>
                      {getRecipePrepBadge(recipe.preparationMinutes).label}
                    </span>
                    <h3 className="line-clamp-1 font-heading text-2xl text-slate">{recipe.name}</h3>
                    <p className="mt-1 line-clamp-2 text-sm text-slate/75">{recipe.description || t('recipes.messages.noDescription')}</p>
                  </div>

                  <div className="flex items-center justify-between rounded-lg border border-amber-200 bg-amber-50/50 px-3 py-2 text-sm">
                    <span className="font-semibold text-slate/70">{t('recipes.fields.servings')}</span>
                    <span className="font-bold text-slate">{recipe.servings}</span>
                  </div>

                  <div className="mt-auto flex flex-wrap gap-2 pt-1">
                    <Link className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-ember hover:bg-amber-50" to={`/recipes/${recipe.id}`}>
                      {t('recipes.actions.details')}
                    </Link>
                    {canManage && (
                      <>
                        <Link className="rounded-lg border border-pine/30 px-3 py-1 font-semibold text-pine hover:bg-pine/5" to={`/recipes/${recipe.id}/edit`}>
                          {t('recipes.actions.edit')}
                        </Link>
                        <button
                          className="rounded-lg border border-red-300 px-3 py-1 font-semibold text-red-700 hover:bg-red-50"
                          onClick={() => {
                            setDeleteTarget(recipe)
                          }}
                          type="button"
                        >
                          {t('recipes.actions.delete')}
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
        description={t('recipes.messages.confirmDelete')}
        isOpen={deleteTarget !== null}
        onCancel={() => setDeleteTarget(null)}
        onConfirm={() => {
          if (!deleteTarget) {
            return
          }

          void onDelete(deleteTarget)
          setDeleteTarget(null)
        }}
        title={t('recipes.actions.delete')}
      />
    </section>
  )
}
