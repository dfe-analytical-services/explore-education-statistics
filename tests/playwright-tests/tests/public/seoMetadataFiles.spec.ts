import { test, expect } from '@playwright/test';
import environment from '@util/env';

const { PUBLIC_URL } = environment;

test.describe('SEO Metadata Files', () => {
  test('An xml sitemap file can be found at the expected route', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/sitemap.xml`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/sitemap.xml`);

    await expect(
      page.getByText('http://www.sitemaps.org/schemas/sitemap/0.9'),
    ).toHaveCount(1);
  });

  test('A robots.txt file can be found at the expected route', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/robots.txt`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/robots.txt`);

    await expect(page.getByText('User-Agent: *')).toHaveCount(1);
  });

  test('The robots.txt file points to the sitemap', async ({ page }) => {
    await page.goto(`${PUBLIC_URL}/robots.txt`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/robots.txt`);

    await expect(
      page.getByText('Sitemap: http://localhost:3000/sitemap.xml'),
    ).toHaveCount(1);
  });

  test('The Googlebot is instructed not to crawl fast track data tables', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/robots.txt`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/robots.txt`);

    await expect(
      page.getByText('Disallow: /data-tables/fast-track/'),
    ).toHaveCount(1);
  });
});
