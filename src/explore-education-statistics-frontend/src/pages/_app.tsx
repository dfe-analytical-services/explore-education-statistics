import { logPageView } from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
import BaseApp, { Container, NextAppContext } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import './_app.scss';
import Link from '@frontend/components/Link';
import { RegisterLinkRenderer } from 'explore-education-statistics-common/src/components/Link';

process.env.APP_ROOT_ID = '__next';

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

    RegisterLinkRenderer(Link);

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
