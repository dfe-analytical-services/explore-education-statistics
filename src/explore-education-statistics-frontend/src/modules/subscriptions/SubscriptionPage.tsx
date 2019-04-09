import { NextContext } from 'next';
import React, { Component } from 'react';
import FormattedDate from 'src/components/FormattedDate';
import GoToTopLink from 'src/components/GoToTopLink';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import { baseUrl } from 'src/services/api';
import publicationService, { Release } from 'src/services/publicationService';

interface Props {
  publication: string;
  release: string;
  data: Release;
}

class SubscriptionPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    publication: string;
    release: string;
  }>) {
    const { publication, release } = query;

    const request = release
      ? publicationService.getPublicationRelease(release)
      : publicationService.getLatestPublicationRelease(publication);

    const data = await request;

    return {
      data,
      publication,
      release,
    };
  }

  public render() {
    const { data, release } = this.props;

    const releaseCount =
      data.publication.releases.slice(1).length +
      data.publication.legacyReleases.length;

    return (
      <Page
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/statistics' },
          { name: data.title },
        ]}
      >
        <PageTitle title={data.title} />

        <dl className="dfe-meta-content">
          <dt className="govuk-caption-m">Last Published: </dt>
          <dd>
            <strong>
              <FormattedDate>{data.published}</FormattedDate>
            </strong>
          </dd>
        </dl>

        <form>
          <h3>Your details</h3>

          <div className="govuk-form-group">
            <label className="govuk-label" htmlFor="email">
              Email address
            </label>
            <input
              className="govuk-input"
              id="email"
              name="email"
              type="email"
              aria-describedby="email-hint"
            />
          </div>

          <button type="submit" className="govuk-button">
            Subscribe
          </button>
        </form>

        <hr />

        <GoToTopLink />
      </Page>
    );
  }
}

export default SubscriptionPage;
