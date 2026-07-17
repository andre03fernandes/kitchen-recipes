import { Link } from 'react-router-dom'
import { BrandMark } from '../brand/BrandMark'
import { t } from '../../i18n/text'

const quickLinks = [
  { to: '/recipes', label: t('navigation.recipes') },
  { to: '/pantry', label: t('navigation.pantry') },
  { to: '/newsletter', label: t('navigation.newsletter') },
  { to: '/assistant', label: t('navigation.assistant') },
]

export function AppFooter() {
  return (
    <footer className="mt-8 rounded-3xl border border-slate-700/80 bg-slate-950/70 px-6 py-6 text-slate-200 shadow-[0_-18px_50px_rgba(0,0,0,0.18)] backdrop-blur-sm">
      <div className="grid gap-6 md:grid-cols-[1.3fr_0.9fr_0.9fr]">
        <div className="space-y-4">
          <div className="flex items-center gap-3">
            <BrandMark className="h-12 w-12 shrink-0 drop-shadow-[0_8px_20px_rgba(92,143,224,0.28)]" title={t('footer.title')} />
            <div>
              <p className="font-heading text-2xl text-slate-100">{t('footer.title')}</p>
              <p className="text-sm text-slate-400">{t('footer.subtitle')}</p>
            </div>
          </div>
          <p className="max-w-xl text-sm leading-6 text-slate-300">
            {t('footer.description')}
          </p>
        </div>

        <div>
          <p className="text-xs font-black uppercase tracking-[0.22em] text-slate-500">{t('footer.quickLinks')}</p>
          <div className="mt-4 grid gap-2">
            {quickLinks.map((item) => (
              <Link
                key={item.to}
                className="rounded-xl border border-slate-700 bg-slate-900/60 px-3 py-2 text-sm font-semibold text-slate-200 transition hover:border-sky-400/60 hover:bg-slate-800"
                to={item.to}
              >
                {item.label}
              </Link>
            ))}
          </div>
        </div>

        <div>
          <p className="text-xs font-black uppercase tracking-[0.22em] text-slate-500">{t('footer.valueTitle')}</p>
          <div className="mt-4 space-y-3 text-sm text-slate-300">
            <p>{t('footer.valueOne')}</p>
            <p>{t('footer.valueTwo')}</p>
          </div>
        </div>
      </div>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-3 border-t border-slate-800 pt-4 text-sm text-slate-400">
        <p>{t('footer.copy')}</p>
        <p>{t('footer.tagline')}</p>
      </div>
    </footer>
  )
}