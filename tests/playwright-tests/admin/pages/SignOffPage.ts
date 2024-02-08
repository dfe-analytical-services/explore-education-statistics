import { Locator, Page } from '@playwright/test';

export default class SignOffPage {
  readonly page: Page;
  readonly signOffLink: Locator;
  readonly editReleaseStatusButton: Locator;
  readonly approvalStatus: Locator;
  readonly internalReleasenoteInputBox: Locator;
  readonly whenToPublishRadioButton: Locator;
  readonly nextReleaseExpectedYear: Locator;
  readonly nextReleaseExpectedMonth: Locator;
  readonly updateStatusButton: Locator;
  readonly releaseProcessStatus: Locator;

  constructor(page: Page) {
    this.page = page;
    // Locators
    this.signOffLink = page.locator('//a[contains(text(),"Sign off")]');
    this.editReleaseStatusButton = page.locator(
      '//button[contains(text(),"Edit release status")]',
    );
    this.approvalStatus = page.locator(
      'input[id="releaseStatusForm-approvalStatus-Approved"]',
    );
    this.internalReleasenoteInputBox = page.locator(
      'textarea[id="releaseStatusForm-internalReleaseNote"]',
    );
    this.whenToPublishRadioButton = page.locator(
      'input[id="releaseStatusForm-publishMethod-Immediate"]',
    );
    this.nextReleaseExpectedMonth = page.locator(
      'input[id="releaseStatusForm-nextReleaseDate-month"]',
    );
    this.nextReleaseExpectedYear = page.locator(
      'input[id="releaseStatusForm-nextReleaseDate-year"]',
    );
    this.updateStatusButton = page.locator(
      '//button[contains(text(),"Update status")]',
    );
    this.releaseProcessStatus = page.locator(
      'strong[id="release-process-status-Complete"]',
    );
  }

  async publishReleaseImmediately() {
    await this.signOffLink.click();
    await this.editReleaseStatusButton.click();
    await this.approvalStatus.click();
    await this.internalReleasenoteInputBox.fill('Internal Note');
    await this.whenToPublishRadioButton.click();
    await this.nextReleaseExpectedMonth.fill('12');
    await this.nextReleaseExpectedYear.fill('2040');
    await this.updateStatusButton.click();
  }

  async waitUntilReleaseStatusIsComplete() {
    const releaseStatus = await this.releaseProcessStatus.innerText();
    await this.page.waitForFunction(async expectedtext => {
      return expectedtext.toLowerCase() === 'complete';
    }, releaseStatus);
  }
}
