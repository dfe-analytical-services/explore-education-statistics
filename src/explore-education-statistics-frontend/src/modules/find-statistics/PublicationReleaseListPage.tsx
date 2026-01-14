import FormattedDate from '@common/components/FormattedDate';
import Tag from '@common/components/Tag';
import {
  PublicationReleaseSeriesItem,
  PublicationSummaryRedesign,
} from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { formatPartialDate } from '@common/utils/date/partialDate';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import publicationQueries from '@frontend/queries/publicationQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { Dictionary } from '@frontend/types';
import { QueryClient } from '@tanstack/react-query';
import { GetServerSideProps } from 'next';
import React from 'react';

interface Props {
  publicationSummary: PublicationSummaryRedesign;
  releases: PaginatedList<PublicationReleaseSeriesItem>;
}

const PublicationReleaseListPage = ({
  publicationSummary,
  releases,
}: Props) => {
  const { paging, results } = releases ?? {};
  const { page, totalPages } = paging ?? {};

  return (
    <Page
      title={publicationSummary.title}
      metaTitle={`Releases - ${publicationSummary.title} ${
        page ? `- page ${page}` : ''
      }`}
      description={`Releases in ${publicationSummary.title.toLocaleLowerCase()}.`}
      caption="Releases in this series"
      captionInsideTitle
      width="wide"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: publicationSummary.title,
          link: `/find-statistics/${publicationSummary.slug}/${publicationSummary.latestRelease.slug}`,
        },
      ]}
      breadcrumbLabel="Releases"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          {publicationSummary.nextReleaseDate && (
            <div
              className="govuk-!-margin-bottom-2"
              data-testid="next-release-date"
            >
              <h3 className="govuk-heading-m govuk-!-margin-bottom-2">
                Next release date
              </h3>
              <time>
                {formatPartialDate(publicationSummary.nextReleaseDate)}
              </time>
            </div>
          )}
          <Link
            to={`/find-statistics/${publicationSummary.slug}/${publicationSummary.latestRelease.slug}`}
            className="govuk-!-margin-bottom-8 govuk-!-display-inline-block"
          >
            Release home (latest release)
          </Link>
          <table data-testid="release-updates-table">
            <caption className="govuk-table__caption--m">
              Table showing all published releases in this series
            </caption>

            <thead>
              <tr>
                <th scope="col">Release period</th>
                <th scope="col">Published date</th>
                <th scope="col">Last update</th>
              </tr>
            </thead>
            <tbody>
              {results.map(release =>
                'releaseId' in release ? (
                  <tr key={release.releaseId}>
                    <td>
                      <Link
                        to={`/find-statistics/${publicationSummary.slug}/${release.slug}?redesign=true`} // TODO EES-6449 remove query param when live
                      >
                        {release.title}
                      </Link>
                    </td>
                    <td>
                      <div className="dfe-flex dfe-flex-wrap dfe-gap-2">
                        <FormattedDate>{release.published}</FormattedDate>
                        {release.isLatestRelease === true && (
                          <Tag>Latest release</Tag>
                        )}
                      </div>
                    </td>
                    <td>
                      <FormattedDate>{release.lastUpdated}</FormattedDate>
                    </td>
                  </tr>
                ) : (
                  <tr key={release.title}>
                    <td>
                      <Link to={release.url}>{release.title}</Link>
                    </td>
                    <td>Not available</td>
                    <td>Not available</td>
                  </tr>
                ),
              )}
            </tbody>
          </table>
          {page && !!totalPages && (
            <Pagination
              currentPage={page}
              totalPages={totalPages}
              onClick={pageNumber => {
                logEvent({
                  category: 'Publication releases',
                  action: `Pagination clicked`,
                  label: `Page ${pageNumber}`,
                });
              }}
            />
          )}
        </div>
      </div>
    </Page>
  );
};

export default PublicationReleaseListPage;

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const {
    publication: publicationSlug,
    page,
    pageSize,
  } = query as Dictionary<string>;

  const queryClient = new QueryClient();

  try {
    const [publicationSummary, releases] = await Promise.all([
      queryClient.fetchQuery(
        publicationQueries.getPublicationSummaryRedesign(publicationSlug),
      ),
      queryClient.fetchQuery(
        publicationQueries.getPublicationReleaseList(publicationSlug, {
          page: page ? Number(page) : undefined,
          pageSize: pageSize ? Number(pageSize) : undefined,
        }),
      ),
    ]);

    return {
      props: {
        publicationSummary,
        releases,
      },
    };
  } catch (error) {
    return {
      notFound: true,
    };
  }
};
