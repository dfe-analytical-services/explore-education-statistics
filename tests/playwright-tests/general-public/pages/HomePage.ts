import { Locator, Page } from '@playwright/test';
import environment from '../../utils/env';

export default class HomePage {
  readonly page: Page;
  readonly glossary: Locator;
  readonly exploreFindStatistics: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.glossary = page.locator('//h3/a[text()="Glossary"]');
    this.exploreFindStatistics = page.locator(
      'a[data-testid="home--find-statistics-link"]',
    );
  }

  async navigateToGlossaryPage() {
    await this.page.goto(environment.PUBLIC_URL);
    await this.glossary.click();
    await this.page.waitForURL('**/glossary');
  }

  async navigateToExploreFindStatisticsPage() {
    await this.page.goto(environment.PUBLIC_URL);
    await this.exploreFindStatistics.click();
    await this.page.waitForURL('**/find-statistics');
  }
}
