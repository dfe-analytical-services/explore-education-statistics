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

const getAuthTokens = async (
  userName: string,
  email: string,
  password: string,
  adminUrl: string,
  idp: IdpOption,
): Promise<AuthDetails> => {
  console.log(`Getting authentication details for user ${email}`);

  const browser = await puppeteer.launch({
    headless: true,
    acceptInsecureCerts: true,
    browser: 'chrome',
    executablePath: getChromePath(),
    args: ['--no-sandbox', '--disable-setuid-sandbox'],
  });

  const page = await browser.newPage();
  await page.goto(adminUrl);

  await page.waitForSelector('xpath///*[.="Sign in"]', {
    timeout: 10000,
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

    const authTokens = await page.evaluate(() => {
      let idToken = '';
      let accessToken = '';
      let refreshToken = '';
      let expiry = 0;

      for (let i = 0; i < localStorage.length; i += 1) {
        const key = localStorage.key(i) as string;
        const json = JSON.parse(localStorage.getItem(key) as string);

        if (key.includes('-accesstoken-')) {
          accessToken = json.secret;
          expiry = json.expiresOn;
        }

        if (key.includes('-refreshtoken-')) {
          refreshToken = json.secret;
        }

        if (key.includes('-idtoken-')) {
          idToken = json.secret;
        }
      }

      return {
        id_token: idToken,
        access_token: accessToken,
        refresh_token: refreshToken,
        expires_at: new Date(expiry * 1000),
      };
    });

    return {
      userName,
      authTokens: {
        accessToken: authTokens.access_token,
        refreshToken: authTokens.refresh_token,
        expiryDate: authTokens.expires_at,
      },
    };
  } finally {
    await browser.close();
  }
};

const getAuthTokensAzure = async (
  page: Page,
  email: string,
  password: string,
) => {
  await page.waitForSelector("xpath///*[.='Sign in']", {
    visible: true,
    timeout: 10000,
  });

  const emailInput = await page.$("xpath///*[@name='loginfmt']");
  await emailInput!.type(email);

  const btn = await page.$("xpath///*[@type='submit']");
  await btn!.click();

  try {
    await page.waitForSelector("xpath///*[.='Enter password']", {
      visible: true,
      timeout: 10000,
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
    throw e;
  }

  const pwdInput = await page.$("xpath///*[@name='passwd']");
  await pwdInput!.type(password);

  const signInBtn = await page.$("xpath///*[@type='submit']");
  await signInBtn!.click();

  try {
    await page.waitForSelector("xpath///*[.='Stay signed in?']", {
      visible: true,
      timeout: 10000,
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

  await page.waitForSelector("xpath///*[.='Dashboard']", {
    visible: true,
    timeout: 10000,
  });
};

const getAuthTokensKeycloak = async (
  page: Page,
  email: string,
  password: string,
) => {
  await page.waitForSelector(
    "xpath///*[contains(text(), 'Sign in to your account')]",
    {
      visible: true,
      timeout: 10000,
    },
  );

  const emailInput = await page.$("xpath///*[@id='username']");
  await emailInput!.type(email);

  const pwdInput = await page.$("xpath///*[@name='password']");
  await pwdInput!.type(password);

  const signInBtn = await page.$("xpath///*[@type='submit']");
  await signInBtn!.click();

  await page.waitForSelector("xpath///*[.='Dashboard']", {
    visible: true,
    timeout: 10000,
  });
};

export default getAuthTokens;
