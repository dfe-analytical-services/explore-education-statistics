/* eslint-disable import/prefer-default-export */
/* eslint-disable lines-between-class-members */
import { Locator, Page } from '@playwright/test';
import { environment } from '../../utils/env';

// Glossary page
export class HomePage {
  readonly page: Page;
  readonly glossary: Locator;

  constructor(page) {
    this.page = page;
    this.glossary = page.locator('//h3/a[text()="Glossary"]');
  }

  async navigateToGlossaryPage() {
    await this.page.goto(environment.BASE_URL);
    await this.glossary.click();
    await this.page.waitForURL('**/glossary');
  }
}
