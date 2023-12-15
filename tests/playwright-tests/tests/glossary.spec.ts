import { test, expect } from '@playwright/test';
import { HomePage } from '../general-public/pages/Basepage';
import { GlossaryPage } from '../general-public/pages/glossary-page';

test.describe('Verify the end to end functionality of glossary page', () => {
  test.beforeEach(async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.navigateToGlossaryPage();
  });

  test('Verify that accordion sections in glossary page are visible and voluntary repayment section is searchable via search ', async ({
    page,
  }) => {
    const glossarypage = new GlossaryPage(page);

    const url = await page.url();
    await expect(url).toContain('glossary');

    await expect(glossarypage.accordion_section_A).toBeVisible();
    await expect(glossarypage.accordion_section_B).toBeVisible();
    await expect(glossarypage.accordion_section_C).toBeVisible();
    await expect(glossarypage.accordion_section_D).toBeVisible();
    await expect(glossarypage.accordion_section_E).toBeVisible();
    await expect(glossarypage.accordion_section_Z).toBeVisible();

    await glossarypage.page_search_box.fill('Voluntary repayment');
    await expect(glossarypage.page_search_results).toBeTruthy();

    await glossarypage.page_search_selection.click();
    await expect(glossarypage.voluntary_repayment_section).toBeVisible();
    await expect(glossarypage.voluntary_repayment_section_text).toContainText(
      glossarypage.voluntary_repayment_section_paragraph_text,
    );
  });
});
