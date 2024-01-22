import { test } from '@playwright/test';
import environment from '../utils/env';
import AzureLoginPage from '../admin/azpage/AzureLoginPage';
import AdminPage from '../admin/pages/AdminPage';
import ThemesPage from '../admin/pages/ThemesPage';
import CreateThemePage from '../admin/pages/CreateThemePage';
import uiTestString from '../utils/uiTestString';

test.describe('Verify the end to end functionality of themes and topics', () => {
  let adminPage: AdminPage;
  let themesPage: ThemesPage;
  let createThemePage: CreateThemePage;
  let azPage: AzureLoginPage;

  test.beforeEach(async ({ page }) => {
    await page.goto(environment.ADMIN_URL);
    adminPage = new AdminPage(page);
    themesPage = new ThemesPage(page);
    createThemePage = new CreateThemePage(page);
    azPage = new AzureLoginPage(page);

    await adminPage.clickSignIn();
    await azPage.doSignIn();
  });

  test('Verify that themes are being created and displayed in the themes home screen', async () => {
    await adminPage.manageThemesTopicLink.click();
    await themesPage.createThemeLink.click();

    const uiThemeTitle = uiTestString('Test theme');
    const uiThemeSummary = uiTestString('Test summary');

    await createThemePage.doCreateTheme(uiThemeTitle, uiThemeSummary);
    await themesPage.checkThemeIsDisplayed(uiThemeTitle);
  });
});
