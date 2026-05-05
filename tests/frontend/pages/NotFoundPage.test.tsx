import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import i18n from '@/i18n';
import { NotFoundPage } from '@/pages/NotFoundPage';

function renderWithProviders(ui: React.ReactElement) {
  return render(
    <MemoryRouter>
      <I18nextProvider i18n={i18n}>
        {ui}
      </I18nextProvider>
    </MemoryRouter>,
  );
}

describe('NotFoundPage', () => {
  it('renders "Page not found" message', () => {
    renderWithProviders(<NotFoundPage />);

    expect(screen.getByText('Page not found')).toBeInTheDocument();
  });

  it('has link to /dashboard', () => {
    renderWithProviders(<NotFoundPage />);

    const link = screen.getByRole('link', { name: /go to dashboard/i });
    expect(link).toBeInTheDocument();
    expect(link.getAttribute('href')).toBe('/dashboard');
  });

  it('renders description text', () => {
    renderWithProviders(<NotFoundPage />);

    expect(
      screen.getByText("The page you're looking for doesn't exist."),
    ).toBeInTheDocument();
  });
});
