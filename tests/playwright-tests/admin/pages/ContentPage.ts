import { Locator, Page } from '@playwright/test';

export default class ContentPage {
  readonly page: Page;
  readonly contentLink: Locator;
  readonly addHeadlineTextBlockButton: Locator;
  readonly addHeadlineTextInputBox: Locator;
  readonly editBlockButton: Locator;
  readonly editBlockSectionInputBox: Locator;
  readonly saveCloseButton: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.contentLink = this.page.locator('//a[contains(text(), "Content")]');
    this.addHeadlineTextBlockButton = page.locator(
      '//button[contains(text(),"Add a headlines text block")]',
    );
    this.addHeadlineTextInputBox = page.locator(
      '//p[contains(text(), "This section is empty")]',
    );
    this.editBlockButton = page
      .locator('button[type="button"]')
      .filter({ hasText: 'Edit block' });
    this.editBlockSectionInputBox = page.locator(
      '[aria-label="Editor editing area: main"]',
    );
    this.saveCloseButton = page.locator(
      '//button[contains(text(), "Save & close")]',
    );
  }

  async fillHeadlineTextContentDetails() {
    await this.contentLink.click();
    await this.addHeadlineTextBlockButton.click();
    await this.addHeadlineTextInputBox.waitFor({ state: 'visible' });
    await this.page.waitForTimeout(2000);
    await this.editBlockButton.click();
    await this.editBlockSectionInputBox.fill('content');
    await this.saveCloseButton.click();

  }
}
