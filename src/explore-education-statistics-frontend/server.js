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
  process.env.NEXT_PUBLIC_CONTENT_API_BASE_URL.replace('/api', ''),
  process.env.NEXT_PUBLIC_DATA_API_BASE_URL.replace('/api', ''),
  process.env.NEXT_PUBLIC_NOTIFICATION_API_BASE_URL.replace('/api', ''),
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
const app = next({ dev });
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

  server.use(
    helmet({
      contentSecurityPolicy: {
        directives: {
          defaultSrc: ["'self'"],
          scriptSrc: cspScriptSrc,
          styleSrc: ["'self'", "'unsafe-inline'"],
          imgSrc: [
            "'self'",
            process.env.NEXT_PUBLIC_CONTENT_API_BASE_URL.replace('/api', ''),
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
