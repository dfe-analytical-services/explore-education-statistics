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
      metaTitle={`${publicationSummary.title} - ${
        releaseVersionSummary.title
      } - updates ${page ? `- page ${page}` : ''}`}
      description={`Table showing updates to ${publicationSummary.title.toLocaleLowerCase()} ${releaseVersionSummary.title.toLocaleLowerCase()}.`}
      caption={releaseVersionSummary.title}
      backLinkDestination={`/find-statistics/${publicationSummary.slug}/${releaseVersionSummary.slug}?redesign=true`} // TODO EES-6449 remove redesign query param
      width="wide"
    >
      <Link
        to={`/find-statistics/${publicationSummary.slug}/releases`}
        className="govuk-!-margin-bottom-6 govuk-!-display-inline-block"
      >
        All releases in this series
      </Link>

      <table className="govuk-table" data-testid="release-updates-table">
        <caption className="govuk-table__caption--m">
          Table showing updates to this release
        </caption>

        <thead className="govuk-table__head">
          <tr className="govuk-table__row">
            <th scope="col" className="govuk-table__header">
              Date updated
            </th>
            <th scope="col" className="govuk-table__header">
              Summary of update
            </th>
          </tr>
        </thead>

        <tbody className="govuk-table__body">
          {results.map(update => (
            <tr className="govuk-table__row" key={update.date}>
              <td className="govuk-table__cell">
                <FormattedDate>{update.date}</FormattedDate>
              </td>
              <td className="govuk-table__cell">{update.summary}</td>
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

  if (process.env.APP_ENV === 'Production') {
    return {
      redirect: {
        destination: `/find-statistics/${publicationSlug}/${releaseSlug}`,
        permanent: true,
      },
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
