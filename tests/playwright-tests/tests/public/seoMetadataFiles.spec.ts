import { test, expect } from '@playwright/test';
import environment from '@util/env';

const { PUBLIC_URL, PROD_PUBLIC_URL } = environment;

// Playwright finds two elements with getByText('...sitemal.xml') because the html span they're displayed
// in contains the xml <loc> tag, which playwright then treats as another html element.
// TODO: Either resolve this, or re-create these tests in robot if the team prefers
test.describe.skip('SEO Metadata Files', () => {
  test('An xml sitemap index file can be found at the expected route', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/sitemap.xml`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/sitemap.xml`);

    await expect(
      page.getByText('http://www.sitemaps.org/schemas/sitemap/0.9'),
    ).toHaveCount(1);

    await expect(
      page.getByText(`${PROD_PUBLIC_URL}/server-sitemap.xml`),
    ).toHaveCount(1);
  });

  test('An xml sitemap file can be found at the expected route', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/sitemap-0.xml`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/sitemap-0.xml`);

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
      page.getByText(`Sitemap: ${PROD_PUBLIC_URL}/sitemap.xml`),
    ).toHaveCount(1);

    await expect(
      page.getByText(`Sitemap: ${PROD_PUBLIC_URL}/server-sitemap.xml`),
    ).toHaveCount(1);
  });

  test('Bots are instructed not to crawl fast track data tables', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/robots.txt`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/robots.txt`);

    await expect(
      page.getByText('Disallow: /data-tables/fast-track/'),
    ).toHaveCount(1);
  });
});
