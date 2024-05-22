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
import SubscriptionStatusMessage from '@frontend/modules/subscriptions/components/SubscriptionStatusMessage';
import Head from 'next/head';

interface Props {
  publicationSlug: string;
  publicationTitle: string;
  publicationId: string;
  token: string;
}

const ConfirmUnsubscriptionPage: NextPage<Props> = ({
  publicationSlug,
  publicationTitle,
  publicationId,
  token,
}) => {
  const [unsubscribedSubscription, setUnsubscribedSubscription] = useState<
    Subscription | undefined
  >(undefined);

  const onConfirmClicked = async () => {
    // TODO: Currently we get a 400 if the token isn't valid, but aren't
    // properly handling this like we are for verification errors.
    const response = await notificationService.confirmUnsubscription(
      publicationId,
      token,
    );

    setUnsubscribedSubscription(response);
  };

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
        <meta name="googlebot" content="noindex,nofollow" />
      </Head>
      {unsubscribedSubscription ? (
        <SubscriptionStatusMessage
          title={publicationTitle}
          message="You have successfully unsubscribed from these updates."
          slug={unsubscribedSubscription.slug}
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

    const publication = await publicationService.getPublicationTitle(
      publicationSlug as string,
    );

    return {
      props: {
        publicationSlug: publicationSlug as string,
        publicationId: publication.id,
        publicationTitle: publication.title,
        token,
      },
    };
  },
);

export default ConfirmUnsubscriptionPage;
