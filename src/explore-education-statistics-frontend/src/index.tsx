import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import { loadPolyfills } from './polyfill';
import * as serviceWorker from './serviceWorker';

loadPolyfills().then(() => {
  ReactDOM.render(<App />, document.getElementById('application'));

  // If you want your app to work offline and load faster, you can change
  // unregister() to register() below. Note this comes with some pitfalls.
  // Learn more about service workers: http://bit.ly/CRA-PWA
  serviceWorker.unregister();
});
