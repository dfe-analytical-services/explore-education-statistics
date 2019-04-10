import { NextContext } from 'next';
import React, { Component } from 'react';
import GoToTopLink from 'src/components/GoToTopLink';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import functionsService, { SubscriptionData } from 'src/services/functionsService';
import publicationService, { Release } from 'src/services/publicationService';
import SubscriptionForm, { SubscriptionFormSubmitHandler } from './components/SubscriptionForm';

interface Props {
  publication: string;
  data: Release;
}

interface State {
  publicationId: string;
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

  public state: State = {
    publicationId: this.props.publication,
  };

  private handleFormSubmit: SubscriptionFormSubmitHandler = async ({ email }) => {
    if (email !== '') {
      const publicationId = this.state.publicationId;
      const { result } = await functionsService.subscribe(
        {
          email,
          publicationId,
        },
      );
    }
  };

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

        <SubscriptionForm
          onSubmit={this.handleFormSubmit}
        />

        <hr />

        <GoToTopLink />
      </Page>
    );
  }
}

export default SubscriptionPage;
