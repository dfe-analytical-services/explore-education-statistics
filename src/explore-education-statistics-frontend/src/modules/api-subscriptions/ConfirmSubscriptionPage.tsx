import Button from '@common/components/Button';
import Panel from '@common/components/Panel';
import { Dictionary } from '@common/types';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import Page from '@frontend/components/Page';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import { DataSetFile } from '@frontend/services/dataSetFileService';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import Link from '@frontend/components/Link';
import apiNotificationService from '@frontend/services/apiNotificationService';
import { isAxiosError } from 'axios';
import React, { useState } from 'react';
import { GetServerSideProps, NextPage } from 'next';
import Head from 'next/head';
import { QueryClient } from '@tanstack/react-query';

interface Props {
  dataSetFile: DataSetFile;
  token: string;
}

const ConfirmSubscriptionPage: NextPage<Props> = ({ dataSetFile, token }) => {
  const [submitted, toggleSubmitted] = useToggle(false);
  const [subscriptionError, setSubscriptionError] = useState<string>();

  const { id, title } = dataSetFile;

  const onConfirm = async () => {
    try {
      await apiNotificationService.confirmPendingSubscription(id, token);
    } catch (error) {
      setSubscriptionError(() => {
        if (isAxiosError(error) && error.response?.status === 404) {
          return 'NotFound';
        }
        return isAxiosError(error) && error.response?.data
          ? error.response.data.errors[0].code
          : 'ConfirmationError';
      });

      logger.error(error);
    }

    toggleSubmitted.on();
  };

  const getErrorMessage = () => {
    switch (subscriptionError) {
      case 'ApiPendingSubscriptionAlreadyExpired':
        return (
          <>
            <p>
              Your subscription verification token has expired. You can try
              again by re-subscribing from the{' '}
              <Link to={`/data-catalogue/data-set/${id}`}>
                data set's main screen.
              </Link>
            </p>
            <p>
              If this issue persists, please contact{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>{' '}
              for support with details of the data set you are trying to
              subscribe to.
            </p>
          </>
        );
      case 'ApiVerifiedSubscriptionAlreadyExists':
        return (
          <>
            <p>You are already subscribed to this data set.</p>
            <p>
              <Link to={`/data-catalogue/data-set/${id}`}>View {title}</Link>
            </p>
          </>
        );
      case 'AuthorizationTokenInvalid':
        return (
          <>
            <p>
              The authorization token is invalid. You can try again by copy and
              pasting or clicking the unsubscribe link in your email.
            </p>
            <p>
              If this issue persists, please contact{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>{' '}
              for support with details of the data set you are trying to
              subscribe to.
            </p>
          </>
        );
      case 'NotFound':
        return <p>There is no pending subscription for this data set.</p>;
      default:
        return (
          <>
            <p>There has been a problem, please try again.</p>
            <p>
              If this issue persists, please contact{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>{' '}
              for support with details of the data set you are trying to
              subscribe to.
            </p>
          </>
        );
    }
  };

  if (subscriptionError) {
    return <Page title="Verification failed">{getErrorMessage()}</Page>;
  }

  return (
    <Page
      title={title}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Data catalogue', link: '/data-catalogue' },
        {
          name: title,
          link: `/data-catalogue/data-set/${id}`,
        },
      ]}
    >
      <Head>
        <meta name="robots" content="noindex,nofollow" />
      </Head>
      {submitted ? (
        <Panel headingTag="h2" title="Subscribed">
          <p>You have successfully subscribed to these updates.</p>
          <p>
            <Link to={`/data-catalogue/data-set/${id}`}>View {title}</Link>
          </p>
        </Panel>
      ) : (
        <>
          <p>
            Please confirm you wish to subscribe to notifications about this
            data set.
          </p>
          <Button onClick={onConfirm}>Confirm</Button>
        </>
      )}
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { dataSetId, token } = query as Dictionary<string>;

    const queryClient = new QueryClient();

    const dataSetFile = await queryClient.fetchQuery(
      dataSetFileQueries.get(dataSetId),
    );

    return {
      props: {
        dataSetFile,
        token,
      },
    };
  },
);
export default ConfirmSubscriptionPage;
