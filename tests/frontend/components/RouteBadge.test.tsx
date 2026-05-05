import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { I18nextProvider } from 'react-i18next';
import i18n from '@/i18n';
import { RouteBadge } from '@/components/RouteBadge';

function renderWithI18n(ui: React.ReactElement) {
  return render(
    <I18nextProvider i18n={i18n}>
      {ui}
    </I18nextProvider>,
  );
}

describe('RouteBadge', () => {
  it('renders "Unknown" when type is null', () => {
    renderWithI18n(<RouteBadge type={null} />);
    expect(screen.getByText('Unknown')).toBeInTheDocument();
  });

  it('renders "Unknown" when type is undefined', () => {
    renderWithI18n(<RouteBadge type={undefined} />);
    expect(screen.getByText('Unknown')).toBeInTheDocument();
  });

  it('renders Tram for type 0', () => {
    renderWithI18n(<RouteBadge type={0} />);
    expect(screen.getByText('Tram')).toBeInTheDocument();
  });

  it('renders Metro for type 1', () => {
    renderWithI18n(<RouteBadge type={1} />);
    expect(screen.getByText('Metro')).toBeInTheDocument();
  });

  it('renders Bus for type 3', () => {
    renderWithI18n(<RouteBadge type={3} />);
    expect(screen.getByText('Bus')).toBeInTheDocument();
  });

  it('renders Trolley for type 11', () => {
    renderWithI18n(<RouteBadge type={11} />);
    expect(screen.getByText('Trolley')).toBeInTheDocument();
  });

  it('applies correct color class for Tram (type 0)', () => {
    renderWithI18n(<RouteBadge type={0} />);
    const badge = screen.getByText('Tram').closest('div');
    expect(badge).toHaveClass('bg-amber-100');
    expect(badge).toHaveClass('text-amber-800');
  });

  it('applies correct color class for Metro (type 1)', () => {
    renderWithI18n(<RouteBadge type={1} />);
    const badge = screen.getByText('Metro').closest('div');
    expect(badge).toHaveClass('bg-blue-100');
    expect(badge).toHaveClass('text-blue-800');
  });

  it('applies correct color class for Bus (type 3)', () => {
    renderWithI18n(<RouteBadge type={3} />);
    const badge = screen.getByText('Bus').closest('div');
    expect(badge).toHaveClass('bg-green-100');
    expect(badge).toHaveClass('text-green-800');
  });

  it('applies correct color class for Trolley (type 11)', () => {
    renderWithI18n(<RouteBadge type={11} />);
    const badge = screen.getByText('Trolley').closest('div');
    expect(badge).toHaveClass('bg-purple-100');
    expect(badge).toHaveClass('text-purple-800');
  });

  it('falls back to Type label for unknown type number', () => {
    renderWithI18n(<RouteBadge type={99} />);
    expect(screen.getByText('Type 99')).toBeInTheDocument();
  });
});
