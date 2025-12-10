import React, { useState } from 'react';
import { GetServerSideProps, NextPage } from 'next';
import Button from '@common/components/Button';
import publicationService from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import notificationService, {
  Subscription,
} from '@frontend/services/notificationService';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import VerificationErrorMessage from '@frontend/modules/subscriptions/components/VerificationErrorMessage';
import Head from 'next/head';
import Panel from '@common/components/Panel';
import Link from '@frontend/components/Link';

interface Props {
  publicationSlug: string;
  publicationTitle: string;
  publicationId: string;
  token: string;
}

const ConfirmSubscriptionPage: NextPage<Props> = ({
  publicationSlug,
  publicationTitle,
  publicationId,
  token,
}) => {
  const [confirmedSubscription, setConfirmedSubscription] = useState<
    Subscription | 'verification-error' | undefined
  >(undefined);

  const onConfirmClicked = async () => {
    const response = await notificationService.confirmPendingSubscription(
      publicationId,
      token,
    );

    setConfirmedSubscription(
      response === undefined ? 'verification-error' : response,
    );
  };

  if (confirmedSubscription === 'verification-error') {
    return (
      <Page title="Verification failed">
        <VerificationErrorMessage slug={publicationSlug} />
      </Page>
    );
  }

  return (
    <Page
      title={publicationTitle}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: publicationTitle,
          link: `/find-statistics/${publicationSlug}`,
        },
      ]}
    >
      <Head>
        <meta name="robots" content="noindex,nofollow" />
      </Head>
      <div role="status">
        {confirmedSubscription ? (
          <Panel headingTag="h2" title="Subscribed">
            <p>You have successfully subscribed to these updates.</p>
            <p>
              <Link to={`/find-statistics/${confirmedSubscription.slug}`}>
                View {publicationTitle}
              </Link>
            </p>
          </Panel>
        ) : (
          <>
            <p>
              Please confirm you wish to subscribe to notifications about this
              publication.
            </p>
            <Button onClick={onConfirmClicked}>Confirm</Button>
          </>
        )}
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { publicationSlug, token } = query as Dictionary<string>;

    const publication = await publicationService.getPublicationTitle(
      publicationSlug as string,
    );

    return {
      props: {
        publicationSlug: publicationSlug as string,
        publicationId: publication.publicationId,
        publicationTitle: publication.title,
        token,
      },
    };
  },
);

export default ConfirmSubscriptionPage;
