const express = require('express');
const next = require('next');
const url = require('url');

const app = next({
  dev: process.env.NODE_ENV !== 'production',
  dir: './src',
});

const handleRequest = app.getRequestHandler();

async function startServer(port = 3000) {
  try {
    await app.prepare();
  } catch (err) {
    console.error(err);
    process.exit(1);
  }

  const server = express();

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

  server.get('/statistics/:publication/:release?', (req, res) => {
    return app.render(req, res, '/statistics/publication', {
      publication: req.params.publication,
      release: req.params.release,
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
