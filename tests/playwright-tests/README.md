# Playwright UI tests

End-to-end UI tests written with [Playwright](https://playwright.dev/). These are
gradually replacing the Robot Framework tests in `tests/robot-tests`.

## Setup

From the repository root:

```bash
pnpm install
pnpm --filter explore-education-statistics-playwright-tests install:browsers
cp tests/playwright-tests/.env.example tests/playwright-tests/.env
```

Then edit `.env` to point at the environment under test. Variables set in the shell
take precedence over `.env`.

## Running tests

From `tests/playwright-tests`:

```bash
pnpm test:e2e                     # run everything
pnpm test:e2e glossary            # run spec files matching "glossary"
pnpm test:e2e --grep @prod        # run tests tagged for an environment
pnpm test:e2e --ui                # interactive UI mode
pnpm test:e2e --debug             # step through with the inspector
pnpm report                       # open the last HTML report
```

## Conventions

- Tests live under `tests/<project>/`, where each project maps to a site (e.g.
  `general-public`) and gets its own `baseURL` in `playwright.config.ts`.
- Tag tests with the environments they're safe to run against (`@local`, `@dev`,
  `@test`, `@preprod`, `@prod`), matching the Robot `Force Tags`.
- Keep tests independent: each test navigates to where it needs to be, rather than
  relying on state left by an earlier test.
- Prefer user-facing locators (`getByRole`, `getByLabel`, `getByText`) and fall back
  to `getByTestId` only where there's no accessible alternative. Avoid XPath and
  CSS class selectors.
- Use web-first assertions (`await expect(locator).toBeVisible()`) instead of
  explicit waits.
