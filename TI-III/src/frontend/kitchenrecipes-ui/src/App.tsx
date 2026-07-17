import { useEffect, useState } from 'react'
import type { ReactElement } from 'react'
import { Link, NavLink, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { t } from './i18n/text'
import { BrandMark } from './components/brand/BrandMark'
import { AppFooter } from './components/layout/AppFooter'
import { AccountPage } from './pages/account/AccountPage'
import { NewsletterFormPage } from './pages/newsletter/NewsletterFormPage'
import { NewsletterListPage } from './pages/newsletter/NewsletterListPage'
import { NewsletterDetailsPage } from './pages/newsletter/NewsletterDetailsPage'
import { PantryFormPage } from './pages/pantry/PantryFormPage'
import { PantryAssistantPage } from './pages/pantry/PantryAssistantPage'
import { PantryDetailsPage } from './pages/pantry/PantryDetailsPage'
import { PantryListPage } from './pages/pantry/PantryListPage'
import { RecipeDetailsPage } from './pages/recipes/RecipeDetailsPage'
import { RecipeFormPage } from './pages/recipes/RecipeFormPage'
import { RecipeListPage } from './pages/recipes/RecipeListPage'
import { logout, me } from './services/accountApi'
import type { AuthUserDto } from './types/account'

const navItems = [
  { to: '/', label: t('navigation.home') },
  { to: '/recipes', label: t('navigation.recipes') },
  { to: '/pantry', label: t('navigation.pantry') },
  { to: '/newsletter', label: t('navigation.newsletter') },
  { to: '/assistant', label: t('navigation.assistant') },
  { to: '/account', label: t('navigation.account') },
]

type HomePageProps = {
  currentUser: AuthUserDto | null
}

type CarouselItem = {
  title: string
  caption: string
  imageUrl: string
}

function CarouselShotCard({ item, index, alt = false }: { item: CarouselItem; index: number; alt?: boolean }) {
  const [imageSrc, setImageSrc] = useState(item.imageUrl)
  const [retriedWithLocalPath, setRetriedWithLocalPath] = useState(false)
  const [imageFailed, setImageFailed] = useState(false)

  useEffect(() => {
    setImageSrc(item.imageUrl)
    setRetriedWithLocalPath(false)
    setImageFailed(false)
  }, [item.imageUrl])

  const fallbackGradients = [
    'bg-gradient-to-br from-sky-500/45 via-indigo-500/35 to-slate-900/95',
    'bg-gradient-to-br from-amber-400/40 via-orange-500/35 to-slate-900/95',
    'bg-gradient-to-br from-emerald-500/40 via-teal-500/35 to-slate-900/95',
    'bg-gradient-to-br from-rose-500/40 via-fuchsia-500/30 to-slate-900/95',
  ]
  const fallbackClass = fallbackGradients[index % fallbackGradients.length]
  const overlayClass = alt
    ? 'bg-[radial-gradient(circle_at_bottom_right,rgba(244,198,120,0.22),transparent_30%),linear-gradient(170deg,rgba(39,51,69,0.2),rgba(22,27,38,0.9))]'
    : 'bg-[radial-gradient(circle_at_top_left,rgba(148,184,255,0.25),transparent_32%),linear-gradient(160deg,rgba(33,43,61,0.24),rgba(18,24,34,0.88))]'

  return (
    <article className={`kitchen-shot-card${alt ? ' kitchen-shot-card-alt' : ''}`}>
      {!imageFailed && (
        <img
          alt={item.title}
          className="kitchen-shot-image"
          src={imageSrc}
          loading="lazy"
          onError={() => {
            if (!retriedWithLocalPath && imageSrc.startsWith('/react/')) {
              setRetriedWithLocalPath(true)
              setImageSrc(imageSrc.replace('/react/', '/'))
              return
            }

            setImageFailed(true)
          }}
        />
      )}
      {imageFailed && (
        <div className={`absolute inset-0 ${fallbackClass}`}>
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(255,255,255,0.16),transparent_50%)]" />
        </div>
      )}
      <div className={`absolute inset-0 ${overlayClass}`} />
      <div className={`absolute inset-0 flex flex-col justify-between ${alt ? 'p-4' : 'p-5'}`}>
        <div className="flex items-start justify-between gap-3">
          <p className="rounded-full border border-white/15 bg-white/8 px-3 py-1 text-[10px] font-black uppercase tracking-[0.24em] text-sky-100/80">
            {item.title}
          </p>
          {alt ? (
            <span className="text-[10px] font-black uppercase tracking-[0.24em] text-amber-100/70">Kitchen</span>
          ) : (
            <div className="rounded-full border border-amber-200/20 bg-amber-100/10 px-3 py-1 text-[10px] font-black uppercase tracking-[0.24em] text-amber-100/80">
              {index % 2 === 0 ? 'Live' : 'Mood'}
            </div>
          )}
        </div>
        <div>
          {!alt && <p className="kitchen-shot-title text-2xl">{item.title}</p>}
          <p className={`kitchen-shot-caption ${alt ? 'text-sm' : 'mt-2 text-sm'}`}>{item.caption}</p>
        </div>
      </div>
    </article>
  )
}

function HomePage({ currentUser }: HomePageProps) {
  const carouselItems: CarouselItem[] = [
    {
      title: 'Tools',
      caption: 'A quiet collection of knives, boards, and everyday utensils.',
      imageUrl: '/react/home/tools.jpg',
    },
    {
      title: 'Pizza',
      caption: 'A bright crust, molten cheese, and a hot oven finish.',
      imageUrl: '/react/home/pizza.jpg',
    },
    {
      title: 'Pasta',
      caption: 'Silky sauce, slow movement, and a generous table.',
      imageUrl: '/react/home/pasta.jpg',
    },
    {
      title: 'Market',
      caption: 'Fresh herbs, citrus, and morning light from the stalls.',
      imageUrl: '/react/home/market.jpg',
    },
    {
      title: 'Oven',
      caption: 'Heat, texture, and a perfect finishing glow.',
      imageUrl: '/react/home/oven.jpg',
    },
    {
      title: 'Innovation',
      caption: 'Classic kitchen energy with a fresher pulse.',
      imageUrl: '/react/home/ideas.jpg',
    },
  ]

  const heroCards = [
    {
      label: t('home.story1Label'),
      title: t('home.story1Title'),
      description: t('home.story1Description'),
      delay: '0ms',
    },
    {
      label: t('home.story2Label'),
      title: t('home.story2Title'),
      description: t('home.story2Description'),
      delay: '120ms',
    },
    {
      label: t('home.story3Label'),
      title: t('home.story3Title'),
      description: t('home.story3Description'),
      delay: '240ms',
    },
  ]

  const quickLinks = [
    { to: '/recipes', title: t('navigation.recipes'), description: t('home.modules.recipes') },
    { to: '/pantry', title: t('navigation.pantry'), description: t('home.modules.pantry') },
    { to: '/assistant', title: t('navigation.assistant'), description: t('home.modules.assistant') },
  ]

  return (
    <>
      <section className="kitchen-home space-y-16 pt-4 sm:pt-8">
      <div className="home-reveal overflow-hidden rounded-[2rem] border border-slate-700/80 bg-gradient-to-br from-slate-950 via-slate-900 to-slate-800 text-slate-100 shadow-[0_24px_80px_rgba(0,0,0,0.25)]" style={{ animationDelay: '0ms' }}>
        <div className="grid gap-12 lg:grid-cols-[1.05fr_0.95fr]">
          <div className="relative p-10 sm:p-14">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(148,184,255,0.18),transparent_34%),radial-gradient(circle_at_bottom_right,rgba(244,198,120,0.14),transparent_30%)]" />
            <div className="relative max-w-2xl space-y-10">
              <div className="inline-flex items-center gap-2 rounded-full border border-sky-300/20 bg-white/6 px-4 py-2 text-[11px] font-black uppercase tracking-[0.26em] text-sky-100/80">
                <span className="h-2 w-2 rounded-full bg-sky-300" />
                {t('home.kicker')}
              </div>
              <div className="space-y-6">
                <h1 className="max-w-xl font-heading text-4xl leading-[1.02] text-slate-50 sm:text-[3.4rem] lg:text-[4rem]">
                  {t('home.heroTitle')}
                </h1>
                <p className="max-w-2xl text-base leading-7 text-slate-300 sm:text-lg">
                  {t('home.heroDescription')}
                </p>
              </div>

              <div className="flex flex-wrap gap-5">
                <Link className="rounded-full bg-sky-500 px-6 py-3.5 text-sm font-black uppercase tracking-[0.18em] text-white transition hover:-translate-y-0.5 hover:bg-sky-400" to="/recipes">
                  {t('home.ctaPrimary')}
                </Link>
                <Link className="rounded-full border border-slate-600 bg-slate-900/70 px-6 py-3.5 text-sm font-black uppercase tracking-[0.18em] text-slate-100 transition hover:-translate-y-0.5 hover:border-sky-300/70 hover:bg-slate-800" to="/pantry">
                  {t('home.ctaSecondary')}
                </Link>
                <Link className="rounded-full border border-amber-200/30 bg-amber-100/10 px-6 py-3.5 text-sm font-black uppercase tracking-[0.18em] text-amber-50 transition hover:-translate-y-0.5 hover:bg-amber-100/15" to="/assistant">
                  {t('home.ctaAssistant')}
                </Link>
              </div>

              <div className="grid gap-5 sm:grid-cols-3">
                {heroCards.map((card) => (
                  <article
                    key={card.title}
                    className="home-reveal rounded-[1.4rem] border border-slate-700/80 bg-slate-900/80 p-4"
                    style={{ animationDelay: card.delay }}
                  >
                    <p className="text-[11px] font-black uppercase tracking-[0.24em] text-sky-200/70">{card.label}</p>
                    <h2 className="mt-2 font-heading text-2xl text-slate-50">{card.title}</h2>
                    <p className="mt-2 text-sm leading-6 text-slate-300">{card.description}</p>
                  </article>
                ))}
              </div>
            </div>
          </div>

          <div className="relative border-t border-slate-700/80 bg-slate-950/55 p-10 sm:p-14 lg:border-l lg:border-t-0">
            <div className="absolute inset-0 overflow-hidden">
              <div className="kitchen-home-glow kitchen-home-glow-a" />
              <div className="kitchen-home-glow kitchen-home-glow-b" />
              <div className="kitchen-home-glow kitchen-home-glow-c" />
            </div>

            <div className="relative space-y-8">
              <div className="rounded-[1.6rem] border border-slate-700 bg-slate-900/80 p-7">
                <p className="text-xs font-black uppercase tracking-[0.24em] text-sky-200/65">{t('home.feature1Label')}</p>
                <p className="mt-3 font-heading text-3xl text-slate-50">{t('home.feature1Value')}</p>
                <p className="mt-3 text-sm leading-6 text-slate-300">{t('home.feature1Description')}</p>
              </div>

              <div className="grid gap-6">
                {[
                  { label: t('home.highlight1Label'), title: t('home.highlight1Title'), description: t('home.highlight1Description') },
                  { label: t('home.highlight2Label'), title: t('home.highlight2Title'), description: t('home.highlight2Description') },
                ].map((item, index) => (
                  <article
                    key={item.title}
                    className="home-reveal rounded-[1.4rem] border border-slate-700 bg-slate-900/75 p-6"
                    style={{ animationDelay: `${120 + index * 120}ms` }}
                  >
                    <p className="text-[11px] font-black uppercase tracking-[0.24em] text-amber-200/80">{item.label}</p>
                    <h2 className="mt-2 font-heading text-2xl text-slate-50">{item.title}</h2>
                    <p className="mt-2 text-sm leading-6 text-slate-300">{item.description}</p>
                  </article>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="chef-highlights-grid gap-8">
        {[
          { label: t('home.feature2Label'), value: t('home.feature2Value'), description: t('home.feature2Description') },
          { label: t('home.feature3Label'), value: t('home.feature3Value'), description: t('home.feature3Description') },
          { label: t('home.nextMilestoneTitle'), value: t('home.nextMilestoneDescription'), description: t('home.kitchenTip') },
        ].map((item, index) => (
          <article
            key={item.label}
            className="chef-highlight-card"
            style={{ animationDelay: `${180 + index * 120}ms` }}
          >
            <div className="chef-highlight-gradient bg-gradient-to-r from-sky-300 via-slate-200 to-amber-200" />
            <div className="chef-highlight-content">
              <p className="text-[11px] font-black uppercase tracking-[0.24em] text-sky-200/70">{item.label}</p>
              <p className="mt-2 font-heading text-2xl text-slate-100">{item.value}</p>
              <p className="mt-2 text-sm leading-6 text-slate-300">{item.description}</p>
            </div>
          </article>
        ))}
      </div>

      <div className="grid gap-10 lg:grid-cols-[1fr_0.95fr]">
        <article className="home-reveal rounded-[1.8rem] border border-amber-200/70 bg-amber-50/70 p-10 shadow-sm" style={{ animationDelay: '180ms' }}>
          <p className="text-xs font-black uppercase tracking-[0.24em] text-ember/70">{t('home.journeyTitle')}</p>
          <h2 className="mt-3 font-heading text-3xl text-slate-900">{t('home.journeyDescription')}</h2>
          <div className="mt-8 space-y-6">
            <div className="rounded-2xl bg-white/80 p-6 transition duration-300 hover:-translate-y-0.5 hover:shadow-md">
              <p className="text-sm font-black uppercase tracking-[0.18em] text-pine/80">{t('home.step1Label')}</p>
              <p className="mt-1 font-heading text-2xl text-slate-900">{t('home.step1Title')}</p>
              <p className="mt-2 text-sm leading-6 text-slate-700">{t('home.step1Description')}</p>
            </div>
            <div className="rounded-2xl bg-white/80 p-6 transition duration-300 hover:-translate-y-0.5 hover:shadow-md">
              <p className="text-sm font-black uppercase tracking-[0.18em] text-pine/80">{t('home.step2Label')}</p>
              <p className="mt-1 font-heading text-2xl text-slate-900">{t('home.step2Title')}</p>
              <p className="mt-2 text-sm leading-6 text-slate-700">{t('home.step2Description')}</p>
            </div>
            <div className="rounded-2xl bg-white/80 p-6 transition duration-300 hover:-translate-y-0.5 hover:shadow-md">
              <p className="text-sm font-black uppercase tracking-[0.18em] text-pine/80">{t('home.step3Label')}</p>
              <p className="mt-1 font-heading text-2xl text-slate-900">{t('home.step3Title')}</p>
              <p className="mt-2 text-sm leading-6 text-slate-700">{t('home.step3Description')}</p>
            </div>
          </div>
        </article>

        <article className="home-reveal rounded-[1.8rem] border border-slate-700/80 bg-slate-950 p-10 text-slate-100 shadow-[0_18px_60px_rgba(0,0,0,0.2)]" style={{ animationDelay: '300ms' }}>
          <p className="text-xs font-black uppercase tracking-[0.24em] text-sky-200/70">{t('home.lockedTitle')}</p>
          <h2 className="mt-3 font-heading text-3xl text-slate-50">{t('home.lockedDescription')}</h2>
          <div className="mt-8 grid gap-6 sm:grid-cols-2">
            {currentUser ? (
              <>
                {quickLinks.map((item, index) => (
                  <Link
                    key={item.to}
                    className="rounded-2xl border border-slate-700 bg-slate-900/80 p-6 transition duration-300 hover:-translate-y-1 hover:border-sky-300/60 hover:bg-slate-800"
                    style={{ animationDelay: `${300 + index * 100}ms` }}
                    to={item.to}
                  >
                    <p className="font-heading text-2xl text-slate-50">{item.title}</p>
                    <p className="mt-2 text-sm leading-6 text-slate-300">{item.description}</p>
                  </Link>
                ))}
              </>
            ) : (
              <>
                <Link className="rounded-2xl border border-slate-700 bg-slate-900/80 p-6 transition duration-300 hover:-translate-y-1 hover:border-sky-300/60 hover:bg-slate-800" to="/account">
                  <p className="text-xs font-black uppercase tracking-[0.24em] text-amber-200/70">{t('home.ctaLogin')}</p>
                  <p className="mt-2 font-heading text-2xl text-slate-50">{t('home.story1Title')}</p>
                </Link>
                <Link className="rounded-2xl border border-slate-700 bg-slate-900/80 p-6 transition duration-300 hover:-translate-y-1 hover:border-sky-300/60 hover:bg-slate-800" to="/account">
                  <p className="text-xs font-black uppercase tracking-[0.24em] text-amber-200/70">{t('home.ctaRegister')}</p>
                  <p className="mt-2 font-heading text-2xl text-slate-50">{t('home.story2Title')}</p>
                </Link>
              </>
            )}
          </div>
        </article>
      </div>
    </section>

      <section className="space-y-8 pt-4">
      <div className="flex items-end justify-between gap-4">
        <div>
          <p className="text-xs font-black uppercase tracking-[0.28em] text-sky-200/70">{t('home.kicker')}</p>
          <h2 className="mt-2 font-heading text-3xl text-slate-50">Kitchen in motion</h2>
        </div>
        <p className="max-w-xl text-sm leading-6 text-slate-300">Tools, pizza, pasta, and market details moving like a living showcase.</p>
      </div>

      <div className="kitchen-carousel-window rounded-[1.8rem] border border-slate-700/70 bg-slate-950/45 p-6 sm:p-7">
        <div className="kitchen-carousel-track kitchen-carousel-track-left">
          {[...carouselItems, ...carouselItems].map((item, index) => (
            <CarouselShotCard key={`${item.title}-${index}`} item={item} index={index} />
          ))}
        </div>
      </div>

      <div className="kitchen-carousel-window rounded-[1.8rem] border border-slate-700/70 bg-slate-950/35 p-6 sm:p-7">
        <div className="kitchen-carousel-track kitchen-carousel-track-right">
          {[...carouselItems.slice(2), ...carouselItems.slice(0, 2), ...carouselItems.slice(2), ...carouselItems.slice(0, 2)].map((item, index) => (
            <CarouselShotCard key={`${item.title}-alt-${index}`} item={item} index={index} alt />
          ))}
        </div>
      </div>
      </section>
    </>
  )
}

function App() {
  const location = useLocation()
  const navigate = useNavigate()
  const [currentUser, setCurrentUser] = useState<AuthUserDto | null>(null)
  const [isAuthLoading, setIsAuthLoading] = useState(true)

  useEffect(() => {
    void loadCurrentUser()
  }, [location.pathname])

  async function loadCurrentUser() {
    setIsAuthLoading(true)
    try {
      const user = await me()
      setCurrentUser(user)
    } catch {
      setCurrentUser(null)
    } finally {
      setIsAuthLoading(false)
    }
  }

  async function onHeaderLogout() {
    try {
      await logout()
    } finally {
      setCurrentUser(null)
      navigate('/', { replace: true })
    }
  }

  function renderProtectedPage(element: ReactElement) {
    if (isAuthLoading) {
      return <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm"><p className="text-slate/70">{t('account.messages.loading')}</p></section>
    }

    if (!currentUser) {
      return (
        <section className="rounded-2xl border border-amber-300/50 bg-white/85 p-6 shadow-sm">
          <h2 className="font-heading text-3xl text-slate">{t('home.lockedTitle')}</h2>
          <p className="mt-3 text-slate/75">{t('account.messages.authRequired')}</p>
          <Link className="mt-5 inline-flex rounded-xl bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90" to="/account">
            {t('account.actions.goToLogin')}
          </Link>
        </section>
      )
    }

    return element
  }

  function renderAdminPage(element: ReactElement) {
    if (isAuthLoading) {
      return <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm"><p className="text-slate/70">{t('account.messages.loading')}</p></section>
    }

    if (!currentUser) {
      return (
        <section className="rounded-2xl border border-amber-300/50 bg-white/85 p-6 shadow-sm">
          <h2 className="font-heading text-3xl text-slate">{t('home.lockedTitle')}</h2>
          <p className="mt-3 text-slate/75">{t('account.messages.authRequired')}</p>
          <Link className="mt-5 inline-flex rounded-xl bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90" to="/account">
            {t('account.actions.goToLogin')}
          </Link>
        </section>
      )
    }

    if (currentUser.role !== 'Admin') {
      return (
        <section className="rounded-2xl border border-amber-300/50 bg-white/85 p-6 shadow-sm">
          <h2 className="font-heading text-3xl text-slate">{t('account.messages.adminRequiredTitle')}</h2>
          <p className="mt-3 text-slate/75">{t('account.messages.adminRequired')}</p>
          <Link className="mt-5 inline-flex rounded-xl border border-pine/30 px-4 py-2 font-semibold text-pine hover:bg-pine/5" to="/">
            {t('navigation.home')}
          </Link>
        </section>
      )
    }

    return element
  }

  return (
    <div className="mx-auto flex min-h-screen w-full max-w-6xl flex-col px-4 py-6 sm:px-6 lg:px-8">
      <header className="rounded-2xl border border-amber-300/70 bg-white/80 px-5 py-4 shadow-sm backdrop-blur-sm">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <BrandMark className="h-11 w-11 shrink-0 drop-shadow-[0_8px_20px_rgba(92,143,224,0.28)]" />
            <div>
              <p className="font-heading text-2xl text-slate">{t('shell.title')}</p>
              <p className="text-sm text-slate/70">{t('shell.tagline')}</p>
            </div>
          </div>
          <nav className="flex flex-wrap gap-2">
            {(currentUser ? navItems : navItems.filter((item) => item.to === '/' || item.to === '/account')).map((item) => (
              <NavLink
                key={item.to}
                className={({ isActive }) =>
                  `rounded-lg px-3 py-2 text-sm font-semibold transition ${isActive ? 'bg-pine text-white' : 'bg-white text-slate hover:bg-amber-100'}`
                }
                to={item.to}
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-50/60 px-3 py-2">
            {currentUser ? (
              <>
                <p className="text-sm font-semibold text-slate">{currentUser.fullName}</p>
                <span className="rounded-full bg-pine/10 px-2 py-0.5 text-[11px] font-bold uppercase tracking-wide text-pine">{currentUser.role}</span>
                <button
                  className="rounded-lg border border-red-300 px-3 py-1 text-xs font-bold text-red-700 hover:bg-red-50"
                  onClick={() => {
                    void onHeaderLogout()
                  }}
                  type="button"
                >
                  {t('account.actions.logout')}
                </button>
              </>
            ) : (
              <Link className="rounded-lg bg-pine px-3 py-1 text-xs font-bold text-white hover:bg-pine/90" to="/account">
                {t('account.loginTitle')}
              </Link>
            )}
          </div>
        </div>
      </header>

      <main className="mt-6 flex-1">
        <Routes>
          <Route path="/" element={<HomePage currentUser={currentUser} />} />
          <Route path="/recipes" element={renderProtectedPage(<RecipeListPage canManage={currentUser?.role === 'Admin'} />)} />
          <Route path="/recipes/new" element={renderAdminPage(<RecipeFormPage />)} />
          <Route path="/recipes/:id" element={renderProtectedPage(<RecipeDetailsPage canManage={currentUser?.role === 'Admin'} />)} />
          <Route path="/recipes/:id/edit" element={renderAdminPage(<RecipeFormPage />)} />
          <Route path="/assistant" element={renderProtectedPage(<PantryAssistantPage />)} />
          <Route path="/pantry" element={renderProtectedPage(<PantryListPage canManage={currentUser?.role === 'Admin'} />)} />
          <Route path="/pantry/assistant" element={renderProtectedPage(<PantryAssistantPage />)} />
          <Route path="/pantry/new" element={renderAdminPage(<PantryFormPage />)} />
          <Route path="/pantry/:id" element={renderProtectedPage(<PantryDetailsPage canManage={currentUser?.role === 'Admin'} />)} />
          <Route path="/pantry/:id/edit" element={renderAdminPage(<PantryFormPage />)} />
          <Route path="/newsletter" element={renderProtectedPage(<NewsletterListPage canManage={currentUser?.role === 'Admin'} />)} />
          <Route path="/newsletter/new" element={renderAdminPage(<NewsletterFormPage />)} />
          <Route path="/newsletter/:id" element={renderProtectedPage(<NewsletterDetailsPage canManage={currentUser?.role === 'Admin'} />)} />
          <Route path="/newsletter/:id/edit" element={renderAdminPage(<NewsletterFormPage />)} />
          <Route path="/account" element={<AccountPage onAuthChanged={setCurrentUser} />} />
        </Routes>
      </main>

      <AppFooter />

      {currentUser && (
        <Link
          className="ai-launcher-button fixed bottom-5 right-5 z-40 inline-flex items-center gap-2 rounded-full border-2 border-amber-100 bg-pine px-6 py-3.5 text-base font-black uppercase tracking-wide text-white shadow-[0_12px_30px_rgba(22,101,52,0.45)] ring-4 ring-pine/20 transition hover:-translate-y-0.5 hover:scale-[1.02] hover:bg-pine/90"
          to="/assistant"
        >
          {t('pantryAssistant.launcher')}
        </Link>
      )}
    </div>
  )
}

export default App
