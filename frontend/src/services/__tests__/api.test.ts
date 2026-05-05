import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import api from '../api';

// Mock sonner toast
vi.mock('sonner', () => ({
  toast: {
    error: vi.fn(),
    success: vi.fn(),
  },
}));

const server = setupServer();

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(async () => {
  server.resetHandlers();
  localStorage.clear();
  // Reset toast mock calls across tests
  const { toast } = await import('sonner');
  (toast.error as ReturnType<typeof vi.fn>).mockClear();
});
afterAll(() => server.close());

describe('api client', () => {
  describe('request interceptor', () => {
    it('adds Authorization header when token is in localStorage', async () => {
      localStorage.setItem('token', 'test-token-123');

      server.use(
        http.get('/api/test-auth', ({ request }) => {
          const authHeader = request.headers.get('Authorization');
          return HttpResponse.json({ authHeader });
        }),
      );

      const response = await api.get('/test-auth');
      expect(response.data.authHeader).toBe('Bearer test-token-123');
    });

    it('does not add Authorization header when no token in localStorage', async () => {
      server.use(
        http.get('/api/test-no-auth', ({ request }) => {
          const authHeader = request.headers.get('Authorization');
          return HttpResponse.json({ hasAuth: !!authHeader });
        }),
      );

      const response = await api.get('/test-no-auth');
      expect(response.data.hasAuth).toBe(false);
    });

    it('adds Accept-Language header from localStorage', async () => {
      localStorage.setItem('language', 'bg');

      server.use(
        http.get('/api/test-lang', ({ request }) => {
          const langHeader = request.headers.get('Accept-Language');
          return HttpResponse.json({ langHeader });
        }),
      );

      const response = await api.get('/test-lang');
      expect(response.data.langHeader).toBe('bg');
    });

    it('does not add Accept-Language header when no language in localStorage', async () => {
      server.use(
        http.get('/api/test-no-lang', ({ request }) => {
          const langHeader = request.headers.get('Accept-Language');
          return HttpResponse.json({ hasLang: !!langHeader });
        }),
      );

      const response = await api.get('/test-no-lang');
      expect(response.data.hasLang).toBe(false);
    });
  });

  describe('response interceptor - 400 errors', () => {
    it('shows toast on 400 error for non-auth routes', async () => {
      const { toast } = await import('sonner');

      server.use(
        http.get('/api/routes', () => {
          return HttpResponse.json(
            { error: 'Invalid request', details: ['Field required'] },
            { status: 400 },
          );
        }),
      );

      try {
        await api.get('/routes');
      } catch {
        // expected
      }

      expect(toast.error).toHaveBeenCalledWith('Field required');
    });

    it('shows toast with data.error when details not present', async () => {
      const { toast } = await import('sonner');

      server.use(
        http.get('/api/routes', () => {
          return HttpResponse.json(
            { error: 'Something bad happened' },
            { status: 400 },
          );
        }),
      );

      try {
        await api.get('/routes');
      } catch {
        // expected
      }

      expect(toast.error).toHaveBeenCalledWith('Something bad happened');
    });

    it('shows fallback message when no error body on 400', async () => {
      const { toast } = await import('sonner');

      server.use(
        http.get('/api/routes', () => {
          return new HttpResponse(null, { status: 400 });
        }),
      );

      try {
        await api.get('/routes');
      } catch {
        // expected
      }

      expect(toast.error).toHaveBeenCalledWith('Bad request');
    });

    it('does not show toast for auth login route on 400', async () => {
      const { toast } = await import('sonner');

      server.use(
        http.post('/api/auth/login', () => {
          return HttpResponse.json(
            { error: 'Invalid credentials' },
            { status: 400 },
          );
        }),
      );

      try {
        await api.post('/auth/login', { email: 'x', password: 'x' });
      } catch {
        // expected
      }

      expect(toast.error).not.toHaveBeenCalled();
    });

    it('does not show toast for auth register route on 400', async () => {
      const { toast } = await import('sonner');

      server.use(
        http.post('/api/auth/register', () => {
          return HttpResponse.json(
            { error: 'Email taken' },
            { status: 400 },
          );
        }),
      );

      try {
        await api.post('/auth/register', { email: 'x', password: 'x', fullName: 'X' });
      } catch {
        // expected
      }

      expect(toast.error).not.toHaveBeenCalled();
    });
  });

  describe('response interceptor - 401 errors', () => {
    it('removes token and user from localStorage on 401', async () => {
      localStorage.setItem('token', 'old-token');
      localStorage.setItem('user', JSON.stringify({ userId: 'u1' }));

      server.use(
        http.get('/api/protected', () => {
          return HttpResponse.json({ error: 'Unauthorized' }, { status: 401 });
        }),
      );

      try {
        await api.get('/protected');
      } catch {
        // expected
      }

      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('user')).toBeNull();
    });
  });

  describe('success responses', () => {
    it('passes through successful responses unchanged', async () => {
      server.use(
        http.get('/api/routes', () => {
          return HttpResponse.json([{ routeId: 'R1', name: 'Route 1' }]);
        }),
      );

      const response = await api.get('/routes');
      expect(response.status).toBe(200);
      expect(response.data).toEqual([{ routeId: 'R1', name: 'Route 1' }]);
    });
  });
});
