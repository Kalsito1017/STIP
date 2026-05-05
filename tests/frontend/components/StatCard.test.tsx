import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Bus } from 'lucide-react';
import { StatCard } from '../StatCard';

describe('StatCard', () => {
  it('renders title and value', () => {
    render(<StatCard title="Active Vehicles" value="42" icon={Bus} />);
    expect(screen.getByText('Active Vehicles')).toBeInTheDocument();
    expect(screen.getByText('42')).toBeInTheDocument();
  });

  it('renders subtitle when provided', () => {
    render(<StatCard title="Vehicles" value={10} subtitle="Currently tracked" icon={Bus} />);
    expect(screen.getByText('Currently tracked')).toBeInTheDocument();
  });

  it('does not render subtitle when not provided', () => {
    render(<StatCard title="Vehicles" value={10} icon={Bus} />);
    // The component doesn't render the subtitle <p> when subtitle is undefined
    expect(screen.queryByText('Currently tracked')).not.toBeInTheDocument();
  });

  it('renders trend arrow for up', () => {
    render(<StatCard title="Vehicles" value="42" icon={Bus} trend="up" />);
    // Up triangle: ▲ (U+25B2)
    const trendElement = screen.getByText('\u25B2');
    expect(trendElement).toBeInTheDocument();
    expect(trendElement).toHaveClass('text-green-500');
  });

  it('renders trend arrow for down', () => {
    render(<StatCard title="Vehicles" value="42" icon={Bus} trend="down" />);
    // Down triangle: ▼ (U+25BC)
    const trendElement = screen.getByText('\u25BC');
    expect(trendElement).toBeInTheDocument();
    expect(trendElement).toHaveClass('text-red-500');
  });

  it('renders without trend when trend prop not provided', () => {
    render(<StatCard title="Vehicles" value={42} icon={Bus} />);
    expect(screen.queryByText('\u25B2')).not.toBeInTheDocument();
    expect(screen.queryByText('\u25BC')).not.toBeInTheDocument();
  });
});
