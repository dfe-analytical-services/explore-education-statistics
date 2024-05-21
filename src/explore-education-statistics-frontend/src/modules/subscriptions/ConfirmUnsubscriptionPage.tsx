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
import Head from 'next/head';

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
  const [unsubscribedSubscription, setUnsubscribedSubscription] = useState<
    Subscription | undefined
  >(undefined);

  const onConfirmClicked = async () => {
    // TODO: Currently we get a 400 if the token isn't valid, but aren't
    // properly handling this like we are for verification errors.
    const response = await notificationService.confirmUnsubscription(
      data.id,
      token,
    );

    setUnsubscribedSubscription(response);
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
      <Head>
        <meta name="robots" content="noindex,nofollow" />
        <meta name="googlebot" content="noindex,nofollow" />
      </Head>
      {unsubscribedSubscription ? (
        <SubscriptionStatusMessage
          title={data.title}
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
