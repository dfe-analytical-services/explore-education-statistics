/* eslint-disable import/prefer-default-export */
/* eslint-disable lines-between-class-members */
import { Locator, Page } from '@playwright/test';

// Glossary page
export class GlossaryPage {
  readonly page: Page;
  readonly page_search_box: Locator;
  readonly page_search_results: Locator;
  readonly page_search_selection: Locator;
  readonly accordion_section_A: Locator;
  readonly accordion_section_B: Locator;
  readonly accordion_section_C: Locator;
  readonly accordion_section_D: Locator;
  readonly accordion_section_E: Locator;
  readonly accordion_section_Z: Locator;
  readonly voluntary_repayment_section: Locator;
  readonly voluntary_repayment_section_text: Locator;
  readonly education_statistics_methodology: Locator;

  constructor(page) {
    this.page = page;
    // Locators
    this.page_search_box = page.locator('input#pageSearchForm-input');
    this.page_search_results = page.locator('div#pageSearchForm-resultsLabel');
    this.page_search_selection = page.locator(
      'li[id="pageSearchForm-option-0"]',
    );
    this.accordion_section_A = page.locator(
      '(//*[@data-testid="accordionSection-heading"])[1]',
    );
    this.accordion_section_B = page.locator(
      '(//*[@data-testid="accordionSection-heading"])[2]',
    );
    this.accordion_section_C = page.locator(
      '(//*[@data-testid="accordionSection-heading"])[3]',
    );
    this.accordion_section_D = page.locator(
      '(//*[@data-testid="accordionSection-heading"])[4]',
    );
    this.accordion_section_E = page.locator(
      '(//*[@data-testid="accordionSection-heading"])[5]',
    );
    this.accordion_section_Z = page.locator(
      '(//*[@data-testid="accordionSection-heading"])[26]',
    );
    this.voluntary_repayment_section = page.locator(
      '//h3[text()="Voluntary repayment"]',
    );
    this.voluntary_repayment_section_text = page.locator(
      '//div[@id="voluntary-repayment"]//p[1]',
    );
    this.education_statistics_methodology = page.locator(
      '//a[text()="Education statistics: methodology"]',
    );
  }

  // standard text
  readonly voluntary_repayment_section_paragraph_text =
    'A borrower can at any time choose to repay some or all of their loan balance early, in addition to any repayments they are liable to make based on their income';
}
