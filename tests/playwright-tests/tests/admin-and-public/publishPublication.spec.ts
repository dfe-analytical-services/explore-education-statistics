import { test, expect } from '@playwright/test';
import environment from '@util/env';
import AzureLoginPage from '@admin/azpage/AzureLoginPage';
import AdminPage from '@admin/pages/AdminPage';
import CreateNewPublicationPage from '@admin/pages/CreateNewPublicationPage';
import uiTestString from '@util/uiTestString';
import ContentPage from '@admin/pages/ContentPage';
import ManagePublicationReleasePage from '@admin/pages/ManagePublicationReleasePage';
import HomePage from '@general-public/pages/HomePage';
import FindStatisticsPage from '@general-public/pages/FindStatisticsPage';
import CreateNewReleasePage from '@admin/pages/CreateNewReleasePage';
import SignOffPage from '@admin/pages/SignOffPage';
import ReleasePage from '@general-public/pages/ReleasePage';

test.describe.configure({ mode: 'serial' });

let adminPage: AdminPage;
let azPage: AzureLoginPage;
let createNewPublicationPage: CreateNewPublicationPage;
let managePublicationReleasePage: ManagePublicationReleasePage;
let contentPage: ContentPage;
let homePage: HomePage;
let createNewReleasePage: CreateNewReleasePage;
let signOffPage: SignOffPage;
let findStatisticsPage: FindStatisticsPage;
let releasePage: ReleasePage;
let publicationName: string;
let publicationSummary: string;

test.beforeEach(async ({ page }) => {
  adminPage = new AdminPage(page);
  azPage = new AzureLoginPage(page);
  createNewPublicationPage = new CreateNewPublicationPage(page);
  managePublicationReleasePage = new ManagePublicationReleasePage(page);
  contentPage = new ContentPage(page);
  homePage = new HomePage(page);
  createNewReleasePage = new CreateNewReleasePage(page);
  signOffPage = new SignOffPage(page);
  findStatisticsPage = new FindStatisticsPage(page);
  releasePage = new ReleasePage(page);
});

test('Verify that user is able to create a release via admin', async ({
  page,
}) => {
  await page.goto(environment.ADMIN_URL);
  await adminPage.clickSignIn();
  await azPage.doSignIn();
  await page.waitForLoadState('load');
  await adminPage.selectTestThemeAndTestTopic();
  await adminPage.createNewPublicationButton.click();

  publicationName = uiTestString('publish publication');
  publicationSummary = uiTestString('publication summary');

  await createNewPublicationPage.createNewPublication(
    publicationName,
    publicationSummary,
  );
  await adminPage.clickPublication(publicationName);
  await managePublicationReleasePage.clickCreateNewReleaseLink();
  await createNewReleasePage.clickCreateNewRelease('AYQ1', '2022');
  await contentPage.fillHeadlineTextContentDetails();
  await signOffPage.publishReleaseImmediately();
  await signOffPage.waitUntilReleaseStatusIsComplete();
  await page.waitForLoadState('domcontentloaded');
});

test('Validate that user is able to see the created release in public and title is same as publication name', async () => {
  await homePage.navigateToExploreFindStatisticsPage();
  await findStatisticsPage.navigateToPublicReleasePage(publicationName);
  const title = await releasePage.pageTitle.textContent();
  await expect(title).toContain(publicationName);
});
