import { NextContext } from 'next';
import React, { Component } from 'react';
import GoToTopLink from 'src/components/GoToTopLink';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import publicationService, { Release } from 'src/services/publicationService';
import {baseUrl} from "../../services/api";
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

  private handleFilterFormSubmit: SubscriptionFormSubmitHandler = async ({
                                                                           email,
                                                                         }) => {
    if (email !== '') {

      const data = {
        'email': email,
        'publicationId': this.state.publicationId,
      };

      const response = await fetch(`${baseUrl.function}/api/publication/subscribe/`, {
        body: JSON.stringify(data),
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
        },
        method: 'POST',
        mode: 'no-cors'
      });

      const json = await response.json();
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
          onSubmit={this.handleFilterFormSubmit}
        />

        <hr />

        <GoToTopLink />
      </Page>
    );
  }
}

export default SubscriptionPage;
