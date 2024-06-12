/* eslint-disable no-restricted-syntax */
/* eslint-disable no-await-in-loop */
import { test, expect } from '@playwright/test';
import seoRedirects from '@frontend-root/redirects';

const typedRedirects = seoRedirects as Record<string, string>;

const PRODUCTION_PUBLIC_URL =
  'https://explore-education-statistics.service.gov.uk';
// Toggle this to true to test new redirects added in a local branch
const RUN_CONTAINS_LOCAL_REDIRECT_CHANGES = false;

test.describe('Production Redirects', () => {
  test.describe.configure({ retries: 2 });

  for (const redirect of Object.keys(typedRedirects)) {
    test(
      `Redirects on Prod or changed locally - ${redirect}`,
      { tag: ['@prod', '@local'] },
      async ({ page }) => {
        const response = await page.goto(`${PRODUCTION_PUBLIC_URL}${redirect}`);
        const statusCode = response.status();

        if (statusCode === 404) {
          // When running in the pipeline against prod, no redirects should 404
          // New redirects added to a branch should 404 UNTIL said redirect is merged and deployed
          expect(RUN_CONTAINS_LOCAL_REDIRECT_CHANGES).toBe(true);
        }
        if (statusCode === 200) {
          // If the redirect is already on prod, we should be redirected to its target
          // If we got a 200 response without a redirect, then this is a working page
          // It must be removed from redirects.js to prevent users being redirected away from genuine content
          const url = page.url();
          expect(url).toBe(`${PRODUCTION_PUBLIC_URL}${seoRedirects[redirect]}`);
          expect(url).not.toBe(`${PRODUCTION_PUBLIC_URL}${redirect}`);
        }
      },
    );
  }
});
