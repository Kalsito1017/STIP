const ANIMATION_STYLES = `
  @keyframes drawLine {
    to { stroke-dashoffset: 0; }
  }
  @keyframes flowLine {
    to { stroke-dashoffset: -80; }
  }
  @keyframes pulseNode {
    0%, 100% { opacity: 0.15; }
    50% { opacity: 0.6; }
  }
  @keyframes pulseCenter {
    0%, 100% { opacity: 0.3; r: 4; }
    50% { opacity: 0.9; r: 7; }
  }
  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }
  @keyframes shimmerLine {
    0% { stroke-opacity: 0.3; }
    50% { stroke-opacity: 0.7; }
    100% { stroke-opacity: 0.3; }
  }

  .route-line {
    fill: none;
    stroke-linecap: round;
    stroke-linejoin: round;
  }

  .route-draw {
    stroke-dasharray: 2500;
    stroke-dashoffset: 2500;
    animation: drawLine 3s ease-out forwards;
  }

  .route-flow {
    stroke-dasharray: 12 28;
    stroke-dashoffset: 0;
    animation: flowLine 3s linear infinite;
  }

  .route-shimmer {
    animation: shimmerLine 4s ease-in-out infinite;
  }

  .delay-300 { animation-delay: 0.3s; }
  .delay-500 { animation-delay: 0.5s; }
  .delay-700 { animation-delay: 0.7s; }
  .delay-900 { animation-delay: 0.9s; }
  .delay-1100 { animation-delay: 1.1s; }
  .delay-1400 { animation-delay: 1.4s; }
  .delay-1700 { animation-delay: 1.7s; }
  .delay-2000 { animation-delay: 2s; }
  .delay-2400 { animation-delay: 2.4s; }

  .flow-delay-0 { animation-delay: 0s; }
  .flow-delay-500 { animation-delay: 0.5s; }
  .flow-delay-1000 { animation-delay: 1s; }
  .flow-delay-1500 { animation-delay: 1.5s; }
  .flow-delay-2000 { animation-delay: 2s; }

  .node-pulse {
    animation: pulseNode 3s ease-in-out infinite;
  }
  .node-pulse-delay { animation-delay: 1.5s; }

  .center-pulse {
    animation: pulseCenter 2.5s ease-in-out infinite;
  }
`;

export function RouteLines() {
  return (
    <div className="absolute inset-0 overflow-hidden pointer-events-none select-none" aria-hidden="true">
      <style>{ANIMATION_STYLES}</style>
      <svg
        viewBox="0 0 1000 700"
        preserveAspectRatio="xMidYMid slice"
        className="w-full h-full"
      >
        {/* Radial lines from center */}
        <line x1="500" y1="350" x2="120" y2="80" className="route-line route-draw" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />
        <line x1="500" y1="350" x2="500" y2="40" className="route-line route-draw delay-300" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />
        <line x1="500" y1="350" x2="880" y2="80" className="route-line route-draw delay-500" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />
        <line x1="500" y1="350" x2="960" y2="350" className="route-line route-draw delay-700" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />
        <line x1="500" y1="350" x2="880" y2="620" className="route-line route-draw delay-900" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />
        <line x1="500" y1="350" x2="500" y2="660" className="route-line route-draw delay-1100" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />
        <line x1="500" y1="350" x2="120" y2="620" className="route-line route-draw delay-700" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />
        <line x1="500" y1="350" x2="40" y2="350" className="route-line route-draw delay-900" stroke="#3b82f6" strokeWidth="1" opacity="0.5" />

        {/* Ring roads */}
        <circle cx="500" cy="350" r="100" className="route-line route-draw delay-1400" stroke="#60a5fa" strokeWidth="1" opacity="0.45" />
        <circle cx="500" cy="350" r="220" className="route-line route-draw delay-1700" stroke="#60a5fa" strokeWidth="1" opacity="0.35" />
        <circle cx="500" cy="350" r="360" className="route-line route-draw delay-2000" stroke="#60a5fa" strokeWidth="0.8" opacity="0.25" />

        {/* Grid connectors */}
        <line x1="180" y1="130" x2="820" y2="130" className="route-line route-draw delay-500" stroke="#3b82f6" strokeWidth="0.8" opacity="0.3" />
        <line x1="180" y1="230" x2="650" y2="230" className="route-line route-draw delay-700" stroke="#3b82f6" strokeWidth="0.8" opacity="0.35" />
        <line x1="350" y1="230" x2="820" y2="230" className="route-line route-draw delay-900" stroke="#3b82f6" strokeWidth="0.8" opacity="0.3" />
        <line x1="180" y1="470" x2="820" y2="470" className="route-line route-draw delay-500" stroke="#3b82f6" strokeWidth="0.8" opacity="0.3" />
        <line x1="180" y1="570" x2="820" y2="570" className="route-line route-draw delay-700" stroke="#3b82f6" strokeWidth="0.8" opacity="0.35" />
        <line x1="230" y1="80" x2="230" y2="620" className="route-line route-draw delay-900" stroke="#3b82f6" strokeWidth="0.8" opacity="0.25" />
        <line x1="400" y1="80" x2="400" y2="620" className="route-line route-draw delay-1100" stroke="#3b82f6" strokeWidth="0.8" opacity="0.3" />
        <line x1="600" y1="80" x2="600" y2="620" className="route-line route-draw delay-700" stroke="#3b82f6" strokeWidth="0.8" opacity="0.3" />
        <line x1="770" y1="80" x2="770" y2="620" className="route-line route-draw delay-900" stroke="#3b82f6" strokeWidth="0.8" opacity="0.25" />

        {/* Diagonal connectors */}
        <line x1="220" y1="160" x2="370" y2="310" className="route-line route-draw delay-1100" stroke="#06b6d4" strokeWidth="1" opacity="0.35" />
        <line x1="630" y1="310" x2="780" y2="460" className="route-line route-draw delay-1400" stroke="#06b6d4" strokeWidth="1" opacity="0.35" />
        <line x1="220" y1="540" x2="370" y2="390" className="route-line route-draw delay-1100" stroke="#06b6d4" strokeWidth="1" opacity="0.35" />
        <line x1="630" y1="390" x2="780" y2="240" className="route-line route-draw delay-1400" stroke="#06b6d4" strokeWidth="1" opacity="0.35" />

        {/* Flowing data lines */}
        <line x1="500" y1="350" x2="880" y2="80" className="route-line route-flow flow-delay-0" stroke="#22d3ee" strokeWidth="1.5" opacity="0.5" />
        <line x1="500" y1="350" x2="120" y2="620" className="route-line route-flow flow-delay-1000" stroke="#22d3ee" strokeWidth="1.5" opacity="0.5" />
        <line x1="500" y1="350" x2="40" y2="350" className="route-line route-flow flow-delay-2000" stroke="#22d3ee" strokeWidth="1.5" opacity="0.4" />
        <circle cx="500" cy="350" r="220" className="route-line route-flow flow-delay-1500" stroke="#22d3ee" strokeWidth="1.2" opacity="0.35" />

        {/* Shimmer lines */}
        <line x1="180" y1="470" x2="820" y2="470" className="route-line route-shimmer" stroke="#3b82f6" strokeWidth="0.6" opacity="0" />
        <line x1="180" y1="130" x2="820" y2="130" className="route-line route-shimmer" stroke="#3b82f6" strokeWidth="0.6" opacity="0" style={{ animationDelay: '2s' }} />

        {/* Nodes */}
        <circle cx="500" cy="350" r="4" className="center-pulse" fill="#22d3ee" />
        <circle cx="500" cy="350" r="1.5" fill="#06b6d4" />

        {[
          [120, 80], [500, 40], [880, 80], [960, 350],
          [880, 620], [500, 660], [120, 620], [40, 350],
          [230, 130], [400, 130], [600, 130], [770, 130],
          [230, 230], [350, 230], [650, 230],
          [230, 470], [400, 470], [600, 470], [770, 470],
          [230, 570], [400, 570], [600, 570], [770, 570],
          [230, 350], [600, 350], [370, 310], [630, 310],
          [370, 390], [630, 390],
        ].map(([cx, cy], i) => (
          <circle
            key={i}
            cx={cx}
            cy={cy}
            r={1.8}
            className={i % 2 === 0 ? 'node-pulse' : 'node-pulse node-pulse-delay'}
            fill="#60a5fa"
            opacity="0"
          />
        ))}

        {/* Route-type indicator dots (colored) */}
        {[[200, 130], [500, 80], [800, 230], [300, 470], [600, 570], [200, 570]].map(([cx, cy], i) => (
          <circle
            key={`rt-${i}`}
            cx={cx}
            cy={cy}
            r={2.5}
            className={i < 3 ? 'node-pulse' : 'node-pulse node-pulse-delay'}
            fill={i % 3 === 0 ? '#22d3ee' : i % 3 === 1 ? '#f59e0b' : '#a78bfa'}
            opacity="0"
          />
        ))}
      </svg>
    </div>
  );
}
