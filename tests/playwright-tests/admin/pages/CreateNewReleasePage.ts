import { Locator, Page } from '@playwright/test';

export default class CreateNewReleasePage {
  readonly page: Page;
  readonly academicType: Locator;
  readonly academicYear: Locator;
  readonly releaseType: Locator;
  readonly createNewReleaseButton: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.academicType = page.locator('//label[contains(text(),"Type")]');
    this.academicYear = page.locator(
      'input[name="timePeriodCoverageStartYear"]',
    );
    this.releaseType = page.locator(
      'input[id="releaseSummaryForm-releaseType-AccreditedOfficialStatistics"]',
    );
    this.createNewReleaseButton = page.locator(
      '//button[contains(text(),"Create new release")]',
    );
  }

  async clickCreateNewRelease(academicType: string, academicYear: string) {
    await this.academicType.selectOption(academicType);
    await this.academicYear.fill(academicYear);
    await this.releaseType.click();
    await this.createNewReleaseButton.click();
  }
}
