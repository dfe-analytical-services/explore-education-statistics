import Button from '@common/components/Button';
import publicationService, {
  PublicationTitle,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import notificationService from '@frontend/services/notificationService';
import classNames from 'classnames';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import { redirect } from 'next/navigation';
import styles from './SubscriptionPage.module.scss';

interface Props {
  slug: string;
  data: PublicationTitle;
  token: string;
}

const ConfirmSubscriptionPage: NextPage<Props> = ({ data, slug, token }) => {
  const onConfirmClicked = async () => {
    await notificationService.confirmPendingSubscription(data.id, token);

    // Don't think we actually need this since the awaited request above returns a redirect
    redirect(`/subscriptions?slug=${slug}&verified=true`);
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
      <div
        className={classNames(
          'govuk-panel',
          'govuk-panel--confirmation',
          styles.panelContainer,
        )}
      >
        <h1 className="govuk-panel__title">{data.title}</h1>
        <div className="govuk-panel__body">
          Please confirm you wish to subscribe to notifications about this
          publication
        </div>
        <Button type="submit" onClick={onConfirmClicked}>
          Confirm
        </Button>
      </div>
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

export default ConfirmSubscriptionPage;
