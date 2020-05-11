import Link from '@frontend/components/Link';
import NotFoundPage from '@frontend/modules/NotFoundPage';
import { NextPageContext } from 'next';
import React from 'react';
import Page from '../components/Page';

interface Props {
  errorMessage?: string;
  statusCode: number;
}

const ErrorPage = ({ statusCode }: Props) => {
  switch (statusCode) {
    case 404:
      return <NotFoundPage />;
    default:
      return (
        <Page title="Sorry, there's a problem with the service">
          <p>Try again later.</p>
          <p>
            In the meantime, if you need any help or support{' '}
            <Link to="/contact-us">
              contact our Explore education statistics team
            </Link>
            .
          </p>
        </Page>
      );
  }
};

ErrorPage.getInitialProps = ({ res, err }: NextPageContext): Props => {
  return {
    errorMessage: err?.message ?? '',
    statusCode: res?.statusCode ?? 500,
  };
};

export default ErrorPage;
