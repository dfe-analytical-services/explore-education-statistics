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
  process.env.CONTENT_API_BASE_URL.replace('/api', ''),
  process.env.DATA_API_BASE_URL.replace('/api', ''),
  process.env.NOTIFICATION_API_BASE_URL.replace('/api', ''),
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

  function replaceLastOccurance(input, pattern, replacement) {
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

  // Redirect URLs with trailing slash to equivalent without slash with 301
  server.use((req, res, nextNotShadowed) => {
    let newUri = req.url;
    newUri = replaceLastOccurance(newUri, '/', '');
    newUri = replaceLastOccurance(newUri, '/meta-guidance', '/data-guidance');
    newUri = replaceLastOccurance(
      newUri,
      '/download-latest-data',
      '/data-catalogue',
    );

    if (newUri !== req.url && newUri !== '') {
      return res.redirect(301, newUri);
    }
    nextNotShadowed();
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
