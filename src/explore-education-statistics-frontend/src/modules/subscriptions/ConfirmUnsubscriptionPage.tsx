import Button from '@common/components/Button';
import publicationService, {
  PublicationTitle,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import notificationService from '@frontend/services/notificationService';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import { redirect } from 'next/navigation';

interface Props {
  slug: string;
  data: PublicationTitle;
  token: string;
}

const ConfirmUnsubscriptionPage: NextPage<Props> = ({ data, slug, token }) => {
  const onConfirmClicked = async () => {
    await notificationService.confirmUnsubscription(data.id, token);

    // Don't think we actually need this since the awaited request above returns a redirect.
    // We could actually return a 200 and handle the UI logic here without redirects, but that's a bigger change...
    redirect(`/subscriptions?slug=${slug}&unsubscribed=true`);
  };

  return (
    <Page
      title={data.title}
      caption="Notify me"
      breadcrumbLabel="Notify me"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        { name: data.title, link: `/find-statistics/${slug}` },
      ]}
    >
      <p>
        Please confirm you wish to unsubscribe to notifications about this
        publication.
      </p>
      <Button onClick={onConfirmClicked}>Confirm</Button>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { slug, token } = query as Dictionary<string>;

    const data = await publicationService.getPublicationTitle(slug as string);

    return {
      props: {
        data,
        slug,
        token,
      },
    };
  },
);

export default ConfirmUnsubscriptionPage;
