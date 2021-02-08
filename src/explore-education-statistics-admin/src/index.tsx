import '@admin/polyfill';
import configureAxios from '@admin/services/utils/configureAxios';
import { enableES5 } from 'immer';
import React from 'react';
import ReactDOM from 'react-dom';
import * as serviceWorker from './serviceWorker';

enableES5();
configureAxios();

import('./App').then(({ default: App }) => {
  ReactDOM.render(<App />, document.getElementById('root'));
});

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
