/* eslint-disable import/prefer-default-export */
/* eslint-disable lines-between-class-members */
import { Locator, Page } from '@playwright/test';

// Admin homepage
export class AdminPage {
  readonly page: Page;
  readonly signInButton: Locator;
  manageThemesTopicLink: Locator;

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
