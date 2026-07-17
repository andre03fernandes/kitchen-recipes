import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { t } from '../../i18n/text'
import { getRecipeById } from '../../services/recipesApi'
import type { RecipeDto } from '../../types/recipes'

type RecipeDetailsPageProps = {
  canManage?: boolean
}

export function RecipeDetailsPage({ canManage = false }: RecipeDetailsPageProps) {
  const params = useParams<{ id: string }>()
  const recipeId = useMemo(() => Number(params.id), [params.id])

  const [recipe, setRecipe] = useState<RecipeDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [feedback, setFeedback] = useState('')

  useEffect(() => {
    async function loadRecipe() {
      if (!Number.isInteger(recipeId) || recipeId <= 0) {
        setFeedback(t('recipes.errors.loadDetailsFailed'))
        setIsLoading(false)
        return
      }

      try {
        const data = await getRecipeById(recipeId)
        setRecipe(data)
      } catch {
        setFeedback(t('recipes.errors.loadDetailsFailed'))
      } finally {
        setIsLoading(false)
      }
    }

    void loadRecipe()
  }, [recipeId])

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{t('recipes.details.title')}</h2>
          <p className="mt-2 text-slate/80">{t('recipes.details.description')}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {canManage && recipe && (
            <Link className="rounded-xl border border-pine/30 px-4 py-2 font-semibold text-pine hover:bg-pine/5" to={`/recipes/${recipe.id}/edit`}>
              {t('recipes.actions.edit')}
            </Link>
          )}
          <Link className="rounded-xl border border-slate-300 px-4 py-2 font-semibold text-slate hover:bg-slate-50" to="/recipes">
            {t('recipes.actions.backToList')}
          </Link>
        </div>
      </div>

      {feedback && <p className="mb-4 rounded-lg bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">{feedback}</p>}

      {isLoading ? (
        <p className="text-slate/70">{t('recipes.messages.loading')}</p>
      ) : !recipe ? (
        <p className="text-slate/70">{t('recipes.errors.loadDetailsFailed')}</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="rounded-xl border border-amber-200 bg-white p-4 sm:col-span-2">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('recipes.fields.image')}</p>
            {recipe.imageUrl ? (
              <img alt={recipe.name} className="mt-2 h-40 w-40 rounded-lg object-cover" src={recipe.imageUrl} />
            ) : (
              <p className="mt-1 text-slate/70">{t('recipes.messages.noImage')}</p>
            )}
          </div>

          <div className="rounded-xl border border-amber-200 bg-amber-50/40 p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('recipes.fields.name')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{recipe.name}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('recipes.fields.preparationMinutes')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{recipe.preparationMinutes}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('recipes.fields.servings')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{recipe.servings}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('recipes.details.createdAtUtc')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{new Date(recipe.createdAtUtc).toLocaleString('en-US')}</p>
          </div>

          <div className="rounded-xl border border-amber-200 bg-white p-4 sm:col-span-2">
            <p className="text-xs uppercase tracking-wide text-slate/70">{t('recipes.fields.description')}</p>
            <p className="mt-1 text-slate/90">{recipe.description || t('recipes.messages.noDescription')}</p>
          </div>
        </div>
      )}
    </section>
  )
}
