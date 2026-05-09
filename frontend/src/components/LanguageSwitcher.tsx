import { motion } from 'motion/react';
import { Globe } from 'lucide-react';
import { useAppStore } from '../store/useAppStore';
import { type Locale, SUPPORTED_LOCALES } from '../i18n';
import { useTranslation } from 'react-i18next';

const LABELS: Record<Locale, string> = {
  en: 'EN',
  bg: 'БГ',
};

interface LanguageSwitcherProps {
  compact?: boolean;
}

export function LanguageSwitcher({ compact = false }: LanguageSwitcherProps) {
  const { t } = useTranslation('common');
  const language = useAppStore((s) => s.language);
  const setLanguage = useAppStore((s) => s.setLanguage);

  if (compact) {
    return (
      <button
        onClick={() => {
          const next = SUPPORTED_LOCALES[(SUPPORTED_LOCALES.indexOf(language) + 1) % SUPPORTED_LOCALES.length];
          setLanguage(next);
        }}
        className="flex items-center justify-center bg-card/90 backdrop-blur-md shadow-lg border-border/60 border rounded-full h-10 w-10 text-foreground hover:bg-card transition-colors"
        aria-label={language === 'en' ? t('switch_to_bulgarian') : t('switch_to_english')}
        title={language === 'en' ? t('switch_to_bulgarian') : t('switch_to_english')}
      >
        <Globe className="w-4 h-4" />
      </button>
    );
  }

  return (
    <div
      className="flex items-center bg-secondary rounded-lg p-0.5 gap-0.5"
      role="radiogroup"
      aria-label={t('language_label')}
    >
      {SUPPORTED_LOCALES.map((loc) => (
        <button
          key={loc}
          onClick={() => setLanguage(loc)}
          className={`relative flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-md text-xs font-semibold transition-colors ${
            language === loc
              ? 'text-foreground'
              : 'text-muted-foreground hover:text-foreground/70'
          }`}
          role="radio"
          aria-checked={language === loc}
          aria-label={loc === 'en' ? t('switch_to_english') : t('switch_to_bulgarian')}
        >
          {language === loc && (
            <motion.div
              layoutId="lang-pill"
              className="absolute inset-0 bg-card rounded-md shadow-sm border border-border"
              transition={{ type: 'spring', stiffness: 500, damping: 35 }}
            />
          )}
          <span className="relative z-10 flex items-center gap-1.5">
            <Globe className="w-3 h-3" />
            {LABELS[loc]}
          </span>
        </button>
      ))}
    </div>
  );
}
