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

interface Props {
  publicationSlug: string;
  data: PublicationTitle;
  token: string;
}

const ConfirmUnsubscriptionPage: NextPage<Props> = ({
  data,
  publicationSlug,
  token,
}) => {
  const [subscriptionState, setSubscriptionState] = useState<
    Subscription | undefined
  >(undefined);

  const onConfirmClicked = async () => {
    const response = await notificationService.confirmUnsubscription(
      data.id,
      token,
    );

    setSubscriptionState(response);
  };

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
      {subscriptionState ? (
        <SubscriptionStatusMessage
          title="Unsubscribed"
          message="You have successfully unsubscribed from these updates."
          slug={subscriptionState.slug}
        />
      ) : (
        <>
          <p>
            Please confirm you wish to unsubscribe to notifications about this
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

export default ConfirmUnsubscriptionPage;
