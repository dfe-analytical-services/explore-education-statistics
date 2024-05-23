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
  'https://*.googletagmanager.com',
  'https://*.google-analytics.com',
  'https://*.analytics.google.com',
  'https://dc.services.visualstudio.com/v2/track',
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
  port: process.env.PORT || 3000,
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
    if (
      input === undefined ||
      input === null ||
      input.length === 0 ||
      !input.endsWith(pattern)
    ) {
      return input;
    }

    return `${input.slice(0, -pattern.length)}${replacement}`;
  }

  function isRedirectionRequired(request) {
    const hostnameIncludesWWW = request.hostname.startsWith('www');
    let redirectionPath = request.path;

    // Redirect URLs with trailing slash to equivalent without slash with 301
    redirectionPath = replaceLastOccurrence(redirectionPath, '/', '');

    // Bots are adding /1000 and Google isn't yet omitting them from its index automatically,
    // so we should redirect away from them
    redirectionPath = replaceLastOccurrence(redirectionPath, '/1000', '');

    // Search wrongly indexed pages from Google Search Console for matches
    // Redirect away if found to (eventually) clear routes from index
    const urlMatch = seoRedirects.find(source => source.from === request.url);
    if (urlMatch !== undefined) {
      redirectionPath.path = urlMatch.to;
    }

    const redirectionRequired =
      hostnameIncludesWWW || redirectionPath !== request.path;

    if (redirectionRequired) {
      // PUBLIC_URL ends in a slash, and redirectionPath starts with one, but we only want one in total
      const newUrl = new URL(
        `${replaceLastOccurrence(
          process.env.PUBLIC_URL,
          '/',
          '',
        )}${redirectionPath}`,
      );

      // Restore any search parameters on original request
      Object.keys(request.query).forEach(paramKey => {
        newUrl.searchParams.append(paramKey, request.param(paramKey));
      });

      return newUrl.href;
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

    const potentialRedirectUrl = isRedirectionRequired(req);
    if (potentialRedirectUrl !== undefined) {
      return res.redirect(301, potentialRedirectUrl);
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
