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
