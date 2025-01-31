// Load app styles first to ensure correct style ordering
import './styles/_all.scss';
import '@admin/polyfill';
import configureAxios from '@admin/services/utils/configureAxios';
import { createRoot } from 'react-dom/client';
import React from 'react';
import * as serviceWorker from './serviceWorker';

configureAxios();

import('./App').then(({ default: App }) => {
  const container = document.getElementById('root');
  if (!container) {
    throw Error('No container found.');
  }
  const root = createRoot(container);
  root.render(<App />);
});

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
