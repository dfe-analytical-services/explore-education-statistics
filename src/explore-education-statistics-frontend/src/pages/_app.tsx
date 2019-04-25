import loadPolyfill from '@common/polyfill';
import { logPageView } from '@frontend/services/googleAnalyticsService';
import 'core-js/fn/array/virtual/flat-map';
import 'core-js/fn/array/virtual/includes';
import 'cross-fetch/polyfill';
import { Container, default as BaseApp, NextAppContext } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import './_app.scss';

class App extends BaseApp {
  public static async getInitialProps({ Component, ctx }: NextAppContext) {
    let pageProps = {};

    if (Component.getInitialProps) {
      pageProps = await Component.getInitialProps(ctx);
    }

    return { pageProps };
  }

  public async componentWillMount() {
    if (process.browser) {
      await loadPolyfill();
    }
  }

  public componentDidMount() {
    logPageView();

    if (Router.router !== null) {
      Router.router.events.on('routeChangeComplete', logPageView);
    }

    document.body.classList.add('js-enabled');
  }

  public render() {
    const { Component, pageProps } = this.props;

    return (
      <Container>
        <Helmet titleTemplate="%s - GOV.UK" />
        <Component {...pageProps} />
      </Container>
    );
  }
}

export default App;
