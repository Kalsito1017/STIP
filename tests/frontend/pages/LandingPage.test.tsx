import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import i18n from '@/i18n';
import { LandingPage } from '@/pages/LandingPage';

function renderWithProviders(ui: React.ReactElement) {
  return render(
    <MemoryRouter>
      <I18nextProvider i18n={i18n}>
        {ui}
      </I18nextProvider>
    </MemoryRouter>,
  );
}

describe('LandingPage', () => {
  it('renders hero section with title text', () => {
    renderWithProviders(<LandingPage />);

    expect(screen.getByText('See what happens')).toBeInTheDocument();
    expect(screen.getByText('before it happens.')).toBeInTheDocument();
  });

  it('renders hero subtitle', () => {
    renderWithProviders(<LandingPage />);

    expect(
      screen.getByText(
        'Real-time transport intelligence for Sofia. Live tracking, delay prediction, and reliability scoring — powered by machine learning.',
      ),
    ).toBeInTheDocument();
  });

  it('renders "Get Started" link to /login', () => {
    renderWithProviders(<LandingPage />);

    const links = screen.getAllByRole('link', { name: /get started/i });
    expect(links.length).toBeGreaterThanOrEqual(2);

    for (const link of links) {
      expect(link.getAttribute('href')).toBe('/login');
    }
  });

  it('renders "How it works" section', () => {
    renderWithProviders(<LandingPage />);

    const howItWorksLink = screen.getByRole('link', { name: /how it works/i });
    expect(howItWorksLink).toBeInTheDocument();

    // The section header text
    expect(screen.getByText('How It Works')).toBeInTheDocument();
  });

  it('renders stats bar', () => {
    renderWithProviders(<LandingPage />);

    // Stats labels should be visible
    expect(screen.getAllByText('Routes')[0]).toBeInTheDocument();
    expect(screen.getByText('Stops')).toBeInTheDocument();
    expect(screen.getByText('Daily positions')).toBeInTheDocument();
    expect(screen.getByText('Accuracy')).toBeInTheDocument();
  });

  it('renders challenge section', () => {
    renderWithProviders(<LandingPage />);

    expect(screen.getByText('The Challenge')).toBeInTheDocument();
    expect(
      screen.getByText('Sofia moves 24/7. Not everything runs on time.'),
    ).toBeInTheDocument();
  });

  it('renders pipeline steps', () => {
    renderWithProviders(<LandingPage />);

    expect(screen.getByText('Capture')).toBeInTheDocument();
    expect(screen.getByText('Analyze')).toBeInTheDocument();
    expect(screen.getByText('Predict')).toBeInTheDocument();
  });

  it('renders footer call-to-action', () => {
    renderWithProviders(<LandingPage />);

    expect(screen.getByText('Ready to see Sofia')).toBeInTheDocument();
    expect(screen.getByText('differently?')).toBeInTheDocument();

    const createAccountLink = screen.getByRole('link', { name: /create an account/i });
    expect(createAccountLink).toBeInTheDocument();
    expect(createAccountLink.getAttribute('href')).toBe('/register');
  });

  it('renders STIP logo', () => {
    renderWithProviders(<LandingPage />);

    const logo = screen.getByAltText('STIP Logo');
    expect(logo).toBeInTheDocument();
  });

  it('renders powered by section', () => {
    renderWithProviders(<LandingPage />);

    expect(screen.getByText('Powered by')).toBeInTheDocument();
    expect(screen.getByText(/PostgreSQL \+ PostGIS/)).toBeInTheDocument();
    expect(screen.getByText(/XGBoost Machine Learning/)).toBeInTheDocument();
    expect(screen.getByText(/SignalR Real-Time Push/)).toBeInTheDocument();
  });
});
