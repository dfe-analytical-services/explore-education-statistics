import { defineConfig, devices } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';

const envFile = path.join(import.meta.dirname, '.env');

if (fs.existsSync(envFile)) {
  // Variables already set in the environment take precedence over the file.
  process.loadEnvFile(envFile);
}

const { CI, PUBLIC_URL, PUBLIC_AUTH_USER, PUBLIC_AUTH_PASSWORD } = process.env;

if (!PUBLIC_URL) {
  throw new Error(
    'PUBLIC_URL must be set, either in the environment or in tests/playwright-tests/.env (see .env.example)',
  );
}

const isCI = !!CI;

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: isCI,
  retries: isCI ? 2 : 0,
  reporter: isCI
    ? [
        ['list'],
        ['html', { open: 'never' }],
        ['junit', { outputFile: 'test-results/junit.xml' }],
      ]
    : [['list'], ['html', { open: 'on-failure' }]],
  use: {
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'general-public',
      testDir: './tests/general-public',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: PUBLIC_URL,
        // Send credentials up front rather than waiting for a 401 challenge, as
        // Front Door can serve a cached 200 that never issues one. Scoped to
        // the public site's origin so they aren't leaked to external links.
        httpCredentials:
          PUBLIC_AUTH_USER && PUBLIC_AUTH_PASSWORD
            ? {
                username: PUBLIC_AUTH_USER,
                password: PUBLIC_AUTH_PASSWORD,
                origin: new URL(PUBLIC_URL).origin,
                send: 'always',
              }
            : undefined,
      },
    },
  ],
});
