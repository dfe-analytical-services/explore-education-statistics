const basicAuth = require('express-basic-auth');
const express = require('express');
const helmet = require('helmet');
const nextApp = require('next');
const referrerPolicy = require('referrer-policy');
const url = require('url');

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
        ],
        fontSrc: ["'self'", 'https://static.hotjar.com'],
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

  // Strip trailing slashes as these
  // don't work well with Next
  server.use((req, res, next) => {
    if (req.path !== '/' && req.path.endsWith('/')) {
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

  server.get('/statistics/:publication/:release?', (req, res) => {
    return app.render(req, res, '/statistics/publication', {
      publication: req.params.publication,
      release: req.params.release,
    });
  });

  server.get('/table-tool/:publicationId?', (req, res) => {
    return app.render(req, res, '/table-tool', {
      publicationId: req.params.publicationId,
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
