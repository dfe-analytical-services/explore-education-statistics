const express = require('express');
const next = require('next');

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
    if (req.path.endsWith('/')) {
      res.redirect(301, req.url.slice(0, -1));
    } else {
      next();
    }
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
