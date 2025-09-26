import FormattedDate from '@common/components/FormattedDate';
import publicationService, {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import releaseUpdatesService, {
  ReleaseUpdate,
} from '@common/services/releaseUpdatesService';
import { PaginatedList } from '@common/services/types/pagination';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import styles from '@frontend/modules/find-statistics/ReleaseUpdatesPage.module.scss';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { Dictionary } from '@frontend/types';
import { GetServerSideProps } from 'next';
import React from 'react';

interface Props {
  publicationSummary: PublicationSummaryRedesign;
  releaseUpdates: PaginatedList<ReleaseUpdate>;
  releaseVersionSummary: ReleaseVersionSummary;
}

const ReleaseUpdatesPage = ({
  publicationSummary,
  releaseUpdates,
  releaseVersionSummary,
}: Props) => {
  const { paging, results } = releaseUpdates ?? {};
  const { page, totalPages } = paging ?? {};

  return (
    <Page
      title={publicationSummary.title}
      metaTitle={`Updates - ${publicationSummary.title} - ${
        releaseVersionSummary.title
      } - ${page ? `- page ${page}` : ''}`}
      description={`Updates to ${publicationSummary.title.toLocaleLowerCase()} ${releaseVersionSummary.title.toLocaleLowerCase()}.`}
      caption={`Updates to ${releaseVersionSummary.title}`}
      captionInsideTitle
      width="wide"
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: publicationSummary.title,
          link: `/find-statistics/${publicationSummary.slug}/${releaseVersionSummary.slug}`,
        },
      ]}
      breadcrumbLabel="Updates"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <Link
            to={`/find-statistics/${publicationSummary.slug}/${releaseVersionSummary.slug}`}
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

  // TODO EES-6449 - remove
  if (process.env.APP_ENV === 'Production') {
    return {
      notFound: true,
    };
  }

  const publicationSummary =
    await publicationService.getPublicationSummaryRedesign(publicationSlug);

  const [releaseVersionSummary, releaseUpdates] = await Promise.all([
    publicationService.getReleaseVersionSummary(publicationSlug, releaseSlug),
    releaseUpdatesService.getReleaseUpdates(publicationSlug, releaseSlug, {
      page: page ? Number(page) : undefined,
      pageSize: pageSize ? Number(pageSize) : undefined,
    }),
  ]);

  return {
    props: {
      publicationSummary,
      releaseVersionSummary,
      releaseUpdates,
    },
  };
};
