import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { createRecipe, getRecipeById, updateRecipe } from '../../services/recipesApi'
import { t } from '../../i18n/text'
import { readImageFileAsDataUrl } from '../../utils/imageUpload'

type FormState = {
  name: string
  description: string
  imageUrl: string
  preparationMinutes: string
  servings: string
}

const emptyState: FormState = {
  name: '',
  description: '',
  imageUrl: '',
  preparationMinutes: '30',
  servings: '2',
}

export function RecipeFormPage() {
  const params = useParams<{ id: string }>()
  const navigate = useNavigate()
  const recipeId = useMemo(() => Number(params.id), [params.id])
  const isEditMode = Number.isInteger(recipeId) && recipeId > 0

  const [form, setForm] = useState<FormState>(emptyState)
  const [isLoading, setIsLoading] = useState(isEditMode)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [feedback, setFeedback] = useState('')

  useEffect(() => {
    if (!isEditMode) {
      return
    }

    async function loadRecipe() {
      try {
        const recipe = await getRecipeById(recipeId)
        setForm({
          name: recipe.name,
          description: recipe.description,
          imageUrl: recipe.imageUrl ?? '',
          preparationMinutes: recipe.preparationMinutes.toString(),
          servings: recipe.servings.toString(),
        })
      } catch {
        setFeedback(t('recipes.errors.loadDetailsFailed'))
      } finally {
        setIsLoading(false)
      }
    }

    void loadRecipe()
  }, [isEditMode, recipeId])

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
      description: form.description,
      imageUrl: form.imageUrl.trim() ? form.imageUrl : null,
      preparationMinutes: Number(form.preparationMinutes),
      servings: Number(form.servings),
    }

    try {
      if (isEditMode) {
        await updateRecipe(recipeId, payload)
      } else {
        await createRecipe(payload)
      }

      navigate('/recipes')
    } catch {
      setFeedback(t('recipes.errors.saveFailed'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{isEditMode ? t('recipes.form.editTitle') : t('recipes.form.createTitle')}</h2>
          <p className="mt-2 text-slate/80">{t('recipes.form.description')}</p>
        </div>
        <Link className="rounded-xl border border-pine/30 px-4 py-2 font-semibold text-pine hover:bg-pine/5" to="/recipes">
          {t('recipes.actions.backToList')}
        </Link>
      </div>

      {feedback && <p className="mb-4 rounded-lg bg-red-50 px-3 py-2 text-sm font-semibold text-red-700">{feedback}</p>}

      {isLoading ? (
        <p className="text-slate/70">{t('recipes.messages.loading')}</p>
      ) : (
        <form className="grid gap-4" onSubmit={onSubmit}>
          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('recipes.fields.name')}
            <input
              className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
              maxLength={120}
              onChange={(event) => updateField('name', event.target.value)}
              required
              type="text"
              value={form.name}
            />
          </label>

          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('recipes.fields.description')}
            <textarea
              className="min-h-24 rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
              maxLength={1000}
              onChange={(event) => updateField('description', event.target.value)}
              value={form.description}
            />
          </label>

          <label className="grid gap-2 text-sm font-semibold text-slate">
            {t('recipes.fields.image')}
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
                    setFeedback(t('recipes.errors.invalidImageFile'))
                  }
                })()
              }}
              type="file"
            />
            {form.imageUrl && (
              <div className="flex flex-wrap items-center gap-3 rounded-lg border border-amber-200 bg-amber-50/40 p-3">
                <img alt={form.name || t('recipes.details.title')} className="h-20 w-20 rounded-lg object-cover" src={form.imageUrl} />
                <button
                  className="rounded-lg border border-slate-300 px-3 py-1 text-sm font-semibold text-slate hover:bg-slate-50"
                  onClick={() => updateField('imageUrl', '')}
                  type="button"
                >
                  {t('recipes.actions.removeImage')}
                </button>
              </div>
            )}
          </label>

          <div className="grid gap-4 sm:grid-cols-2">
            <label className="grid gap-2 text-sm font-semibold text-slate">
              {t('recipes.fields.preparationMinutes')}
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                min={1}
                onChange={(event) => updateField('preparationMinutes', event.target.value)}
                required
                type="number"
                value={form.preparationMinutes}
              />
            </label>

            <label className="grid gap-2 text-sm font-semibold text-slate">
              {t('recipes.fields.servings')}
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                min={1}
                onChange={(event) => updateField('servings', event.target.value)}
                required
                type="number"
                value={form.servings}
              />
            </label>
          </div>

          <div className="mt-2 flex flex-wrap gap-3">
            <button
              className="rounded-xl bg-pine px-5 py-2 font-semibold text-white transition hover:bg-pine/90 disabled:cursor-not-allowed disabled:bg-pine/60"
              disabled={isSubmitting}
              type="submit"
            >
              {isSubmitting ? t('recipes.actions.saving') : t('recipes.actions.save')}
            </button>
            <Link className="rounded-xl border border-slate-300 px-5 py-2 font-semibold text-slate hover:bg-slate-50" to="/recipes">
              {t('recipes.actions.cancel')}
            </Link>
          </div>
        </form>
      )}
    </section>
  )
}
