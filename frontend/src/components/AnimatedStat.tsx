import { useState, useEffect, useRef } from 'react';
import { useCountUp } from '../hooks/useCountUp';
import { getLocale } from '../lib/utils';

const COUNT_UP_DURATION_MS = 1500;
const COUNT_UP_STAGGER_MS = 200;
const INTERSECTION_THRESHOLD = 0.3;

export function AnimatedStat({
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

  const count = useCountUp(value, COUNT_UP_DURATION_MS, visible);

  return (
    <div ref={ref} className="text-center">
      <div className="text-3xl sm:text-4xl font-bold text-blue-600">
        {count.toLocaleString(getLocale())}{suffix}
      </div>
      <div className="text-sm text-slate-500 mt-1">{label}</div>
    </div>
  );
}

export { COUNT_UP_STAGGER_MS };
