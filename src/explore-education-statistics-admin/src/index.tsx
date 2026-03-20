// Load app styles first to ensure correct style ordering
import './styles/_all.scss';
import '@admin/polyfill';
import configureAxios from '@admin/services/utils/configureAxios';
import { initAll } from 'govuk-frontend';
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

  /* 
  Running `initAll` seems to get the error: `SupportError: GOV.UK Frontend initialised
   without `<body class="govuk-frontend-supported">` from template `<script>` snippet`. 
  Using setTimeout works around this error message. 
  */
  setTimeout(() => {
    initAll(); // Required to enable the 'enhanced file upload' in `FormFileInput` under the div with the class `govuk-drop-zone`.
  }, 2000);
});

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
