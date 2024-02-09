import { Locator, Page } from '@playwright/test';

export default class ReleasePage {
  readonly page: Page;
  readonly pageTitle: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.pageTitle = page.locator('h1[data-testid="page-title"]');
  }
}
