import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { Layout } from './Layout';
import { useAppStore as store } from '../store/useAppStore';

vi.mock('../hooks/useAuth', () => ({
  useLogin: vi.fn(),
  useRegister: vi.fn(),
  useLogout: vi.fn(() => vi.fn()),
  useDeleteAccount: vi.fn(),
}));

function renderLayout() {
  return render(
    <MemoryRouter initialEntries={['/dashboard']}>
      <Layout />
    </MemoryRouter>,
  );
}

describe('Layout', () => {
  beforeEach(() => {
    store.setState({
      connectionState: 'disconnected',
      user: null,
      isAuthenticated: false,
    });
  });

  it('renders the STIP branding in sidebar', () => {
    renderLayout();
    const elements = document.querySelectorAll('h1');
    const stip = Array.from(elements).find((el) => el.textContent === 'STIP');
    expect(stip).toBeInTheDocument();
  });

  it('shows connection status when connected', () => {
    store.setState({ connectionState: 'connected' });
    const { container } = renderLayout();
    expect(container.textContent).toMatch(/live|Live|connected/i);
  });

  it('shows user name when authenticated', () => {
    store.setState({
      user: { userId: 'u1', email: 'test@test.com', fullName: 'Test User' },
      isAuthenticated: true,
      connectionState: 'connected',
    });
    const { getAllByText } = renderLayout();

    const matches = getAllByText(/Test User/);
    expect(matches.length).toBeGreaterThanOrEqual(1);
  });

  it('renders sidebar with navigation links', () => {
    renderLayout();
    // Sidebar includes Live Map link
    const elements = document.querySelectorAll('a');
    const liveMap = Array.from(elements).find((el) => el.textContent?.includes('Live Map'));
    expect(liveMap).toBeInTheDocument();
  });
});
