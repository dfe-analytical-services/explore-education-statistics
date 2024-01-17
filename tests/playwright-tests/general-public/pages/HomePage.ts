import { Locator, Page } from '@playwright/test';
import environment from '../../utils/env';

export default class HomePage {
  readonly page: Page;
  readonly glossary: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.glossary = page.locator('//h3/a[text()="Glossary"]');
  }

  async navigateToGlossaryPage() {
    await this.page.goto(environment.PUBLIC_URL);
    await this.glossary.click();
    await this.page.waitForURL('**/glossary');
  }
}
