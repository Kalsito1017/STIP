import { useAppStore } from '../store/useAppStore';
import { type Locale, SUPPORTED_LOCALES } from '../i18n';

const LABELS: Record<Locale, string> = {
  en: 'EN',
  bg: 'БГ',
};

export function LanguageSwitcher() {
  const language = useAppStore((s) => s.language);
  const setLanguage = useAppStore((s) => s.setLanguage);

  const next = SUPPORTED_LOCALES[(SUPPORTED_LOCALES.indexOf(language) + 1) % SUPPORTED_LOCALES.length];

  return (
    <button
      onClick={() => setLanguage(next)}
      className="flex items-center gap-2 w-full px-3 py-1.5 rounded-md text-sm font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition-colors"
      aria-label={`Switch to ${next === 'en' ? 'English' : 'Bulgarian'}`}
    >
      <span className="font-mono text-xs">{LABELS[language]}</span>
      <span className="text-slate-400 text-xs">→ {LABELS[next]}</span>
    </button>
  );
}
