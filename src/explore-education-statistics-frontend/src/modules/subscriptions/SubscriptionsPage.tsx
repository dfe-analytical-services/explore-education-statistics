import { NextContext } from 'next';
import React, { Component } from 'react';
import Details from 'src/components/Details';
import FormattedDate from 'src/components/FormattedDate';
import GoToTopLink from 'src/components/GoToTopLink';
import Link from 'src/components/Link';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import { baseUrl } from 'src/services/api';
import publicationService, { Release } from 'src/services/publicationService';

interface Props {
  publication: string;
  release: string;
  data: Release;
}

class SubscriptionsPage extends Component<Props> {
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
        {!release && (
          <strong className="govuk-tag govuk-!-margin-bottom-2">
            {' '}
            This is the latest data{' '}
          </strong>
        )}

        <PageTitle title={data.title} />

        <dl className="dfe-meta-content">
          <dt className="govuk-caption-m">Published: </dt>
          <dd>
            <strong>
              <FormattedDate>{data.published}</FormattedDate>
            </strong>
          </dd>
        </dl>

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h3 id="subsection-title">About these statistics</h3>

              <h4 data-testid="release-period">
                <span className="govuk-caption-m">For school year: </span>
                {data.releaseName} {!release && <span>(latest data)</span>}
                <Details summary={`See previous ${releaseCount} releases`}>
                  <ul
                    className="govuk-list"
                    data-testid="previous-releases-list"
                  >
                    {data.publication.releases
                      .slice(1)
                      .map(({ id, slug, releaseName }) => (
                        <li key={id} data-testid="item-internal">
                          <Link
                            to={`/statistics/${data.publication.slug}/${slug}`}
                          >
                            {releaseName}
                          </Link>
                        </li>
                      ))}
                    {data.publication.legacyReleases.map(
                      ({ id, description, url }) => (
                        <li key={id} data-testid="item-external">
                          <a href={url}>{description}</a>
                        </li>
                      ),
                    )}
                  </ul>
                </Details>
              </h4>
            </aside>
          </div>
        </div>

        <hr />

        <h2>Latest headline facts and figures - {data.releaseName}</h2>

        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Exploring the data
        </h2>

        <GoToTopLink />
      </Page>
    );
  }
}

export default SubscriptionsPage;
