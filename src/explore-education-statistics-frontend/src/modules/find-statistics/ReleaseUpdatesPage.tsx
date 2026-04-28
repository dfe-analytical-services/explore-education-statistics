import FormattedDate from '@common/components/FormattedDate';
import { ReleaseVersionSummary } from '@common/services/publicationService';
import { ReleaseUpdate } from '@common/services/releaseUpdatesService';
import { PaginatedList } from '@common/services/types/pagination';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import styles from '@frontend/modules/find-statistics/ReleaseUpdatesPage.module.scss';
import publicationQueries from '@frontend/queries/publicationQueries';
import releaseUpdatesQueries from '@frontend/queries/releaseUpdatesQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { Dictionary } from '@frontend/types';
import { QueryClient } from '@tanstack/react-query';
import { GetServerSideProps } from 'next';
import React from 'react';

interface Props {
  releaseUpdates: PaginatedList<ReleaseUpdate>;
  releaseVersionSummary: ReleaseVersionSummary;
}

const ReleaseUpdatesPage = ({
  releaseUpdates,
  releaseVersionSummary,
}: Props) => {
  const { publication } = releaseVersionSummary;
  const { paging, results } = releaseUpdates ?? {};
  const { page, totalPages } = paging ?? {};

  return (
    <Page
      title={publication.title}
      metaTitle={`Updates - ${publication.title} - ${
        releaseVersionSummary.title
      } - ${page ? `- page ${page}` : ''}`}
      description={`Updates to ${publication.title.toLocaleLowerCase()} ${releaseVersionSummary.title.toLocaleLowerCase()}.`}
      caption={`Updates to ${releaseVersionSummary.title}`}
      captionInsideTitle
      width="wide"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: publication.title,
          link: `/find-statistics/${publication.slug}/${releaseVersionSummary.slug}`,
        },
      ]}
      breadcrumbLabel="Updates"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <Link
            to={`/find-statistics/${publication.slug}/${releaseVersionSummary.slug}`}
            className="govuk-!-margin-bottom-6 govuk-!-display-inline-block"
          >
            Release home
          </Link>

          <table data-testid="release-updates-table">
            <caption className="govuk-table__caption--m">
              Table showing updates to this release
            </caption>

            <thead>
              <tr>
                <th className={styles.dateColumn} scope="col">
                  Date updated
                </th>
                <th scope="col">Summary of update</th>
              </tr>
            </thead>

            <tbody>
              {results.map(update => (
                <tr key={update.date}>
                  <td>
                    <FormattedDate>{update.date}</FormattedDate>
                  </td>
                  <td>{update.summary}</td>
                </tr>
              ))}
            </tbody>
          </table>

          {page && !!totalPages && (
            <Pagination
              currentPage={page}
              totalPages={totalPages}
              onClick={pageNumber => {
                logEvent({
                  category: 'Publication updates',
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

export default ReleaseUpdatesPage;

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const {
    publication: publicationSlug,
    release: releaseSlug,
    page,
    pageSize,
  } = query as Dictionary<string>;

  const queryClient = new QueryClient();

  try {
    const [releaseVersionSummary, releaseUpdates] = await Promise.all([
      queryClient.fetchQuery(
        publicationQueries.getReleaseVersionSummary(
          publicationSlug,
          releaseSlug,
        ),
      ),
      queryClient.fetchQuery(
        releaseUpdatesQueries.getReleaseUpdates(publicationSlug, releaseSlug, {
          page: page ? Number(page) : undefined,
          pageSize: pageSize ? Number(pageSize) : undefined,
        }),
      ),
    ]);

    return {
      props: {
        releaseVersionSummary,
        releaseUpdates,
      },
    };
  } catch (error) {
    return {
      notFound: true,
    };
  }
};
