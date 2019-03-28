import { Container, default as BaseApp } from 'next/app';
import Router from 'next/router';
import React from 'react';
import Helmet from 'react-helmet';
import { initGA, logPageView } from '../utils/analytics';
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
      initGA();
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
