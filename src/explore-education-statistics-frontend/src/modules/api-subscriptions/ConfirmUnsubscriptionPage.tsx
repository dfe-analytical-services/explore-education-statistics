import Button from '@common/components/Button';
import { Dictionary } from '@common/types';
import Panel from '@common/components/Panel';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import { DataSetFile } from '@frontend/services/dataSetFileService';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import apiNotificationService from '@frontend/services/apiNotificationService';
import React, { useState } from 'react';
import { GetServerSideProps, NextPage } from 'next';
import Head from 'next/head';
import { QueryClient } from '@tanstack/react-query';
import { isAxiosError } from 'axios';

interface Props {
  dataSetFile: DataSetFile;
  token: string;
}

const ConfirmUnsubscriptionPage: NextPage<Props> = ({ dataSetFile, token }) => {
  const [submitted, toggleSubmitted] = useToggle(false);
  const [unsubscribeError, setUnsubscribeError] = useState<string>();
  const { id: dataSetId, title } = dataSetFile;

  const onConfirm = async () => {
    try {
      await apiNotificationService.confirmUnsubscription(dataSetId, token);
    } catch (error) {
      setUnsubscribeError(() => {
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
    switch (unsubscribeError) {
      case 'ApiSubscriptionHasNotBeenVerified':
        return <p>The subscription has not been verified.</p>;
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
        return <p>You are not subscribed to this data set.</p>;
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

  if (unsubscribeError) {
    return <Page title="Unsubscribe failed">{getErrorMessage()}</Page>;
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
          link: `/data-catalogue/data-set/${dataSetId}`,
        },
      ]}
    >
      <Head>
        <meta name="robots" content="noindex,nofollow" />
      </Head>
      {submitted ? (
        <Panel headingTag="h2" title="Unsubscribed">
          <p>You have successfully unsubscribed from these updates.</p>
          <p>
            <Link to={`/data-catalogue/data-set/${dataSetId}`}>
              View {title}
            </Link>
          </p>
        </Panel>
      ) : (
        <>
          <p>
            Please confirm you wish to unsubscribe from notifications about this
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

export default ConfirmUnsubscriptionPage;
