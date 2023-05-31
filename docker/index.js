const express = require('express');

const main = async () => {
  const app = express();

  app.get('/', (_req, res) => {
    return res.status(200).json({
      message: 'Hello world',
    });
  });

  app.listen(8000, () => {
    console.log('Server listening on port 8000');
  });
};

main().catch(e => {
  console.error(e);
});
