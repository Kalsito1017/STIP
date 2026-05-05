import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Outlet } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import i18n from '@/i18n';
import { Layout } from '@/components/Layout';
import { useAppStore } from '@/store/useAppStore';

function renderLayout() {
  return render(
    <MemoryRouter initialEntries={['/dashboard']}>
      <I18nextProvider i18n={i18n}>
        <Layout>
          <Outlet />
        </Layout>
      </I18nextProvider>
    </MemoryRouter>,
  );
}

describe('Layout', () => {
  it('renders the sidebar', () => {
    renderLayout();
    expect(screen.getAllByText('STIP')).toHaveLength(2);
  });

  it('renders navigation items', () => {
    renderLayout();
    expect(screen.getAllByText('Live Map').length).toBeGreaterThanOrEqual(2);
    expect(screen.getAllByText('Dashboard').length).toBeGreaterThanOrEqual(2);
    expect(screen.getAllByText('Routes').length).toBeGreaterThanOrEqual(2);
  });

  it('renders mobile menu button', () => {
    renderLayout();
    expect(screen.getByRole('button', { name: /open menu/i })).toBeInTheDocument();
  });
});
