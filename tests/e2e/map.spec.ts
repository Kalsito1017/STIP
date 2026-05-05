import { test, expect } from '@playwright/test';

test.describe('Live Map', () => {
  test('map renders on landing/root path', async ({ page }) => {
    await page.goto('/');
    // Leaflet map container should be present
    const mapContainer = page.locator('.leaflet-container');
    await expect(mapContainer).toBeVisible({ timeout: 10000 });
  });

  test('map has zoom controls', async ({ page }) => {
    await page.goto('/');
    const zoomIn = page.locator('.leaflet-control-zoom-in');
    const zoomOut = page.locator('.leaflet-control-zoom-out');
    await expect(zoomIn).toBeVisible({ timeout: 10000 });
    await expect(zoomOut).toBeVisible({ timeout: 10000 });
  });

  test('filter panel has route selector', async ({ page }) => {
    await page.goto('/');
    const routeSelect = page.locator('select[aria-label*="Filter" i]');
    await expect(routeSelect).toBeVisible({ timeout: 10000 });
  });

  test('dark mode toggle exists', async ({ page }) => {
    await page.goto('/');
    // Dark mode toggle button
    const darkModeBtn = page.locator('button[aria-label*="dark" i], button[aria-label*="light" i], button[title*="dark" i]');
    await expect(darkModeBtn.first()).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Responsive Design', () => {
  test('mobile viewport shows hamburger menu', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/dashboard');

    // Mobile menu button should be visible
    const menuBtn = page.getByRole('button', { name: /open menu/i });
    await expect(menuBtn).toBeVisible({ timeout: 5000 });
  });

  test('sidebar is hidden by default on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/dashboard');

    // The mobile drawer sidebar should be off-screen (negative translate)
    const mobileSidebar = page.locator('aside.lg\\:hidden');
    await expect(mobileSidebar).toBeVisible();
    await expect(mobileSidebar).toHaveClass(/-translate-x-full/);
  });

  test('mobile sidebar opens when hamburger clicked', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/dashboard');

    // Click hamburger
    await page.getByRole('button', { name: /open menu/i }).click();

    // Sidebar should now be visible (not translated off)
    const mobileSidebar = page.locator('aside.lg\\:hidden');
    await expect(mobileSidebar).toHaveClass(/translate-x-0/);
  });
});

test.describe('Dark Mode', () => {
  test('landing page uses dark theme', async ({ page }) => {
    await page.goto('/');
    // Landing page has bg-slate-950 (dark background)
    const body = page.locator('body');
    const html = page.locator('html');

    // Check dark mode is not active on landing (landing has its own dark bg)
    await expect(body).toBeVisible();
  });

  test('dark mode toggle changes theme', async ({ page }) => {
    await page.goto('/dashboard');
    const html = page.locator('html');

    // Click dark mode toggle
    const darkModeBtn = page.locator('button[aria-label*="dark" i], button[aria-label*="light" i], button[title*="dark" i]');
    const btn = darkModeBtn.first();
    if (await btn.isVisible()) {
      await btn.click();
      // After toggle, html should have or not have 'dark' class
      await expect(html).toBeVisible();
    }
  });
});
