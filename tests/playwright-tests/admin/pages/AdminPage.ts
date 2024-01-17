import { Locator, Page } from '@playwright/test';

export default class AdminPage {
  readonly page: Page;
  readonly signInButton: Locator;
  readonly manageThemesTopicLink: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.signInButton = page.locator('button[id="signin-button"]');
    this.manageThemesTopicLink = page.locator(
      '//a[text()="manage themes and topics" ]',
    );
  }

  async clickSignIn() {
    this.signInButton.click();
  }
}