import { Link } from 'react-router-dom';
import {
  MapPin,
  BrainCircuit,
  BarChart3,
  AlertTriangle,
  Activity,
  TrendingUp,
  Bus,
  ArrowRight,
} from 'lucide-react';
import stipLogo from '../assets/StipLogo.jpg';
import { AnimatedStat, COUNT_UP_STAGGER_MS } from '../components/AnimatedStat';

const stats = [
  { label: 'Routes Tracked', value: 160, suffix: '+' },
  { label: 'Stops Monitored', value: 3500, suffix: '+' },
  { label: 'Daily Vehicle Positions', value: 500000, suffix: '+' },
  { label: 'Real-Time Accuracy', value: 99, suffix: '%' },
];

const features = [
  {
    icon: MapPin,
    title: 'Real-Time Vehicle Tracking',
    description: 'Watch buses, trams, trolleys, and metro move across Sofia on a live map. Filter by route or vehicle type for instant clarity.',
  },
  {
    icon: BrainCircuit,
    title: 'ML-Powered Delay Prediction',
    description: 'XGBoost models trained on historical GTFS data predict arrival delays before they happen — by route, stop, and time of day.',
  },
  {
    icon: BarChart3,
    title: 'Reliability Score System',
    description: 'A custom metric ranks every route so you know which lines run on time and which ones need attention.',
  },
  {
    icon: AlertTriangle,
    title: 'Trip Updates & Service Alerts',
    description: 'Real-time push notifications for disruptions, reroutes, and station closures via GTFS-RT alerts feed.',
  },
  {
    icon: Activity,
    title: 'Stop Analytics & Congestion',
    description: 'See which stops are busiest at which hours, identify transfer hubs, and drill into per-stop arrival patterns.',
  },
  {
    icon: TrendingUp,
    title: 'Delay Intelligence Dashboard',
    description: 'Heatmaps, peak-hour breakdowns, and route-level drill-downs expose exactly where delays hurt the system.',
  },
];

const techStack = [
  '.NET 10 ASP.NET Core',
  'React 19 + TypeScript',
  'PostgreSQL + PostGIS',
  'XGBoost ML',
  'SignalR Real-Time',
  'Redis Caching',
  'Docker Compose',
];

export function LandingPage() {
  return (
    <div className="bg-slate-50">
      <main>
        {/* Hero */}
        <section className="relative min-h-screen flex flex-col items-center justify-center px-4 py-20 text-center">
          <div className="absolute inset-0 bg-gradient-to-b from-blue-50 to-slate-50" />
          <div className="relative z-10 max-w-3xl mx-auto">
            <img
              src={stipLogo}
              alt="STIP Logo"
              width={200}
              height={200}
              className="h-20 sm:h-24 w-auto mx-auto mb-8 rounded-lg shadow-md"
            />
            <h1 className="text-3xl sm:text-5xl font-bold text-slate-900 leading-tight">
              Sofia Transport<br className="sm:hidden" /> Intelligence Platform
            </h1>
            <p className="mt-4 sm:mt-6 text-base sm:text-lg text-slate-600 max-w-xl mx-auto">
              Real-time tracking, delay prediction, and reliability scoring for
              Sofia&rsquo;s public transport — all in one place.
            </p>
            <div className="mt-8 sm:mt-10 flex flex-col sm:flex-row items-center justify-center gap-3">
              <Link
                to="/login"
                className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium text-sm transition-colors shadow-sm"
              >
                Get Started
                <ArrowRight className="w-4 h-4" />
              </Link>
              <a
                href="#features"
                className="inline-flex items-center gap-2 px-6 py-3 text-slate-600 hover:text-slate-900 font-medium text-sm transition-colors"
              >
                Learn More
              </a>
            </div>
          </div>

          <a
            href="#stats"
            aria-label="Scroll to statistics"
            className="absolute bottom-6 motion-safe:animate-bounce text-slate-400"
          >
            <Bus className="w-6 h-6" aria-hidden="true" />
          </a>
        </section>

        {/* Stats */}
        <section id="stats" className="py-16 sm:py-20 px-4">
          <h2 className="sr-only">Key Statistics</h2>
          <div className="max-w-4xl mx-auto">
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-6 sm:gap-10">
              {stats.map((stat, i) => (
                <AnimatedStat
                  key={stat.label}
                  label={stat.label}
                  value={stat.value}
                  suffix={stat.suffix}
                  delay={i * COUNT_UP_STAGGER_MS}
                />
              ))}
            </div>
          </div>
        </section>

        {/* Features */}
        <section id="features" className="py-16 sm:py-20 px-4 bg-white">
          <div className="max-w-5xl mx-auto">
            <div className="text-center mb-12">
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">
                Everything you need to understand Sofia&rsquo;s transport
              </h2>
              <p className="mt-3 text-slate-500 max-w-xl mx-auto">
                From live vehicle positions on a map to ML-powered delay
                predictions — STIP covers the full picture.
              </p>
            </div>
            <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
              {features.map((f) => (
                <div
                  key={f.title}
                  className="group bg-white border border-slate-200 rounded-xl p-5 sm:p-6 shadow-sm hover:shadow-md hover:border-blue-200 transition-all"
                >
                  <div className="w-10 h-10 flex items-center justify-center rounded-lg bg-blue-50 text-blue-600 mb-4 group-hover:bg-blue-100 transition-colors">
                    <f.icon className="w-5 h-5" aria-hidden="true" />
                  </div>
                  <h3 className="font-semibold text-slate-900 mb-2">{f.title}</h3>
                  <p className="text-sm text-slate-500 leading-relaxed">
                    {f.description}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Tech Stack */}
        <section className="py-12 px-4">
          <div className="max-w-4xl mx-auto text-center">
            <h2 className="text-xs font-medium text-slate-400 uppercase tracking-wider mb-4">
              Built with
            </h2>
            <div className="flex flex-wrap items-center justify-center gap-2">
              {techStack.map((tech) => (
                <span
                  key={tech}
                  className="inline-block px-3 py-1 text-xs font-medium text-slate-500 bg-slate-100 border border-slate-200 rounded-full"
                >
                  {tech}
                </span>
              ))}
            </div>
          </div>
        </section>

        {/* Footer CTA */}
        <section className="py-16 sm:py-24 px-4 bg-gradient-to-b from-white to-blue-50">
          <div className="max-w-2xl mx-auto text-center">
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">
              Ready to see Sofia&rsquo;s transport in real time?
            </h2>
            <p className="mt-3 text-slate-500">
              Join the platform that tracks every vehicle, predicts every delay,
              and scores every route.
            </p>
            <div className="mt-8 flex flex-col sm:flex-row items-center justify-center gap-3">
              <Link
                to="/login"
                className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium text-sm transition-colors shadow-sm"
              >
                Get Started
                <ArrowRight className="w-4 h-4" />
              </Link>
              <Link
                to="/register"
                className="inline-flex items-center gap-2 px-6 py-3 text-blue-600 hover:text-blue-700 font-medium text-sm transition-colors"
              >
                Create an Account
              </Link>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}
