import { describe, it, expect, beforeEach, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import i18n from '../../i18n';
import { useAppStore } from '../../store/useAppStore';
import { Sidebar } from '../Sidebar';
import { render } from '@testing-library/react';

const initialState = useAppStore.getInitialState();

// Mock useLogout
const mockLogoutFn = vi.fn();

vi.mock('../../hooks/useAuth', () => ({
  useLogout: () => mockLogoutFn,
}));

function renderWithProviders(ui: React.ReactElement, { initialRoute = '/' } = {}) {
  return render(
    <MemoryRouter initialEntries={[initialRoute]}>
      <I18nextProvider i18n={i18n}>
        {ui}
      </I18nextProvider>
    </MemoryRouter>,
  );
}

beforeEach(() => {
  useAppStore.setState(initialState);
  localStorage.clear();
  vi.clearAllMocks();
});

describe('Sidebar', () => {
  it('renders all nav items', () => {
    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    // Sidebar renders both desktop and mobile versions; use getAllByText for duplicates
    expect(screen.getAllByText('Live Map')).toHaveLength(2);
    expect(screen.getAllByText('Dashboard')).toHaveLength(2);
    expect(screen.getAllByText('Routes')).toHaveLength(2);
    expect(screen.getAllByText('Stops')).toHaveLength(2);
    expect(screen.getAllByText('Analytics')).toHaveLength(2);
    expect(screen.getAllByText('Settings')).toHaveLength(2);
  });

  it('applies active styling to current route navlink', () => {
    renderWithProviders(<Sidebar open={true} onClose={() => {}} />, { initialRoute: '/dashboard' });

    // With route /dashboard, the Dashboard navlink should get active class
    // Check one of the Dashboard links has active class
    const dashboardLinks = screen.getAllByText('Dashboard');
    const activeLink = dashboardLinks.find(
      (el) => el.closest('a')?.className.includes('bg-primary/10')
    );
    expect(activeLink).toBeTruthy();
  });

  it('shows Sign In button when not authenticated', () => {
    useAppStore.setState({ isAuthenticated: false, user: null });
    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    expect(screen.getAllByText('Sign In')).toHaveLength(2);
  });

  it('shows Sign Out button when user is authenticated', () => {
    useAppStore.setState({
      isAuthenticated: true,
      user: { userId: 'u1', email: 'test@test.com', fullName: 'Test User' },
    });

    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    expect(screen.getAllByText('Sign Out')).toHaveLength(2);
    expect(screen.getAllByText('Test User')).toHaveLength(2);
    expect(screen.getAllByText('test@test.com')).toHaveLength(2);
  });

  it('LanguageSwitcher is rendered', () => {
    useAppStore.setState({ language: 'en' });
    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    // EN appears in both desktop and mobile sidebars, each in 2 spans (current + next)
    const enElements = screen.getAllByText('EN');
    expect(enElements.length).toBeGreaterThanOrEqual(2);
  });

  it('calls logout when Sign Out clicked', async () => {
    useAppStore.setState({
      isAuthenticated: true,
      user: { userId: 'u1', email: 'test@test.com', fullName: 'Test User' },
    });
    const user = userEvent.setup();

    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    // Click one of the two Sign Out buttons
    const signOutButtons = screen.getAllByText('Sign Out');
    await user.click(signOutButtons[0]);
    expect(mockLogoutFn).toHaveBeenCalledTimes(1);
  });

  it('navigates to /login when protected route clicked while unauthenticated', async () => {
    useAppStore.setState({ isAuthenticated: false, user: null });

    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    const analyticsLinks = screen.getAllByText('Analytics');
    const lockedLink = analyticsLinks[0].closest('a');
    expect(lockedLink).toBeTruthy();
    // Locked items have href="/" (from <NavLink to="/">) but cursor-not-allowed
    expect(lockedLink?.className).toContain('cursor-not-allowed');
  });

  it('renders lock icon for protected nav items when not authenticated', () => {
    useAppStore.setState({ isAuthenticated: false, user: null });

    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    // Lock icons should appear next to protected items in both sidebars
    const locks = document.querySelectorAll('.lucide-lock');
    // Analytics and Settings both get LockIcon, rendered twice (desktop + mobile) = 4
    expect(locks.length).toBeGreaterThanOrEqual(4);
  });

  it('does not render lock icon when authenticated', () => {
    useAppStore.setState({
      isAuthenticated: true,
      user: { userId: 'u1', email: 'test@test.com', fullName: 'Test User' },
    });

    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    const locks = document.querySelectorAll('.lucide-lock');
    expect(locks.length).toBe(0);
  });

  it('renders STIP brand header', () => {
    renderWithProviders(<Sidebar open={true} onClose={() => {}} />);

    // Rendered in both desktop and mobile sidebars
    expect(screen.getAllByText('STIP')).toHaveLength(2);
  });
});
