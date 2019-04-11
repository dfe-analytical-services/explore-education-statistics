import { NextContext } from 'next';
import React, { Component } from 'react';
import GoToTopLink from 'src/components/GoToTopLink';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import functionsService, {
  SubscriptionData,
} from 'src/services/functionsService';
import publicationService, { Release } from 'src/services/publicationService';
import SubscriptionForm, {
  SubscriptionFormSubmitHandler,
} from './components/SubscriptionForm';

interface Props {
  slug: string;
  data: Release;
}

interface State {
  slug: string;
  title: string;
}

class SubscriptionPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    slug: string;
  }>) {
    const { slug } = query;

    const request = publicationService.getPublication(slug);

    const data = await request;

    return {
      data,
      slug,
    };
  }

  public state: State = {
    slug: this.props.slug,
    title: this.props.data.title,
  };

  private handleFormSubmit: SubscriptionFormSubmitHandler = async ({
    email,
  }) => {
    if (email !== '') {
      const slug = this.state.slug;
      const title = this.state.title;
      await functionsService.subscribeToPublication({
        email,
        slug,
        title,
      });
    }
  };

  public render() {
    const { data, slug } = this.props;

    return (
      <Page
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/statistics' },
          { name: data.title, link: `/statistics/${slug}` },
          { name: 'subscribe' },
        ]}
      >
        <PageTitle title={`${data.title} : Subscribe`} />

        <SubscriptionForm onSubmit={this.handleFormSubmit} />

        <hr />

        <GoToTopLink />
      </Page>
    );
  }
}

export default SubscriptionPage;
