/* eslint-disable import/prefer-default-export */
/* eslint-disable lines-between-class-members */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { Locator, Page } from '@playwright/test';

// Themes home page
export class ThemesPage {
  readonly page: Page;
  readonly createThemeLink: Locator;
  readonly themeTitle:(text: string) => Locator;
   

  constructor(page) {
    this.page = page;
    // Locators
    this.createThemeLink = page.locator('//a[text()="Create theme"]');
    this.themeTitle = (text: string) => page.locator(`//span[text()="${text}"]`);
  }

  async checkThemeIsDisplayed(text: string){
    const themeTile = this.themeTitle(text);
    await themeTile.isVisible();
  }

}
