import '@common/polyfill';
import '@admin/polyfill';
import axiosConfigurer from '@admin/services/util/axios-configurer';
import React from 'react';
import ReactDOM from 'react-dom';

import initApplicationInsights from '@admin/services/configurationService';
import * as serviceWorker from './serviceWorker';

process.env.APP_ROOT_ID = 'root';

// EES-704 - revisit to find a better way to configure Clients used in the common project
// @ts-ignore
window.axiosConfigurer = axiosConfigurer;

import('./App').then(({ default: App }) => {
  ReactDOM.render(<App />, document.getElementById('root'));
});

initApplicationInsights();

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
