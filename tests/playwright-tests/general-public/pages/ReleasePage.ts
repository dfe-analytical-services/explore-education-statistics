import { Locator, Page } from '@playwright/test';

export default class ReleasePage {
  readonly page: Page;
  readonly pageTitle: (text: string) => Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.pageTitle = (text: string) => page.locator(`//h1[text()="${text}"]`);
  }
}
