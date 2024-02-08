import { Locator, Page } from '@playwright/test';

export default class CreateNewPublicationPage {
  readonly page: Page;
  readonly publicationFormTitle: Locator;
  readonly publicationSummary: Locator;
  readonly teamName: Locator;
  readonly teamEmailAddress: Locator;
  readonly contactName: Locator;
  readonly contactTelephone: Locator;
  readonly savePublicationButton: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.publicationFormTitle = page.locator(
      'input[id="publicationForm-title"]',
    );
    this.publicationSummary = page.locator(
      'textarea[id="publicationForm-summary"]',
    );
    this.teamName = page.locator('input[id="publicationForm-teamName"]');
    this.teamEmailAddress = page.locator(
      'input[id="publicationForm-teamEmail"]',
    );
    this.contactName = page.locator('input[id="publicationForm-contactName"]');
    this.contactTelephone = page.locator(
      'input[id="publicationForm-contactTelNo"]',
    );
    this.savePublicationButton = page.locator('button[type="submit"]');
  }

  async createNewPublication(
    publicationName: string,
    publicationSummary: string,
  ) {
    await this.publicationFormTitle.fill(publicationName);
    await this.publicationSummary.fill(publicationSummary);
    await this.teamName.fill('UI test contact name');
    await this.teamEmailAddress.fill('ui_test@test.com');
    await this.contactName.fill('UI test team name');
    await this.contactTelephone.fill('0123 4567');
    await this.savePublicationButton.click();
  }
}
