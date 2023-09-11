import '@admin/polyfill';
import configureAxios from '@admin/services/utils/configureAxios';
import App from '@admin/App';
import React from 'react';
import ReactDOM from 'react-dom';

configureAxios();

ReactDOM.render(<App />, document.getElementById('root'));
