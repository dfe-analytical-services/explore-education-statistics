import { logPageView } from '@frontend/services/googleAnalyticsService';
import BaseApp, { Container, NextAppContext } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import './_app.scss';
import { initHotJar } from '@frontend/services/hotjarService';

class App extends BaseApp {
  public static async getInitialProps({ Component, ctx }: NextAppContext) {
    let pageProps = {};

    if (Component.getInitialProps) {
      pageProps = await Component.getInitialProps(ctx);
    }

    return { pageProps };
  }

  public componentDidMount() {
    logPageView();
    initHotJar();

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
