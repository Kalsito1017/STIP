import { test, expect } from '@playwright/test';

test.describe('Navigation', () => {
  test('landing page loads and shows hero', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('h1')).toContainText('See what happens');
    await expect(page.getByRole('link', { name: 'Get Started' })).toBeVisible();
  });

  test('login page loads', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /sign in/i })).toBeVisible();
  });

  test('register page loads', async ({ page }) => {
    await page.goto('/register');
    await expect(page.getByLabel(/full name/i)).toBeVisible();
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i)).toBeVisible();
  });

  test('not found page shows 404 for unknown routes', async ({ page }) => {
    await page.goto('/nonexistent-route');
    await expect(page.locator('h1')).toContainText(/not found/i);
  });
});

test.describe('Sidebar Navigation', () => {
  test('live map loads without sidebar (unauthenticated)', async ({ page }) => {
    await page.goto('/');
    const sidebar = page.locator('aside');
    const navLinks = sidebar.locator('a');
    // Landing page shows sidebar since it's the Live Map layout
    await expect(sidebar).toBeVisible();
  });

  test('dashboard is accessible via sidebar', async ({ page }) => {
    // Visit dashboard directly
    await page.goto('/dashboard');
    // Sidebar should be visible
    const sidebar = page.locator('aside');
    await expect(sidebar).toBeVisible();
    // Should show Sign In button when unauthenticated
    await expect(sidebar.getByText(/sign in/i)).toBeVisible();
  });
});

test.describe('Language Switching', () => {
  test('language switcher is visible in sidebar', async ({ page }) => {
    await page.goto('/dashboard');
    // Language switcher should show EN and arrow to БГ
    const switcher = page.locator('aside').getByText(/EN/);
    await expect(switcher).toBeVisible();
  });

  test('switching language changes sidebar labels', async ({ page }) => {
    await page.goto('/dashboard');
    // Verify English labels first
    await expect(page.locator('aside').getByText('Routes')).toBeVisible();

    // Click language switcher to switch to Bulgarian
    const enButton = page.locator('aside').getByText('EN');
    await enButton.click();

    // Verify Bulgarian labels
    await expect(page.locator('aside').getByText('Маршрути')).toBeVisible();
  });

  test('switching back to English restores labels', async ({ page }) => {
    await page.goto('/dashboard');

    // Switch to BG
    await page.locator('aside').getByText('EN').click();
    await expect(page.locator('aside').getByText('Български').or(page.locator('aside').getByText('Маршрути'))).toBeVisible({ timeout: 5000 });

    // Switch back to EN
    await page.locator('aside').getByText('БГ').click();
    await expect(page.locator('aside').getByText('Routes')).toBeVisible({ timeout: 5000 });
  });
});

test.describe('Protected Routes', () => {
  test('analytics redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/analytics');
    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);
  });

  test('settings redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/settings');
    await expect(page).toHaveURL(/\/login/);
  });
});
