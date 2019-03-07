import { Container, default as BaseApp } from 'next/app';
import React from 'react';
import Breadcrumbs from 'src/components/Breadcrumbs';
import PageBanner from 'src/components/PageBanner';
import PageFooter from 'src/components/PageFooter';
import PageHeader from 'src/components/PageHeader';

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
    document.body.classList.add('js-enabled');
  }

  public render() {
    const { Component, pageProps, router } = this.props;

    if (router.route.startsWith('/prototypes')) {
      return (
        <Container>
          <Component {...pageProps} />
        </Container>
      );
    }

    return (
      <Container>
        <PageHeader />

        <div className="govuk-width-container">
          <PageBanner />
          <Breadcrumbs />

          <main
            className="govuk-main-wrapper app-main-class"
            id="main-content"
            role="main"
          >
            <Component {...pageProps} />
          </main>
        </div>

        <PageFooter />
      </Container>
    );
  }
}

export default App;
