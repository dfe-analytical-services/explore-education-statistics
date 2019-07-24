import { logPageView } from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
import CookieBanner from '@frontend/components/CookieBanner';
import BaseApp, { Container, NextAppContext } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import './_app.scss';
import { CookiesProvider } from 'react-cookie';

process.env.APP_ROOT_ID = '__next';

class App extends BaseApp {
  public static async getInitialProps({ Component, ctx }: NextAppContext) {
    let pageProps = {};
    let cookieBannerProps = {};

    if (Component.getInitialProps) {
      pageProps = await Component.getInitialProps(ctx);
    }

    if (CookieBanner.getInitialProps) {
      cookieBannerProps = CookieBanner.getInitialProps(ctx);
    }

    return { pageProps, cookieBannerProps };
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
    // @ts-ignore
    const { Component, pageProps, cookieBannerProps } = this.props;

    return (
      <Container>
        <Helmet titleTemplate="%s - GOV.UK" />
        <CookiesProvider>
          <CookieBanner {...cookieBannerProps} />
          <Component {...pageProps} />
        </CookiesProvider>
      </Container>
    );
  }
}

export default App;
