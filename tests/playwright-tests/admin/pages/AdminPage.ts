import { Locator, Page } from '@playwright/test';

export default class AdminPage {
  readonly page: Page;
  readonly signInButton: Locator;
  readonly manageThemesTopicLink: Locator;
  readonly createNewPublicationButton: Locator;
  readonly themeDropdown: Locator;
  readonly topicDropdown: Locator;
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
    this.themeDropdown = page.locator(
      'select[id="publicationsReleases-themeTopic-themeId"]',
    );
    this.topicDropdown = page.locator(
      'select[id="publicationsReleases-themeTopic-topicId"]',
    );
    this.publicationLink = (text: string) =>
      page.locator(`//a[text()="${text}"]`);
  }

  async clickSignIn() {
    this.signInButton.click();
  }

  async selectTestThemeAndTestTopic() {
    await this.themeDropdown.selectOption({ label: 'Test theme' });
  }

  async clickPublication(publicationName: string) {
    await this.publicationLink(publicationName).click();
  }
}
