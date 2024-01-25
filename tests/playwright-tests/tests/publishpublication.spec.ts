import { test } from '@playwright/test';
import environment from '../utils/env';
import AzureLoginPage from '../admin/azpage/AzureLoginPage';
import AdminPage from '../admin/pages/AdminPage';
import CreateNewPublicationPage from '../admin/pages/CreateNewPublicationPage';
import uiTestString from '../utils/uiTestString';
import ContentPage from '../admin/pages/ContentPage';
import ManagePublicationReleasePage from '../admin/pages/ManagePublicationReleasePage';
import HomePage from '../general-public/pages/HomePage';
import FindStatisticsPage from '../general-public/pages/FindStatisticsPage';
import CreateNewReleasePage from '../admin/pages/CreateNewReleasePage';
import SingOffPage from '../admin/pages/SignOffPage';

test.describe.configure({ mode: 'serial' });

let adminPage: AdminPage;
let azPage: AzureLoginPage;
let createNewPublicationPage: CreateNewPublicationPage;
let managePublicationReleasePage: ManagePublicationReleasePage;
let contentPage: ContentPage;
let homePage: HomePage;
let createNewReleasePage: CreateNewReleasePage;
let signOffPage: SingOffPage;
let findStatisticsPage: FindStatisticsPage;
let uiTestPublicationName: string;
let uiTestPublicationSummary: string;

test.beforeEach(async ({ page }) => {
  adminPage = new AdminPage(page);
  azPage = new AzureLoginPage(page);
  createNewPublicationPage = new CreateNewPublicationPage(page);
  managePublicationReleasePage = new ManagePublicationReleasePage(page);
  contentPage = new ContentPage(page);
  homePage = new HomePage(page);
  createNewReleasePage = new CreateNewReleasePage(page);
  signOffPage = new SingOffPage(page);
  findStatisticsPage = new FindStatisticsPage(page);
});

test('Verify that user is able to create a release via admin', async ({
  page,
}) => {
  await page.goto(environment.ADMIN_URL);
  await adminPage.clickSignIn();
  await azPage.doSignIn();
  await page.waitForLoadState('load');
  adminPage.selectTestThemeAndTestTopic();
  await adminPage.createNewPublicationButton.click();

  uiTestPublicationName = uiTestString('publish publication');
  uiTestPublicationSummary = uiTestString('publication summary');

  await createNewPublicationPage.createNewPublication(
    uiTestPublicationName,
    uiTestPublicationSummary,
  );
  await adminPage.clickPublication(uiTestPublicationName);
  await managePublicationReleasePage.createNewReleaseLink();
  await createNewReleasePage.createNewRelease();
  await contentPage.fillHeadlineTextContentDetails();
  await signOffPage.publishReleaseImmediately();
  await signOffPage.waitUntilReleaseStatusToCommplete();
  await page.waitForLoadState('domcontentloaded');
});

test('Validate that user is able to see the created release in public', async () => {
  await homePage.navigateToExploreFindStatisticsPage();
  await findStatisticsPage.navigateToPublicReleasePage(uiTestPublicationName);
});
