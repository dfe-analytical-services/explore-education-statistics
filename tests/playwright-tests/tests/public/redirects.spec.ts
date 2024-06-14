/* eslint-disable no-restricted-syntax */
/* eslint-disable no-await-in-loop */
import { test, expect } from '@playwright/test';
import environment from '@util/env';

import seoRedirects from '@frontend-root/redirects';

const typedRedirects = seoRedirects as Record<string, string>;

const { PUBLIC_URL } = environment;

test.describe('Redirect behaviour', () => {
  test('Absolute paths with trailing slashes are redirected without them', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}/data-catalogue/`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/data-catalogue`);

    await page.goto(`${PUBLIC_URL}/data-catalogue`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/data-catalogue`);

    await page.goto(`${PUBLIC_URL}/glossary/?someRandomUrlParameter=123`);
    await expect(page).toHaveURL(
      `${PUBLIC_URL}/glossary?someRandomUrlParameter=123`,
    );

    // Would be amazing if we could assert that these redirects
    // were done with a 301 rather than a 308...
  });

  test('Redirects do not affect browser history', async ({ page }) => {
    await page.goto(`about:blank`);

    await page.goto(`${PUBLIC_URL}/data-catalogue/`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/data-catalogue`);

    await page.goBack();
    await expect(page).toHaveURL(`about:blank`);
  });

  test('Routes without an absolute path still permit trailing slashes', async ({
    page,
  }) => {
    await page.goto(`${PUBLIC_URL}`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/`);

    await page.goto(`${PUBLIC_URL}/`);
    await expect(page).toHaveURL(`${PUBLIC_URL}/`);
  });

  // Not ideal, I'd rather it.each like Jest has. But from the docs:
  // https://playwright.dev/docs/test-parameterize
  for (const redirect of Object.keys(typedRedirects)) {
    test.skip(`Redirects from ${redirect}`, async ({ page }) => {
      await page.goto(`${PUBLIC_URL}${redirect}`);
      await expect(page).toHaveURL(`${PUBLIC_URL}${seoRedirects[redirect]}`);
    });
  }
});
