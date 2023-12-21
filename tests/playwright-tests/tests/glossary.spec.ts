import { test, expect } from '@playwright/test';
import { HomePage } from '../general-public/pages/Basepage';
import { GlossaryPage } from '../general-public/pages/glossary-page';

test.describe('Verify the end to end functionality of glossary page', () => {
  test.beforeEach(async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.navigateToGlossaryPage();
  });

  test('Verify that accordion sections in glossary page are visible and voluntary repayment section is searchable via search ', async ({ page }) => {
    const glossarypage = new GlossaryPage(page);

    const url = await page.url();
    await expect(url).toContain('glossary');

    await expect(glossarypage.accordionSectionA).toBeVisible();
    await expect(glossarypage.accordionSectionB).toBeVisible();
    await expect(glossarypage.accordionSectionC).toBeVisible();
    await expect(glossarypage.accordionSectionD).toBeVisible();
    await expect(glossarypage.accordionSectionE).toBeVisible();
    await expect(glossarypage.accordionSectionZ).toBeVisible();

    await glossarypage.pageSearchBox.fill('Voluntary repayment');
    await expect(glossarypage.pageSearchResults).toBeTruthy();

    await glossarypage.pageSearchSelection.click();
    
    await expect(glossarypage.voluntaryRepaymentSection).toBeVisible();
    await expect(glossarypage.voluntaryRepaymentSectionText).toContainText(glossarypage.voluntary_repayment_section_paragraph_text);
  });
});
