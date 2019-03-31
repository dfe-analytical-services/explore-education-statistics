import 'core-js/fn/array/virtual/flat-map';
import 'core-js/fn/array/virtual/includes';
import 'cross-fetch/polyfill';

import { Container, default as BaseApp } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import { logPageView } from 'src/services/googleAnalyticsService';
import './_app.scss';

class App extends BaseApp {
  public static async getInitialProps({ Component, ctx }: any) {
    let pageProps = {};

    if (Component.getInitialProps) {
      pageProps = await Component.getInitialProps(ctx);
    }

    return { pageProps };
  }

  public componentDidMount(): void {
    if (process.env.GA_TRACKING_ID !== undefined) {
      logPageView();

      if (Router.router !== null) {
        Router.router.events.on('routeChangeComplete', logPageView);
      }
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
