#!/usr/bin/env node
/* eslint-disable camelcase */
/* eslint-disable no-console */
import puppeteer, { Page } from 'puppeteer';
import getChromePath from './getChromePath';

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiryDate: Date;
}

export interface AuthDetails {
  userName: string;
  authTokens: AuthTokens;
}

export type IdpOption = 'azure' | 'keycloak';

export const getAuthTokens = async (
  email: string,
  password: string,
  adminUrl: string,
  idp: IdpOption,
): Promise<{
  id_token: string;
  access_token: string;
  refresh_token: string;
  expires_at: Date;
}> => {
  console.log(`Getting authentication details for user ${email}`);

  const browser = await puppeteer.launch({
    headless: true,
    ignoreHTTPSErrors: true,
    product: 'chrome',
    executablePath: getChromePath(),
    args: ['--no-sandbox', '--disable-setuid-sandbox'],
  });

  const page = await browser.newPage();

  await page.goto(adminUrl);

  await page.waitForXPath('//*[.="Sign in"]', {
    timeout: 5000,
  });

  await page.click('button[class="govuk-button govuk-button--start"]');

  try {
    switch (idp) {
      case 'azure':
        await getAuthTokensAzure(page, email, password);
        break;
      case 'keycloak':
        await getAuthTokensKeycloak(page, email, password);
        break;
      default:
        throw new Error(`No login strategy for IDP ${idp}`);
    }

    return await page.evaluate(() => {
      for (let i = 0; i < localStorage.length; i += 1) {
        const key = localStorage.key(i) as string;
        const value = localStorage.getItem(key) as string;
        const json = JSON.parse(value);
        if (json.access_token) {
          return json;
        }
      }
      return null;
    });
  } finally {
    await browser.close();
  }
};

const getAuthTokensAzure = async (
  page: Page,
  email: string,
  password: string,
) => {
  await page.waitForXPath("//*[.='Sign in']", {
    visible: true,
    timeout: 5000,
  });

  const emailInput = await page.$x("//*[@name='loginfmt']");
  await emailInput[0].type(email);

  const btn = await page.$x("//*[@type='submit']");

  await btn[0].click();

  try {
    await page.waitForXPath("//*[.='Enter password']", {
      visible: true,
      timeout: 5000,
    });
  } catch (e) {
    const content = await page.content();
    if (content.includes("We couldn't find an account with that username")) {
      throw new Error(
        `User with email ${email} not recognized - received message 'We couldn't find an account with that username'`,
      );
    }
    if (content.includes('This username may be incorrect')) {
      throw new Error(
        `User with email ${email} not recognized - received message 'This username may be incorrect'`,
      );
    }
  }

  const pwdInput = await page.$x("//*[@name='passwd']");
  await pwdInput[0].type(password);

  const signInBtn = await page.$x("//*[@type='submit']");
  await signInBtn[0].click();

  try {
    await page.waitForXPath("//*[.='Stay signed in?']", {
      visible: true,
      timeout: 5000,
    });
  } catch (e) {
    const content = await page.content();
    if (content.includes('You cannot access this right now')) {
      throw new Error(
        `User ${email} cannot login from this location - received message 'You cannot access this right now'`,
      );
    }
    if (content.includes('Your account or password is incorrect')) {
      throw new Error(
        `Password for user ${email} incorrect - received message 'Your account or password is incorrect'`,
      );
    }
  }

  await page.click('#idBtn_Back');

  await page.waitForXPath("//*[.='Dashboard']", {
    visible: true,
    timeout: 10000,
  });
};

const getAuthTokensKeycloak = async (
  page: Page,
  email: string,
  password: string,
) => {
  await page.waitForXPath("//*[contains(text(), 'Sign in to your account')]", {
    visible: true,
    timeout: 5000,
  });

  const emailInput = await page.$x("//*[@id='username']");
  await emailInput[0].type(email);

  const pwdInput = await page.$x("//*[@name='password']");
  await pwdInput[0].type(password);

  const signInBtn = await page.$x("//*[@type='submit']");
  await signInBtn[0].click();

  await page.waitForXPath("//*[.='Dashboard']", {
    visible: true,
    timeout: 5000,
  });
};

const getAuthDetails = async (
  userName: string,
  email: string,
  password: string,
  adminUrl: string,
  idp: IdpOption,
): Promise<AuthDetails> => {
  const authTokens = await getAuthTokens(email, password, adminUrl, idp);

  return {
    userName,
    authTokens: {
      accessToken: authTokens.access_token,
      refreshToken: authTokens.refresh_token,
      expiryDate: authTokens.expires_at,
    },
  };
};

export default getAuthDetails;
