import en from '../locales/en.json'

type Dictionary = typeof en

const dictionary: Dictionary = en

export function t(path: string): string {
  const value = path.split('.').reduce<unknown>((current, key) => {
    if (current && typeof current === 'object' && key in current) {
      return (current as Record<string, unknown>)[key]
    }

    return undefined
  }, dictionary)

  if (typeof value === 'string') {
    return value
  }

  return path
}
