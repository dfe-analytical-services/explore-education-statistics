import ButtonText from '@common/components/ButtonText';
import { FormGroup, FormSelect } from '@common/components/form';
import FormattedDate from '@common/components/FormattedDate';
import Pagination from '@common/components/Pagination';
import Tag from '@common/components/Tag';
import {
  PublicationReleaseSeriesItem,
  PublicationSummary,
} from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { formatPartialDate } from '@common/utils/date/partialDate';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import styles from '@frontend/modules/find-statistics/PublicationReleaseListPage.module.scss';
import publicationQueries from '@frontend/queries/publicationQueries';
import { Dictionary } from '@frontend/types';
import { QueryClient } from '@tanstack/react-query';
import { chunk } from 'lodash';
import { GetServerSideProps } from 'next';
import React, { useMemo, useState } from 'react';

interface Props {
  publicationSummary: PublicationSummary;
  allReleases: PaginatedList<PublicationReleaseSeriesItem>;
}

const MIN_PAGE_SIZE = 10;
const DEFAULT_PAGE_SIZE = 25;

const PublicationReleaseListPage = ({
  publicationSummary,
  allReleases,
}: Props) => {
  const { results } = allReleases ?? {};

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const paginatedReleasesPages = useMemo(
    () => chunk(results, pageSize),
    [results, pageSize],
  );

  const totalPages = paginatedReleasesPages.length;

  const handlePageSizeChange = (
    event: React.ChangeEvent<HTMLSelectElement>,
  ) => {
    const newPageSize = Number(event.target.value);
    setPageSize(newPageSize);
    setCurrentPage(1);
  };

  const handlePageChange = (pageNumber: number) => {
    setCurrentPage(pageNumber);
  };

  return (
    <Page
      title={publicationSummary.title}
      metaTitle={`Releases - ${publicationSummary.title} ${
        currentPage > 1 ? `- page ${currentPage}` : ''
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

          {paginatedReleasesPages.length > 0 ? (
            <div className="table-container">
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
                  {paginatedReleasesPages[currentPage - 1].map(release =>
                    'releaseId' in release ? (
                      <tr key={release.releaseId}>
                        <td>
                          <Link
                            to={`/find-statistics/${publicationSummary.slug}/${release.slug}`}
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
            </div>
          ) : (
            'No results found.'
          )}

          {results.length > MIN_PAGE_SIZE && (
            <FormGroup>
              <FormSelect
                className={styles.pageSizeSelect}
                id="pagination-size-select"
                label="Number of results per page"
                name="pagination-size"
                options={[
                  { value: '10', label: '10' },
                  { value: '25', label: '25' },
                  { value: '50', label: '50' },
                  { value: '100', label: '100' },
                ]}
                value={pageSize.toString()}
                order={[]}
                onChange={handlePageSizeChange}
              />
            </FormGroup>
          )}

          {totalPages > 1 && (
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              renderLink={({
                'aria-current': ariaCurrent,
                'aria-label': ariaLabel,
                'data-testid': testId,
                children,
                className,
                onClick,
              }) => (
                <ButtonText
                  ariaCurrent={ariaCurrent}
                  ariaLabel={ariaLabel}
                  className={`govuk-link ${className}`}
                  testId={testId}
                  onClick={onClick}
                >
                  {children}
                </ButtonText>
              )}
              onClick={handlePageChange}
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
  const { publication: publicationSlug } = query as Dictionary<string>;

  const queryClient = new QueryClient();

  try {
    const [publicationSummary, allReleases] = await Promise.all([
      queryClient.fetchQuery(
        publicationQueries.getPublicationSummary(publicationSlug),
      ),
      queryClient.fetchQuery(
        publicationQueries.getPublicationReleaseList(publicationSlug),
      ),
    ]);

    return {
      props: {
        publicationSummary,
        allReleases,
      },
    };
  } catch (error) {
    return {
      notFound: true,
    };
  }
};
