import { Locator, Page } from '@playwright/test';

// Admin homepage
export default class AdminPage {
  readonly page: Page;
  readonly signInButton: Locator;
  readonly manageThemesTopicLink: Locator;

  constructor(page) {
    this.page = page;
    // Locators
    this.signInButton = page.locator('button[id="signin-button"]');
    this.manageThemesTopicLink = page.locator('//a[text()="manage themes and topics" ]');
  }

  async clickSignIn() {
    this.signInButton.click();
  }
}
