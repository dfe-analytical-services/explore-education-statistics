import { useApplicationInsights } from '@common/contexts/ApplicationInsightsContext';
import Link from '@frontend/components/Link';
import NotFoundPage from '@frontend/modules/NotFoundPage';
import { AxiosError } from 'axios';
import { NextPage, NextPageContext } from 'next';
import React, { useEffect } from 'react';
import Page from '../components/Page';

interface Props {
  error?: NextPageContext['err'];
  statusCode: number;
}

const ErrorPage: NextPage<Props> = ({ statusCode, error }) => {
  const appInsights = useApplicationInsights();

  useEffect(() => {
    if (error && appInsights) {
      // `error` is not actually an Error instance,
      // (it's an object) so we have to convert
      // it into one first before tracking it.
      const exception = new Error(error.message);
      exception.name = error.name;
      exception.stack = error.stack;

      appInsights.trackException({ exception });
    }
  }, [appInsights, error]);

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

ErrorPage.getInitialProps = ({ res, err: error }: NextPageContext): Props => {
  let statusCode = res?.statusCode;

  if (error) {
    const axiosError = error as AxiosError;

    if (axiosError.isAxiosError) {
      statusCode = axiosError.response?.status;
    }
  }

  return {
    error,
    statusCode: statusCode ?? 500,
  };
};

export default ErrorPage;
