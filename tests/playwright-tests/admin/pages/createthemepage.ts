/* eslint-disable lines-between-class-members */
import { Locator, Page } from '@playwright/test';

// Create themes Page
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

  async doCreateTheme(value1 : string, value2: string) {
    await this.themeTitle.fill(value1);
    await this.themeSummary.fill(value2);
    await this.saveThemeButton.click();
  }
}
