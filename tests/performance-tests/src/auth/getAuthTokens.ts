#!/usr/bin/env node

/* eslint-disable camelcase */
import puppeteer from 'puppeteer';

interface Credentials {
  email: string;
  password: string;
  baseUrl: string
}

export interface AuthTokens {
  access_token: string;
  refresh_token: string;
}

export const getRawAuthTokenResponse = async ({
  email,
  password,
  baseUrl,
}: Credentials): Promise<AuthTokens> => {
  const browser = await puppeteer.launch({
    headless: true,
    ignoreHTTPSErrors: true,
  });

  const page = await browser.newPage();

  await page.goto(baseUrl);
  await page.waitForNavigation({
    waitUntil: 'networkidle0',
  });

  await page.click('button[class="govuk-button govuk-button--start"]');

  await page.waitForNavigation({
    timeout: 5000,
    waitUntil: 'domcontentloaded',
  });

  await page.waitForTimeout(2000);

  const emailInput = await page.$x("//*[@name='loginfmt']");
  await emailInput[0].type(email);

  const btn = await page.$x("//*[@type='submit']");
  await btn[0].click();

  await page.waitForNavigation({
    timeout: 5000,
    waitUntil: 'domcontentloaded',
  });

  await page.waitForTimeout(2000);

  const pwdInput = await page.$x("//*[@name='passwd']");
  await pwdInput[0].type(password);

  const signInBtn = await page.$x("//*[@type='submit']");
  await signInBtn[0].click();

  await page.waitForTimeout(2000);

  await page.waitForXPath("//div[text()='Stay signed in?']", {
    visible: true,
    timeout: 60000,
  });

  await page.click('#idBtn_Back');

  await page.waitForTimeout(10000);

  const rawTokenResponse = await page.evaluate(() => {
    /* eslint-disable-next-line no-plusplus */
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i) as string;
      const value = localStorage.getItem(key) as string;
      const json = JSON.parse(value);
      if (json.access_token) {
        return json;
      }
    }
    return null;
  });

  await browser.close();
  return rawTokenResponse;
};

const getAuthTokens = async (credentials: Credentials): Promise<AuthTokens> => {
  const rawTokenResponse = await getRawAuthTokenResponse(credentials);
  const { access_token, refresh_token } = rawTokenResponse;
  return { access_token, refresh_token };
};

export default getAuthTokens;
