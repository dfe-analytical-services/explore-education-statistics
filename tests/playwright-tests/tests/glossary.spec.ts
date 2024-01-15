import { test, expect } from '@playwright/test';
import HomePage from '../general-public/pages/HomePage';
import GlossaryPage from '../general-public/pages/GlossaryPage';

test.describe('Verify the end to end functionality of glossary page', () => {
  test.beforeEach(async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.navigateToGlossaryPage();
  });

  test('Verify that accordion sections in glossary page are visible and voluntary repayment section is searchable via search ', async ({
    page,
  }) => {
    const glossaryPage = new GlossaryPage(page);

    const url = await page.url();
    await expect(url).toContain('glossary');

    await expect(glossaryPage.accordionSectionA).toBeVisible();
    await expect(glossaryPage.accordionSectionB).toBeVisible();
    await expect(glossaryPage.accordionSectionC).toBeVisible();
    await expect(glossaryPage.accordionSectionD).toBeVisible();
    await expect(glossaryPage.accordionSectionE).toBeVisible();
    await expect(glossaryPage.accordionSectionZ).toBeVisible();

    await glossaryPage.pageSearchBox.fill('Voluntary repayment');
    await expect(glossaryPage.pageSearchResults).toBeTruthy();

    await glossaryPage.pageSearchSelection.click();

    await expect(glossaryPage.voluntaryRepaymentSection).toBeVisible();
    await expect(glossaryPage.voluntaryRepaymentSectionText).toContainText(
      glossaryPage.voluntaryRepaymentSectionParagraphText,
    );
  });
});
