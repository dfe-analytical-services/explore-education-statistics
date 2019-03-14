import { NextContext } from 'next';
import React, { Component } from 'react';
import Page from '../components/Page';
import PageTitle from '../components/PageTitle';

interface Props {
  errorMessage: string;
  statusCode: number;
}

class ErrorPage extends Component<Props> {
  private static statusCodeTitles: { [code: number]: string } = {
    404: 'Page not found',
    500: 'Sorry, there is a problem with the service',
  };

  public static getInitialProps({ res, err }: NextContext): Props {
    const statusCode = res ? res.statusCode : 500;
    const errorMessage = err ? err.message : '';

    return {
      errorMessage,
      statusCode,
    };
  }

  private getStatusCodePage() {
    switch (this.props.statusCode) {
      case 404:
        return (
          <>
            <PageTitle title={ErrorPage.statusCodeTitles[404]} />

            <p>If you typed the web address, check it is correct.</p>
            <p>
              If you pasted the web address, check you copied the entire
              address.
            </p>
          </>
        );
      default:
        return (
          <>
            <PageTitle title={ErrorPage.statusCodeTitles[500]} />

            <p>Please try again later.</p>
          </>
        );
    }
  }

  public render() {
    const { errorMessage, statusCode } = this.props;
    const pageTitle = errorMessage
      ? ErrorPage.statusCodeTitles[500]
      : ErrorPage.statusCodeTitles[statusCode];

    return (
      <Page>
        {errorMessage ? (
          <>
            <PageTitle title={pageTitle} />

            <p>{errorMessage}</p>
          </>
        ) : (
          this.getStatusCodePage()
        )}
      </Page>
    );
  }
}

export default ErrorPage;
