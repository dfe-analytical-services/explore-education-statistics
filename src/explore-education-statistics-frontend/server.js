require('core-js/features/array/flat-map');
require('core-js/features/array/flat');

const appInsights = require('applicationinsights');

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
    .start();
}

const basicAuth = require('express-basic-auth');
const express = require('express');
const helmet = require('helmet');
const nextApp = require('next');
const { loadEnvConfig } = require('@next/env');
const path = require('path');
const referrerPolicy = require('referrer-policy');

loadEnvConfig(__dirname);

const app = nextApp({
  dev: process.env.NODE_ENV !== 'production',
  dir: './src',
});

const handleRequest = app.getRequestHandler();

async function startServer(port = process.env.PORT || 3000) {
  try {
    await app.prepare();
  } catch (err) {
    console.error(err);
    process.exit(1);
  }

  const cspConnectSrc = [
    "'self'",
    process.env.CONTENT_API_BASE_URL.replace('/api', ''),
    process.env.DATA_API_BASE_URL.replace('/api', ''),
    process.env.NOTIFICATION_API_BASE_URL.replace('/api', ''),
    'https://www.google-analytics.com',
    'https://dc.services.visualstudio.com/v2/track',
  ];

  const cspScriptSrc = [
    "'self'",
    'https://www.google-analytics.com/',
    "'unsafe-inline'",
    "'unsafe-eval'",
  ];

  const server = express();

  // Use Helmet for configuration of headers and disable express powered by header
  server.disable('x-powered-by');
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
            'https://www.google-analytics.com/',
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

    console.log(`Server started on port ${port}`);
  });
}

startServer().catch(err => {
  if (appInsights.defaultClient) {
    appInsights.defaultClient.trackException({ exception: err });
  }

  console.error(err);
  process.exit(1);
});
