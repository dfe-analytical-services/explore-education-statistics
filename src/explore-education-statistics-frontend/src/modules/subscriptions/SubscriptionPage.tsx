import { NextContext } from 'next';
import React, { Component } from 'react';
import GoToTopLink from 'src/components/GoToTopLink';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import publicationService, { Release } from 'src/services/publicationService';
import SubscriptionForm from './components/SubscriptionForm';

interface Props {
  publication: string;
  data: Release;
}

class SubscriptionPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    publication: string;
  }>) {
    const { publication } = query;

    const request = publicationService.getPublication(publication);

    const data = await request;

    return {
      data,
      publication,
    };
  }

  public render() {
    const { data, publication } = this.props;

    return (
      <Page
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/statistics' },
          { name: data.title, link: `/statistics/${publication}` },
          { name: 'subscribe' },
        ]}
      >
        <PageTitle title={`${data.title} : Subscribe`} />

        <SubscriptionForm />

        <hr />

        <GoToTopLink />
      </Page>
    );
  }
}

export default SubscriptionPage;
