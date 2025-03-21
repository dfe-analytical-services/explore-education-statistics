import { Locator, Page } from '@playwright/test';
import environment from '../../utils/env';

export default class AzureLoginPage {
  readonly page: Page;
  readonly emailAddress: Locator;
  readonly nextButton: Locator;
  readonly password: Locator;
  readonly signInButton: Locator;
  readonly noButton: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.emailAddress = page.locator(
      'input[placeholder="Email, phone, or Skype"]',
    );
    this.nextButton = page.locator('input[type="submit"]');
    this.password = page.locator('input[type="password"]');
    this.signInButton = page.locator('input[id="idSIButton9"]');
    this.noButton = page.locator('input[id="idBtn_Back"]');
  }

  async doSignIn() {
    await this.emailAddress.fill(environment.ADMIN_EMAIL);
    await this.nextButton.click();
    await this.password.waitFor({ state: 'visible' });
    await this.password.fill(environment.ADMIN_PASSWORD);
    await this.signInButton.click();
    await this.noButton.waitFor({ state: 'visible' });
    await this.noButton.click();
  }
}
