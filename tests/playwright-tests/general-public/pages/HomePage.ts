import { Locator, Page } from '@playwright/test';
import environment from '@util/env';

export default class HomePage {
  readonly page: Page;
  readonly glossaryLink: Locator;
  readonly exploreFindStatisticsLink: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.glossaryLink = page.locator('//h3/a[text()="Glossary"]');
    this.exploreFindStatisticsLink = page.locator(
      'a[data-testid="home--find-statistics-link"]',
    );
  }

  async navigateToGlossaryPage() {
    await this.page.goto(environment.PUBLIC_URL);
    await this.glossaryLink.click();
    await this.page.waitForURL('**/glossary');
  }

  async navigateToExploreFindStatisticsPage() {
    await this.page.goto(environment.PUBLIC_URL);
    await this.exploreFindStatisticsLink.click();
    await this.page.waitForURL('**/find-statistics');
  }
}
