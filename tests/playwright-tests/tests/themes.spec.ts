/* eslint-disable no-template-curly-in-string */
/* eslint-disable @typescript-eslint/no-shadow */
/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { test, expect } from "@playwright/test";
import { environment } from "../utils/env";

test("Click and hold", async ({ page }) => {


    // Go to https://letcode.in/
    await page.goto(environment.ADMIN_URL!);
    await page.locator('button[id="signin-button"]').click();

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    await page.locator('input[type="email"]').fill(environment.ADMIN_EMAILADDR!);
    await page.getByRole('button', { name: 'Next' }).click();
    await page.getByPlaceholder('Password').fill(environment.ADMIN_PASSWORD!);
    await page.getByRole('button', { name: 'Sign in' }).click();
    await page.getByRole('button', { name: 'No' }).click();
    await page.waitForLoadState("domcontentloaded");
    await page.waitForNavigation();
    await page.getByRole('link', { name: 'manage themes and topics' }).click();
    await page.getByRole('link', { name: 'Create theme' }).click();
    const date = new Date();
    const utcFormat = date.toISOString().replace("T"," ").substring(0, 19);
    const uithemename = 'UI Test'.concat(utcFormat);
    console.log(uithemename)
  
    await page.locator('input[id="themeForm-title"]').fill(uithemename);
    await page.locator('input[id="themeForm-summary"]').fill(uithemename);

    await page.getByRole('button', { name: 'Save theme' }).click();
    await page.waitForTimeout(200);

    await page.locator(`//span[text()="${uithemename}"]`).isVisible();

    const datatestid = 'Create topic link for '.concat(uithemename);
    console.log(datatestid);


})