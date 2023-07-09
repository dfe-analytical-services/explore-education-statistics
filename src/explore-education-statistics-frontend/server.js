require('core-js/features/array/flat-map');
require('core-js/features/array/flat');

const next = require('next');
const path = require('path');
const appInsights = require('applicationinsights');
const express = require('express');
const basicAuth = require('express-basic-auth');

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
  // eslint-disable-next-line no-console
  console.error(e);
  process.exit(1);
});
