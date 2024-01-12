import { Locator, Page } from '@playwright/test';

export default class GlossaryPage {
  readonly page: Page;
  readonly pageSearchBox: Locator;
  readonly pageSearchResults: Locator;
  readonly pageSearchSelection: Locator;
  readonly accordionSectionA: Locator;
  readonly accordionSectionB: Locator;
  readonly accordionSectionC: Locator;
  readonly accordionSectionD: Locator;
  readonly accordionSectionE: Locator;
  readonly accordionSectionZ: Locator;
  readonly voluntaryRepaymentSection: Locator;
  readonly voluntaryRepaymentSectionText: Locator;
  readonly educationStatisticsMethodology: Locator;

  constructor(page) {
    this.page = page;
    // Locators
    this.pageSearchBox = page.locator('input#pageSearchForm-input');
    this.pageSearchResults = page.locator('div#pageSearchForm-resultsLabel');
    this.pageSearchSelection = page.locator('li[id="pageSearchForm-option-0"]');
    this.accordionSectionA = page.locator('//span[text()="A"]');
    this.accordionSectionB = page.locator('//span[text()="B"]');
    this.accordionSectionC = page.locator('//span[text()="C"]');
    this.accordionSectionD = page.locator('//span[text()="D"]');
    this.accordionSectionE = page.locator('//span[text()="E"]');
    this.accordionSectionZ = page.locator('//span[text()="Z"]');
    this.voluntaryRepaymentSection = page.locator(
      '//h3[text()="Voluntary repayment"]',
    );
    this.voluntaryRepaymentSectionText = page.locator(
      '//div[@id="voluntary-repayment"]//p[1]',
    );
    this.educationStatisticsMethodology = page.locator(
      '//a[text()="Education statistics: methodology"]',
    );
  }

  // standard text
  readonly voluntaryRepaymentSectionParagraphText =
    'A borrower can at any time choose to repay some or all of their loan balance early, in addition to any repayments they are liable to make based on their income';
}
