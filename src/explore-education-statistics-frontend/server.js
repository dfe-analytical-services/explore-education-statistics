require('core-js/fn/array/flat-map');
require('core-js/fn/array/flatten');

if (!Array.prototype.flat) {
  // eslint-disable-next-line no-extend-native
  Array.prototype.flat = Array.prototype.flatten;
}

const appInsights = require('applicationinsights');

// AI Key should be loaded via enviroment variable APPINSIGHTS_INSTRUMENTATIONKEY
if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY) {
  appInsights
    .setup()
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
const referrerPolicy = require('referrer-policy');
const url = require('url');
const cookiesMiddleware = require('universal-cookie-express');

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
    process.env.CONTENT_API_BASE_URL,
    process.env.DATA_API_BASE_URL,
    process.env.FUNCTION_API_BASE_URL,
    'http://*.hotjar.com:*',
    'https://*.hotjar.com:*',
    'https://vc.hotjar.io:*',
    'wss://*.hotjar.com',
    'https://dc.services.visualstudio.com/v2/track',
  ];
  const cspScriptSrc = [
    "'self'",
    'https://www.google-analytics.com/',
    'https://static.hotjar.com/',
    'https://script.hotjar.com',
    "'unsafe-inline'",
    "'unsafe-eval'",
  ];

  const server = express();

  // Use Helmet for configuration of headers and disable express powered by header
  server.disable('x-powered-by');
  server.use(helmet());
  server.use(cookiesMiddleware());
  server.use(
    helmet.contentSecurityPolicy({
      directives: {
        defaultSrc: ["'self'"],
        scriptSrc: cspScriptSrc,
        styleSrc: ["'self'", "'unsafe-inline'"],
        imgSrc: [
          "'self'",
          'data:',
          'https://www.google-analytics.com/',
          'https://insights.hotjar.com',
          'https://static.hotjar.com',
          'https://script.hotjar.com',
        ],
        fontSrc: [
          "'self'",
          'https://static.hotjar.com',
          'https://script.hotjar.com',
        ],
        connectSrc:
          process.env.NODE_ENV !== 'production' ? ['*'] : cspConnectSrc,
        frameSrc: ["'self'", 'https://vars.hotjar.com '],
        frameAncestors: ["'self'"],
        childSrc: ["'self'", 'https://vars.hotjar.com'],
      },
    }),
  );
  server.use(
    helmet.featurePolicy({
      features: {
        fullscreen: ["'self'"],
      },
    }),
  );
  server.use(referrerPolicy({ policy: 'no-referrer-when-downgrade' }));

  server.use((req, res, next) => {
    if (req.path !== '/' && req.path.endsWith('/')) {
      // Strip trailing slashes as these
      // don't work well with Next
      const parsedUrl = url.parse(req.url);
      const redirectUrl = req.path.slice(0, -1) + (parsedUrl.search || '');

      res.redirect(301, redirectUrl);
    } else {
      next();
    }
  });

  if (process.env.BASIC_AUTH === 'true') {
    server.use(
      basicAuth({
        users: {
          [process.env.BASIC_AUTH_USERNAME]: process.env.BASIC_AUTH_PASSWORD,
        },
        challenge: true,
      }),
    );
  }

  server.get('/find-statistics/:publication/:release?', (req, res) => {
    return app.render(req, res, '/find-statistics/publication', {
      publication: req.params.publication,
      release: req.params.release,
    });
  });

  server.get('/methodology/:publication/:release?', (req, res) => {
    return app.render(req, res, '/methodology/methodology', {
      publication: req.params.publication,
      release: req.params.release,
    });
  });

  server.get('/data-tables/permalink/:permalink/', (req, res) => {
    return app.render(req, res, '/data-tables/permalink', {
      permalink: req.params.permalink,
    });
  });

  server.get('/data-tables/:publicationSlug?', (req, res) => {
    return app.render(req, res, '/data-tables', {
      publicationSlug: req.params.publicationSlug,
    });
  });

  server.get('*', (req, res) => handleRequest(req, res));

  server.listen(port, err => {
    if (err) {
      throw err;
    }

    console.log(`Server started on port ${port}`);
  });
}

startServer().catch(err => {
  console.error(err);
  process.exit(1);
});
