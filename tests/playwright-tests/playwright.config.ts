import { defineConfig, devices } from '@playwright/test';
import environment from '@util/env';

const { PUBLIC_USERNAME, PUBLIC_PASSWORD, PUBLIC_URL } = environment;

// eslint-disable-next-line @typescript-eslint/no-var-requires
function appendBasicAuthCredentialsToPublic(pubUrl: string) {
  if (typeof pubUrl !== 'string') {
    return null;
  }
  if (pubUrl.includes(PUBLIC_URL)) {
    const username = PUBLIC_USERNAME;
    const password = PUBLIC_PASSWORD;

    if (username && password) {
      return { username, password };
    }
  }
  return null;
}

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  // testDir: './tests',

  timeout: 15 * 60 * 1000,
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: process.env.CI
    ? [
        ['html', { outputFolder: 'test-results/playwright-testreport' }],
        ['github'],
        ['list'],
        ['junit', { outputFile: 'test-results/playwright-results.xml' }],
      ]
    : [['html']],
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL: ' ',
    ignoreHTTPSErrors: true,
    httpCredentials: appendBasicAuthCredentialsToPublic(PUBLIC_URL),
    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on',
    video: 'retain-on-failure',
  },

  /* Configure different test suites and running against chrome browser */
  projects: [
    {
      name: 'public',
      testDir: './tests/public',
    },
    {
      name: 'admin',
      testDir: './tests/admin',
    },
    {
      name: 'adminandpublic',
      testDir: './tests/admin-and-public',
    },
    {
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1280, height: 720 },
      },
    },

    /* {
    //   name: 'firefox',
    //   use: { ...devices['Desktop Firefox'] },
    //  },

    // {
    //   name: 'webkit',
    //   use: { ...devices['Desktop Safari'] },
    // }, */

    /* Test against mobile viewports. */
    // {
    //   name: 'Mobile Chrome',
    //   use: { ...devices['Pixel 5'] },
    // },
    // {
    //   name: 'Mobile Safari',
    //   use: { ...devices['iPhone 12'] },
    // },

    /* Test against branded browsers. */
    // {
    //   name: 'Microsoft Edge',
    //   use: { ...devices['Desktop Edge'], channel: 'msedge' },
    // },
    // {
    //   name: 'Google Chrome',
    //   use: { ...devices['Desktop Chrome'], channel: 'chrome' },
    // },
  ],

  /* Run your local dev server before starting the tests */
  // webServer: {
  //   command: 'npm run start',
  //   url: 'http://127.0.0.1:3000',
  //   reuseExistingServer: !process.env.CI,
  // },
});
