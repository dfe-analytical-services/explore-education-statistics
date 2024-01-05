/* eslint-disable no-template-curly-in-string */
/* eslint-disable @typescript-eslint/no-shadow */
/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { test } from "@playwright/test";
import { environment } from "../utils/env";
import { AzureLoginPage } from "../admin/azpage/az-loginpage";
import { AdminPage } from "../admin/pages/adminpage";
import { ThemesPage } from "../admin/pages/themes-page";
import generateUIThemeName from "../utils/util";
import { CreateThemePage } from "../admin/pages/createthemepage";

test.describe('Verify the end to end functionality of themes and topics', () => {
    test.beforeEach(async ({ page }) => {
        const adminPage = new AdminPage(page);
        const azPage = new AzureLoginPage(page);

        await page.goto(environment.ADMIN_URL!);
        await adminPage.clickSignIn();
        await azPage.doSignIn();
    });

test("Verify that themes are being created and displayed in the themes home screen", async ({ page }) => {

    const adminPage = new AdminPage(page);
    const themesPage = new ThemesPage(page);
    const createThemePage = new CreateThemePage(page);
   
    await adminPage.manageThemesTopicLink.click();
    await themesPage.createThemeLink.click();

    const uiThemeName: string = generateUIThemeName();
    const uiThemeTile: string = 'Title'.concat(uiThemeName);
    const uiThemeSummary: string = 'Summary'.concat(uiThemeName);

    await createThemePage.doCreateTheme(uiThemeTile,uiThemeSummary);
    await themesPage.checkThemeIsDisplayed(uiThemeTile);  
    });

});