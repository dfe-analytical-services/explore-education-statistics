import { logPageView } from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
import CookieBanner from '@frontend/components/CookieBanner';
import BaseApp, { Container, NextAppContext } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import './_app.scss';
import { CookiesProvider, Cookies } from 'react-cookie';

process.env.APP_ROOT_ID = '__next';

interface Props {
  cookies: Cookies;
}

class App extends BaseApp<Props> {
  public static async getInitialProps({ Component, ctx }: NextAppContext) {
    let pageProps = {};

    if (Component.getInitialProps) {
      pageProps = await Component.getInitialProps(ctx);
    }

    // @ts-ignore
    return { pageProps, cookies: ctx.req.universalCookies.cookies };
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
    const { Component, pageProps, cookies } = this.props;

    return (
      <Container>
        <Helmet titleTemplate="%s - GOV.UK" />
        <CookiesProvider cookies={new Cookies(cookies)}>
          <CookieBanner />
          <Component {...pageProps} />
        </CookiesProvider>
      </Container>
    );
  }
}

export default App;
