import { logPageView } from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
import { initApplicationInsights } from '@frontend/services/applicationInsightsService';
import BaseApp, { Container, NextAppContext } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import './_app.scss';
import { CookiesProvider, Cookies } from 'react-cookie';
import { NextContext } from 'next';

process.env.APP_ROOT_ID = '__next';

interface Props {
  cookies: Cookies;
}

class App extends BaseApp<Props> {
  public static getCookies = (ctx: NextContext) => {
    if (
      ctx.req &&
      // @ts-ignore
      ctx.req.universalCookies &&
      // @ts-ignore
      ctx.req.universalCookies.cookies
    ) {
      // @ts-ignore
      return ctx.req.universalCookies.cookies;
    }
    return { cookies: undefined };
  };

  public static async getInitialProps({ Component, ctx }: NextAppContext) {
    let pageProps = {};

    if (Component.getInitialProps) {
      pageProps = await Component.getInitialProps(ctx);
    }

    // @ts-ignore
    return { pageProps, cookies: this.getCookies(ctx) };
  }

  public componentDidMount() {
    logPageView();
    initHotJar();
    initApplicationInsights();

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
          <Component {...pageProps} />
        </CookiesProvider>
      </Container>
    );
  }
}

export default App;
