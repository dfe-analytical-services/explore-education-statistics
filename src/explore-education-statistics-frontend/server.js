require('core-js/features/array/flat-map');
require('core-js/features/array/flat');

const next = require('next');
const path = require('path');
const { loadEnvConfig } = require('@next/env');
const appInsights = require('applicationinsights');
const express = require('express');
const basicAuth = require('express-basic-auth');
const helmet = require('helmet');
const referrerPolicy = require('referrer-policy');

const seoRedirects = require('./redirects');

loadEnvConfig(__dirname);

if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY) {
  appInsights
    .setup(process.env.APPINSIGHTS_INSTRUMENTATIONKEY)
    .setAutoDependencyCorrelation(true)
    .setAutoCollectRequests(true)
    .setAutoCollectPerformance(true)
    .setAutoCollectExceptions(true)
    .setAutoCollectDependencies(true)
    .setAutoCollectConsole(true)
    .setUseDiskRetryCaching(true)
    .setSendLiveMetrics(true)
    .start();
}

const cspConnectSrc = [
  "'self'",
  `${process.env.CONTENT_API_BASE_URL}/`,
  `${process.env.DATA_API_BASE_URL}/`,
  `${process.env.NOTIFICATION_API_BASE_URL}/`,
  `${process.env.PUBLIC_API_BASE_URL}/`,
  `${process.env.AZURE_SEARCH_ENDPOINT}`,
  'https://*.googletagmanager.com',
  'https://*.google-analytics.com',
  'https://*.analytics.google.com',
  'https://dc.services.visualstudio.com/v2/track',

  // To allow a {PUBLIC_URL}/api requests when browsing the public site
  // from azurewebsites.net or azurefd.net (app service or front door urls),
  // which we may want to do for testing/debugging.
  // The regex ensures this URL is correct regardless of whether PUBLIC_URL ends with a slash
  `${process.env.PUBLIC_URL.replace(/\/$/, '')}/api/`,
];

const cspScriptSrc = [
  "'self'",
  'https://*.googletagmanager.com',
  'https://*.google-analytics.com/',
  'https://*.analytics.google.com',
  "'unsafe-inline'",
  "'unsafe-eval'",
];

const port = process.env.PORT || 3000;
const dev = process.env.NODE_ENV !== 'production';
const url = new URL(process.env.PUBLIC_URL);

const app = next({
  dev,
  hostname: url.hostname,
  port,
});
const handleRequest = app.getRequestHandler();

async function startServer() {
  try {
    await app.prepare();
  } catch (e) {
    // eslint-disable-next-line no-console
    console.error(e);
    process.exit(1);
  }

  const server = express();

  function replaceLastOccurrence(input, pattern, replacement) {
    if (!input || !input.endsWith(pattern)) {
      return input;
    }

    return `${input.slice(0, -pattern.length)}${replacement}`;
  }

  /**
   * @returns An absolute URL if redirection is required; undefined otherwise.
   */
  function getRedirectUrl(request) {
    let redirectPath = request.path.toLowerCase();

    // Redirect URLs with trailing slash to equivalent without slash with 301
    redirectPath = replaceLastOccurrence(redirectPath, '/', '');

    // Bots are adding /1000 and Google isn't yet omitting them from its index automatically,
    // so we should redirect away from them
    redirectPath = replaceLastOccurrence(redirectPath, '/1000', '');

    // Search wrongly indexed pages from Google Search Console for matches
    // Redirect away if found to (eventually) clear routes from index
    redirectPath = seoRedirects[redirectPath] ?? redirectPath;

    const redirectionRequired =
      request.hostname.startsWith('www') ||
      redirectPath !== request.path.toLowerCase();

    if (redirectionRequired) {
      // Restore any search parameters on original request
      const requestUrl = new URL(request.url, url);
      const redirectUrl = new URL(`${redirectPath}${requestUrl.search}`, url);

      return redirectUrl.href;
    }

    return undefined;
  }

  server.use((req, res, nextFunc) => {
    // Return early for speed if redirect definitely isn't required
    if (
      (req.url === '/' && !req.hostname.startsWith('www')) ||
      req.url.startsWith('/assets') ||
      req.url.startsWith('/_next')
    ) {
      nextFunc();
      return undefined;
    }

    const redirectUrl = getRedirectUrl(req);
    if (redirectUrl) {
      return res.redirect(301, redirectUrl);
    }

    nextFunc();
    return undefined;
  });

  server.use(
    helmet({
      contentSecurityPolicy: {
        directives: {
          defaultSrc: ["'self'"],
          scriptSrc: cspScriptSrc,
          styleSrc: ["'self'", "'unsafe-inline'"],
          imgSrc: [
            "'self'",
            process.env.CONTENT_API_BASE_URL.replace('/api', ''),
            'data:',
            'https://*.googletagmanager.com',
            'https://*.google-analytics.com/',
            'https://*.analytics.google.com',
          ],
          fontSrc: ["'self'"],
          connectSrc:
            process.env.NODE_ENV !== 'production' ? ['*'] : cspConnectSrc,
          frameSrc: [
            "'self'",
            'https://department-for-education.shinyapps.io/',
            'https://dfe-analytical-services.github.io/',
          ],
          frameAncestors: ["'self'"],
          childSrc: ["'self'"],
        },
      },
    }),
  );
  server.use(referrerPolicy({ policy: 'no-referrer-when-downgrade' }));

  if (process.env.BASIC_AUTH === 'true') {
    server.use(
      '/_next/static',
      express.static(path.resolve(__dirname, 'src/.next/static')),
    );

    server.use(
      basicAuth({
        users: {
          [process.env.BASIC_AUTH_USERNAME]: process.env.BASIC_AUTH_PASSWORD,
        },
        challenge: true,
      }),
    );
  }
  server.get('*', (req, res) => handleRequest(req, res));
  server.post('*', (req, res) => handleRequest(req, res));

  server.listen(port, err => {
    if (err) {
      throw err;
    }
    // eslint-disable-next-line no-console
    console.log(`Server started on http://localhost:${port}`);
  });
}

startServer().catch(e => {
  appInsights.defaultClient?.trackException({ exception: e });

  // eslint-disable-next-line no-console
  console.error(e);
  process.exit(1);
});
