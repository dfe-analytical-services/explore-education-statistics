import { Locator, Page } from '@playwright/test';

export default class AdminPage {
  readonly page: Page;
  readonly signInButton: Locator;
  readonly manageThemesTopicLink: Locator;
  readonly createNewPublicationButton: Locator;
  readonly testThemeDropdown: Locator;
  readonly testTopicDropdown: Locator;
  readonly publicationLink: (text: string) => Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.signInButton = page.locator('button[id="signin-button"]');
    this.manageThemesTopicLink = page.locator(
      '//a[text()="manage themes and topics" ]',
    );
    this.createNewPublicationButton = page.locator(
      '//a[contains(text(),"Create new publication")]',
    );
    this.testThemeDropdown = page.locator(
      'select[id="publicationsReleases-themeTopic-themeId"]',
    );
    this.testTopicDropdown = page.locator(
      'select[id="publicationsReleases-themeTopic-topicId"]',
    );
    this.publicationLink = (text: string) =>
      page.locator(`//a[text()="${text}"]`);
  }

  async clickSignIn() {
    this.signInButton.click();
  }

  async selectTestThemeAndTestTopic() {
    await this.testThemeDropdown.selectOption({ label: 'Test theme' });
    // await this.testTopicDropdown.selectOption({ label: 'Test topic' });
   // await this.createNewPublicationButton.click();
    await this.page.waitForTimeout(200);
  }

  async clickPublication(publicationName: string) {
    await this.publicationLink(publicationName).click();
  }
}
