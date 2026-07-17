type BrandMarkProps = {
  className?: string
  title?: string
}

export function BrandMark({ className = '', title = 'Kitchen Recipes' }: BrandMarkProps) {
  return (
    <svg
      aria-label={title}
      className={className}
      fill="none"
      viewBox="0 0 64 64"
      xmlns="http://www.w3.org/2000/svg"
    >
      <title>{title}</title>
      <defs>
        <linearGradient id="brand-mark-bg" x1="10" x2="56" y1="8" y2="58" gradientUnits="userSpaceOnUse">
          <stop stopColor="#162033" />
          <stop offset="1" stopColor="#3d5483" />
        </linearGradient>
        <linearGradient id="brand-mark-accent" x1="18" x2="46" y1="18" y2="46" gradientUnits="userSpaceOnUse">
          <stop stopColor="#94b8ff" />
          <stop offset="1" stopColor="#f4f8ff" />
        </linearGradient>
      </defs>
      <rect x="3" y="3" width="58" height="58" rx="16" fill="url(#brand-mark-bg)" stroke="#87a7da" strokeOpacity="0.45" strokeWidth="2" />
      <circle cx="32" cy="32" r="18" stroke="url(#brand-mark-accent)" strokeWidth="2.5" opacity="0.95" />
      <path d="M21 23.5V40.5" stroke="#e7eef8" strokeLinecap="round" strokeWidth="4.6" />
      <path d="M21 32h7.2L37 23.5" stroke="#e7eef8" strokeLinecap="round" strokeLinejoin="round" strokeWidth="4.6" />
      <path d="M28.1 32L37 40.5" stroke="#e7eef8" strokeLinecap="round" strokeLinejoin="round" strokeWidth="4.6" />
      <path d="M45.2 24.8c-1.7-1.6-4-2.5-6.7-2.5-5.3 0-9.1 3.8-9.1 9.7S33.2 42 38.5 42c2.7 0 5-0.9 6.7-2.5" stroke="#9cc9ff" strokeLinecap="round" strokeLinejoin="round" strokeWidth="4.6" />
    </svg>
  )
}