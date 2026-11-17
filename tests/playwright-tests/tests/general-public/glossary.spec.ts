import { expect, test } from '@playwright/test';

// Port of tests/robot-tests/tests/general_public/glossary_page.robot
test.describe(
  'Glossary page',
  {
    tag: ['@general-public', '@local', '@dev', '@test', '@preprod', '@prod'],
  },
  () => {
    test('can be reached from the homepage', async ({ page }) => {
      await page.goto('/');
      await expect(
        page.getByRole('heading', {
          level: 1,
          name: 'Explore our statistics and data',
        }),
      ).toBeVisible();

      await page
        .getByRole('main')
        .getByRole('link', { name: 'Glossary', exact: true })
        .click();

      await expect(page).toHaveURL('/glossary');
      await expect(
        page.getByRole('heading', { level: 1, name: 'Glossary' }),
      ).toBeVisible();
    });

    test.describe('content', () => {
      test.beforeEach(async ({ page }) => {
        await page.goto('/glossary');
      });

      test('has an accordion section for each letter from A to Z', async ({
        page,
      }) => {
        await expect(
          page.getByTestId('accordion').getByTestId('accordionSection-heading'),
        ).toHaveText([...'ABCDEFGHIJKLMNOPQRSTUVWXYZ']);
      });

      test('has a feedback link that opens the research sign-up form in a new tab', async ({
        page,
      }) => {
        const feedbackLink = page.getByRole('link', {
          name: 'sign up for a research session using our online form.',
        });

        await expect(feedbackLink).toHaveAttribute(
          'href',
          'https://forms.office.com/e/brCmzrQfXU',
        );
        await expect(feedbackLink).toHaveAttribute('target', '_blank');
        await expect(feedbackLink).toHaveAttribute('rel', 'noreferrer');
      });

      test('selecting a search result opens and scrolls to its entry', async ({
        page,
      }) => {
        const sectionV = page.getByRole('button', {
          name: /^V, (show|hide) this section$/,
        });
        const entryHeading = page.getByRole('heading', {
          level: 3,
          name: 'Voluntary repayment',
        });

        await expect(sectionV).toHaveAttribute('aria-expanded', 'false');
        await expect(entryHeading).toBeHidden();

        await page
          .getByRole('search')
          .getByRole('combobox')
          .fill('Voluntary repayment');
        await expect(page.getByTestId('results-label')).toHaveText(
          'Found 1 result',
        );
        await page.getByRole('option', { name: 'Voluntary repayment' }).click();

        await expect(sectionV).toHaveAttribute('aria-expanded', 'true');
        await expect(entryHeading).toBeInViewport();
        await expect(
          page.getByText(
            'A borrower can at any time choose to repay some or all of their loan balance early, in addition to any repayments they are liable to make based on their income.',
          ),
        ).toBeVisible();
      });
    });
  },
);
