/* eslint-disable no-restricted-syntax */
/* eslint-disable no-await-in-loop */
import { test, expect } from '@playwright/test';
import environment from '@util/env';

// TODO: Maybe install frontend as a dependency so we can import this properly?
// eslint-disable-next-line import/no-relative-packages
import seoRedirects from '../../../../src/explore-education-statistics-frontend/redirects.js';

const { PUBLIC_URL, PUBLIC_URL_WITHOUT_USERNAME_PASSWORD } = environment;

test.describe('Redirect behaviour', () => {
  test('Absolute paths with trailing slashes are redirected without them', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/data-catalogue/`);
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/data-catalogue`,
    );

    await page.goto(`${PUBLIC_URL}/data-catalogue`);
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/data-catalogue`,
    );

    await page.goto(`${PUBLIC_URL}/glossary/?someRandomUrlParameter=123`);
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/glossary?someRandomUrlParameter=123`,
    );

    // Would be amazing if we could assert that these redirects
    // were done with a 301 rather than a 308...
  });

  test('Redirects do not affect browser history', async ({ page }) => {
    await page.goto(`about:blank`);

    await page.goto(`${PUBLIC_URL}/data-catalogue/`);
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/data-catalogue`,
    );

    await page.goBack();
    await expect(page).toHaveURL(`about:blank`);
  });

  test('Routes without an absolute path still permit trailing slashes', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}`);
    await expect(page).toHaveURL(`${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/`);

    await page.goto(`${PUBLIC_URL}/`);
    await expect(page).toHaveURL(`${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/`);
  });

  test('meta-guidance is redirected to data-guidance', async ({ page }) => {
    await page.goto(
      `${PUBLIC_URL}/find-statistics/seed-publication-release-approver/meta-guidance`,
    );
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/find-statistics/seed-publication-release-approver/data-guidance`,
    );

    await page.goto(
      `${PUBLIC_URL}/find-statistics/seed-publication-release-approver/meta-guidance/`,
    );
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/find-statistics/seed-publication-release-approver/data-guidance`,
    );
  });

  test('download-latest-data is redirected to data-catalogue', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/download-latest-data`);
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/data-catalogue`,
    );

    await page.goto(`${PUBLIC_URL}/download-latest-data/`);
    await expect(page).toHaveURL(
      `${PUBLIC_URL_WITHOUT_USERNAME_PASSWORD}/data-catalogue`,
    );
  });

  // Not ideal, I'd rather it.each like Jest has. But from the docs:
  // https://playwright.dev/docs/test-parameterize
  for (const redirect of seoRedirects) {
    test.skip(`Redirects from ${redirect.from}`, async ({ page }) => {
      await page.goto(`${PUBLIC_URL}${redirect.from}`);
      await expect(page).toHaveURL(`${PUBLIC_URL}${redirect.to}`);
    });
  }
});
