/* eslint-disable import/prefer-default-export */
/* eslint-disable lines-between-class-members */
import { Locator, Page } from '@playwright/test';
import { environment } from '../../utils/env';

// Home page
export class AzureLoginPage {
  readonly page: Page;
    emailAddress: Locator;
    nextButton: Locator;
    password: Locator;
    signInButton: Locator;
    noButton: Locator;

  constructor(page) {
    this.page = page;
    // Locators
    this.emailAddress = page.locator('input[type="email"]');
    this.nextButton =  page.getByRole('button', { name: 'Next' });
    this.password =  page.getByPlaceholder('Password');
    this.signInButton = page.getByRole('button', { name: 'Sign in' })
    this.noButton = page.getByRole('button', { name: 'No' });
  }


}
