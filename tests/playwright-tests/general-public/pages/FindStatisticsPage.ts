import { Locator, Page } from '@playwright/test';
import slugFromTitle from '@util/slugFromTitle';

export default class FindStatisticsPage {
  readonly page: Page;
  readonly releaseLink: (text: string) => Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.releaseLink = (text: string) => page.locator(`//a[text()="${text}"]`);
  }

  async navigateToPublicReleasePage(publicationName: string) {
    await this.releaseLink(publicationName).click({ force: true });
    await this.page.waitForURL(
      `**/find-statistics/${slugFromTitle(publicationName)}`,
    );
  }
}
