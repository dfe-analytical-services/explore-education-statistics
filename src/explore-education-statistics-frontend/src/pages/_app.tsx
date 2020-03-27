import '@frontend/loadEnv';
import '@frontend/polyfill';

import { initApplicationInsights } from '@frontend/services/applicationInsightsService';
import { logPageView } from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
import { NextPageContext } from 'next';
import BaseApp, { AppContext } from 'next/app';
import Router from 'next/router';
import React from 'react';
import { Cookies, CookiesProvider } from 'react-cookie';
import './_app.scss';

interface Props {
  cookies: Cookies;
}

class App extends BaseApp<Props> {
  public static getCookies = (ctx: NextPageContext) => {
    // @ts-ignore
    if (ctx?.req?.universalCookies?.cookies) {
      // @ts-ignore
      return ctx.req.universalCookies.cookies;
    }

    return { cookies: undefined };
  };

  public static async getInitialProps({ Component, ctx }: AppContext) {
    let pageProps = {};

    if (Component.getInitialProps) {
      pageProps = await Component.getInitialProps(ctx);
    }

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
    const { Component, pageProps, cookies } = this.props;

    return (
      <CookiesProvider cookies={new Cookies(cookies)}>
        <Component {...pageProps} />
      </CookiesProvider>
    );
  }
}

export default App;
