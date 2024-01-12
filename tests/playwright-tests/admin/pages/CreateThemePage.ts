import { Locator, Page } from '@playwright/test';

export default class CreateThemePage {
  readonly page: Page;
  readonly themeTitle: Locator;
  readonly themeSummary: Locator;
  readonly saveThemeButton: Locator;

  constructor(page) {
    this.page = page;
    // Locators
    this.themeTitle = page.locator('input[id="themeForm-title"]');
    this.themeSummary= page.locator('input[id="themeForm-summary"]');
    this.saveThemeButton = page.locator('//button[text()="Save theme"]')
  }

  async doCreateTheme(title : string, summary: string) {
    await this.themeTitle.fill(title);
    await this.themeSummary.fill(summary);
    await this.saveThemeButton.click();
  }
}
