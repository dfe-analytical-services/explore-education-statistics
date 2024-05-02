import { test, expect } from '@playwright/test';

import environment from '@util/env';

const { PUBLIC_URL } = environment;

/*
const httpCredentials1 = environment;

const httpCredentials = {
  username: httpCredentials1.PUBLIC_USERNAME,
  password: httpCredentials1.PUBLIC_PASSWORD,
};

*/

test('Example test', async ({ page }) => {
  // Access HTTP credentials from the locally defined object
  /*
  const { username, password } = httpCredentials;

  const { PUBLIC_URL } = environment;

  // Create a new browser context with HTTP authentication
  const context = await browser.newContext({
    httpCredentials: { username, password },
  });

  // Create a new page within the authenticated context
  const page = await context.newPage();
  */

  // Navigate to a URL that requires authentication
  await page.goto(PUBLIC_URL);

  // Now you're authenticated and can continue with your test logic

  await page.waitForTimeout(30000);
});
