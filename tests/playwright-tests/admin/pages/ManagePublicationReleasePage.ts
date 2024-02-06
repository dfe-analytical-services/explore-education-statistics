import { Locator, Page } from '@playwright/test';

export default class ManagePublicationReleasePage {
  readonly page: Page;
  readonly createNewRelease: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.createNewRelease = page.locator(
      '//a[contains(text(),"Create new release")]',
    );
  }

  async clickCreateNewReleaseLink() {
    await this.createNewRelease.click();
  }
}
