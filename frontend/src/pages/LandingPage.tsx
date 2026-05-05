import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import {
  MapPin,
  BrainCircuit,
  BarChart3,
  ArrowRight,
  Satellite,
  Database,
  TrendingUp,
  Clock,
  Route,
} from 'lucide-react';
import stipLogo from '../assets/StipLogo.jpg';
import { useCountUp } from '../hooks/useCountUp';
import { RouteLines } from '../components/RouteLines';
import { useTranslation } from 'react-i18next';
import { getLocale } from '../lib/utils';

const COUNT_UP_DURATION = 1500;
const COUNT_UP_STAGGER = 200;
const INTERSECTION_THRESHOLD = 0.3;

function AnimatedDarkStat({
  label,
  value,
  suffix,
  delay,
}: {
  label: string;
  value: number;
  suffix: string;
  delay: number;
}) {
  const ref = useRef<HTMLDivElement>(null);
  const [visible, setVisible] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          timerRef.current = setTimeout(() => setVisible(true), delay);
        }
      },
      { threshold: INTERSECTION_THRESHOLD }
    );
    observer.observe(el);
    return () => {
      observer.disconnect();
      clearTimeout(timerRef.current);
    };
  }, [delay]);

  const count = useCountUp(value, COUNT_UP_DURATION, visible);

  return (
    <div ref={ref} className="text-center">
      <div className="text-3xl sm:text-4xl font-bold text-blue-400">
        {count.toLocaleString(getLocale())}{suffix}
      </div>
      <div className="text-xs sm:text-sm text-slate-400 mt-1">{label}</div>
    </div>
  );
}

export function LandingPage() {
  const { t } = useTranslation('landing');

  const statsConfig = [
    { label: t('stats_routes'), value: 160, suffix: '+' },
    { label: t('stats_stops'), value: 3500, suffix: '+' },
    { label: t('stats_daily_positions'), value: 500000, suffix: '+' },
    { label: t('stats_accuracy'), value: 99, suffix: '%' },
  ];

  const problemFacts = [
    { icon: Clock, text: t('problem_peak_delays') },
    { icon: Route, text: t('problem_route_204') },
    { icon: MapPin, text: t('problem_no_visibility') },
  ];

  const pipelineSteps = [
    {
      step: '01',
      icon: Satellite,
      title: t('pipeline_capture'),
      description: t('pipeline_capture_desc'),
      accent: 'blue',
    },
    {
      step: '02',
      icon: BrainCircuit,
      title: t('pipeline_analyze'),
      description: t('pipeline_analyze_desc'),
      accent: 'cyan',
    },
    {
      step: '03',
      icon: BarChart3,
      title: t('pipeline_predict'),
      description: t('pipeline_predict_desc'),
      accent: 'emerald',
    },
  ];

  return (
    <div className="bg-slate-950 text-slate-100">
      <section className="relative min-h-screen flex flex-col items-center justify-center px-4 py-20 text-center overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-b from-slate-900 via-slate-950 to-slate-950" />
        <RouteLines />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,rgba(59,130,246,0.06)_0%,transparent_70%)]" />

        <div className="relative z-10 max-w-3xl mx-auto">
          <img
            src={stipLogo}
            alt="STIP Logo"
            width={200}
            height={200}
            className="h-16 sm:h-20 w-auto mx-auto mb-8 rounded-lg shadow-lg shadow-blue-500/10"
          />

          <h1 className="text-4xl sm:text-6xl font-bold tracking-tight">
            <span className="text-white">{t('hero_title_1')}</span>
            <br />
            <span className="bg-gradient-to-r from-blue-400 to-cyan-400 bg-clip-text text-transparent">
              {t('hero_title_2')}
            </span>
          </h1>

          <p className="mt-5 sm:mt-6 text-base sm:text-lg text-slate-400 max-w-lg mx-auto leading-relaxed">
            {t('hero_subtitle')}
          </p>

          <div className="mt-8 sm:mt-10 flex flex-col sm:flex-row items-center justify-center gap-3">
            <Link
              to="/login"
              className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-500 text-white rounded-lg font-medium text-sm transition-all shadow-lg shadow-blue-600/25 hover:shadow-blue-500/30"
            >
              {t('get_started')}
              <ArrowRight className="w-4 h-4" />
            </Link>
            <a
              href="#how-it-works"
              className="inline-flex items-center gap-2 px-6 py-3 border border-slate-700 text-slate-300 hover:text-white hover:border-slate-600 rounded-lg font-medium text-sm transition-colors"
            >
              {t('how_it_works')}
            </a>
          </div>
        </div>

        <a
          href="#challenge"
          aria-label={t('scroll_to_learn', { ns: 'map' })}
          className="absolute bottom-8 motion-safe:animate-bounce text-slate-600 hover:text-slate-400 transition-colors"
        >
          <svg width="20" height="12" viewBox="0 0 20 12" fill="none" aria-hidden="true">
            <path d="M1 1L10 10L19 1" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
        </a>
      </section>

      <section className="py-16 sm:py-20 px-4 border-t border-slate-800">
        <div className="max-w-4xl mx-auto">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-6 sm:gap-10">
            {statsConfig.map((stat, i) => (
              <AnimatedDarkStat
                key={stat.label}
                label={stat.label}
                value={stat.value}
                suffix={stat.suffix}
                delay={i * COUNT_UP_STAGGER}
              />
            ))}
          </div>
        </div>
      </section>

      <section id="challenge" className="py-16 sm:py-24 px-4 bg-slate-900/50">
        <div className="max-w-4xl mx-auto">
          <div className="text-center mb-12">
            <p className="text-xs font-medium uppercase tracking-widest text-blue-400 mb-3">
              {t('the_challenge')}
            </p>
            <h2 className="text-2xl sm:text-3xl font-bold text-white">
              {t('challenge_title')}
            </h2>
          </div>

          <div className="grid sm:grid-cols-3 gap-5">
            {problemFacts.map((fact) => (
              <div
                key={fact.text}
                className="bg-slate-800/60 border border-slate-700/50 rounded-xl p-5 sm:p-6"
              >
                <div className="w-9 h-9 flex items-center justify-center rounded-lg bg-slate-700/50 text-slate-400 mb-4">
                  <fact.icon className="w-5 h-5" />
                </div>
                <p className="text-sm text-slate-400 leading-relaxed">{fact.text}</p>
              </div>
            ))}
          </div>

          <div className="mt-10 text-center">
            <p className="text-slate-500 text-sm max-w-lg mx-auto">
              {t('challenge_footer')}
            </p>
          </div>
        </div>
      </section>

      <section id="how-it-works" className="py-16 sm:py-24 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-14">
            <p className="text-xs font-medium uppercase tracking-widest text-blue-400 mb-3">
              {t('how_it_works_section')}
            </p>
            <h2 className="text-2xl sm:text-3xl font-bold text-white">
              {t('how_title')}
            </h2>
          </div>

          <div className="grid sm:grid-cols-3 gap-6">
            {pipelineSteps.map((step) => {
              const accentColors =
                step.accent === 'blue'
                  ? { bg: 'bg-blue-500/10', border: 'border-blue-500/20', text: 'text-blue-400', icon: 'text-blue-400' }
                  : step.accent === 'cyan'
                    ? { bg: 'bg-cyan-500/10', border: 'border-cyan-500/20', text: 'text-cyan-400', icon: 'text-cyan-400' }
                    : { bg: 'bg-emerald-500/10', border: 'border-emerald-500/20', text: 'text-emerald-400', icon: 'text-emerald-400' };

              return (
                <div
                  key={step.step}
                  className={`relative ${accentColors.bg} ${accentColors.border} border rounded-xl p-5 sm:p-6 group hover:scale-[1.02] transition-transform`}
                >
                  <span className="absolute top-3 right-4 text-4xl font-bold text-slate-800/80 select-none">
                    {step.step}
                  </span>
                  <div className={`w-10 h-10 flex items-center justify-center rounded-lg ${accentColors.icon} bg-slate-800 mb-4`}>
                    <step.icon className="w-5 h-5" />
                  </div>
                  <h3 className={`font-semibold text-lg ${accentColors.text} mb-2`}>
                    {step.title}
                  </h3>
                  <p className="text-sm text-slate-400 leading-relaxed">
                    {step.description}
                  </p>
                </div>
              );
            })}
          </div>

          <div className="hidden sm:flex items-center justify-center gap-0 mt-[-24px] translate-y-6">
            {[0, 1].map((i) => (
              <div key={i} className="w-20 h-px bg-gradient-to-r from-blue-500/30 via-cyan-500/40 to-emerald-500/30" />
            ))}
          </div>
        </div>
      </section>

      <section className="py-12 px-4 border-t border-slate-800">
        <div className="max-w-4xl mx-auto text-center">
          <p className="text-xs font-medium text-slate-500 uppercase tracking-wider mb-4">
            {t('powered_by')}
          </p>
          <div className="flex flex-wrap items-center justify-center gap-x-6 gap-y-2">
            <span className="inline-flex items-center gap-2 text-sm text-slate-400">
              <Database className="w-4 h-4 text-blue-400" />
              PostgreSQL + PostGIS
            </span>
            <span className="text-slate-600 hidden sm:block">&middot;</span>
            <span className="inline-flex items-center gap-2 text-sm text-slate-400">
              <TrendingUp className="w-4 h-4 text-cyan-400" />
              XGBoost Machine Learning
            </span>
            <span className="text-slate-600 hidden sm:block">&middot;</span>
            <span className="inline-flex items-center gap-2 text-sm text-slate-400">
              <BrainCircuit className="w-4 h-4 text-emerald-400" />
              SignalR Real-Time Push
            </span>
          </div>
        </div>
      </section>

      <section className="py-20 sm:py-28 px-4 bg-gradient-to-b from-slate-950 to-slate-900">
        <div className="max-w-2xl mx-auto text-center">
          <h2 className="text-2xl sm:text-4xl font-bold text-white">
            {t('footer_title_1')}
            <br />
            <span className="bg-gradient-to-r from-blue-400 to-cyan-400 bg-clip-text text-transparent">
              {t('footer_title_2')}
            </span>
          </h2>
          <p className="mt-4 text-slate-400 max-w-md mx-auto">
            {t('footer_subtitle')}
          </p>
          <div className="mt-8 flex flex-col sm:flex-row items-center justify-center gap-3">
            <Link
              to="/login"
              className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-500 text-white rounded-lg font-medium text-sm transition-all shadow-lg shadow-blue-600/25 hover:shadow-blue-500/30"
            >
              {t('get_started')}
              <ArrowRight className="w-4 h-4" />
            </Link>
            <Link
              to="/register"
              className="inline-flex items-center gap-2 px-6 py-3 border border-slate-700 text-slate-300 hover:text-white hover:border-slate-600 rounded-lg font-medium text-sm transition-colors"
            >
              {t('create_account')}
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
