import React, { useState } from 'react';
import { GetServerSideProps, NextPage } from 'next';
import Button from '@common/components/Button';
import publicationService, {
  PublicationTitle,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import notificationService, {
  Subscription,
} from '@frontend/services/notificationService';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import SubscriptionStatusMessage from '@frontend/modules/subscriptions/components/SubscriptionStatusMessage';
import VerificationErrorMessage from '@frontend/modules/subscriptions/components/VerificationErrorMessage';

interface Props {
  publicationSlug: string;
  data: PublicationTitle;
  token: string;
}

const ConfirmSubscriptionPage: NextPage<Props> = ({
  data,
  publicationSlug,
  token,
}) => {
  const [confirmedSubscription, setConfirmedSubscription] = useState<
    Subscription | 'verification-error' | undefined
  >(undefined);

  const onConfirmClicked = async () => {
    const response = await notificationService.confirmPendingSubscription(
      data.id,
      token,
    );

    setConfirmedSubscription(
      response === undefined ? 'verification-error' : response,
    );
  };

  if (confirmedSubscription === 'verification-error') {
    return (
      <Page title="Verification failed">
        <VerificationErrorMessage />
      </Page>
    );
  }

  return (
    <Page
      title={data.title}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        { name: data.title, link: `/find-statistics/${publicationSlug}` },
      ]}
    >
      {confirmedSubscription ? (
        <SubscriptionStatusMessage
          title="Subscription verified"
          message="You have successfully subscribed to these updates."
          slug={confirmedSubscription.slug}
        />
      ) : (
        <>
          <p>
            Please confirm you wish to subscribe to notifications about this
            publication.
          </p>
          <Button onClick={onConfirmClicked}>Confirm</Button>
        </>
      )}
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { publicationSlug, token } = query as Dictionary<string>;

    const data = await publicationService.getPublicationTitle(
      publicationSlug as string,
    );

    return {
      props: {
        data,
        publicationSlug,
        token,
      },
    };
  },
);

export default ConfirmSubscriptionPage;
