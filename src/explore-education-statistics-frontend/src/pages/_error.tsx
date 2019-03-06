import { NextContext } from 'next';
import React, { Component, ReactNode } from 'react';
import { Helmet } from 'react-helmet';

interface Props {
  errorPage?: ReactNode;
}

const statusCodeTitles: { [code: number]: string } = {
  404: 'Page not found',
  500: 'Sorry, there is a problem with the service',
};

const statusCodePages: { [code: number]: ReactNode } = {
  404: () => (
    <>
      <h1>{statusCodeTitles[404]}</h1>

      <p>If you typed the web address, check it is correct.</p>
      <p>If you pasted the web address, check you copied the entire address.</p>
    </>
  ),
  500: () => (
    <>
      <h1>{statusCodeTitles[500]}</h1>

      <p>Please try again later.</p>
    </>
  ),
};

class ErrorPage extends Component<Props> {
  public static getInitialProps({ res, err }: NextContext) {
    const statusCode = res ? res.statusCode : 500;
    const errorMessage = err ? err.message : '';

    if (errorMessage) {
      return {
        errorPage: (
          <>
            <h1>There was a problem</h1>

            <p>{errorMessage}</p>
          </>
        ),
        pageTitle: statusCodeTitles[statusCode] || statusCodeTitles[500],
      };
    }

    return {
      errorPage: statusCodePages[statusCode],
      pageTitle: statusCodeTitles[statusCode],
    };
  }

  public render() {
    const { errorPage } = this.props;

    return (
      <div className="govuk-grid-row">
        <Helmet>
          <title>Page Not Found - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-column-two-thirds">
          {errorPage ? (
            errorPage
          ) : (
            <>
              <h1>An error occurred</h1>

              <p>Something went wrong</p>
            </>
          )}
        </div>
      </div>
    );
  }
}

export default ErrorPage;
