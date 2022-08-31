import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import useToggle from '@common/hooks/useToggle';
import {
  PublicationSummaryWithRelease,
  PublicationSortOptions,
} from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import useRouterLoading from '@frontend/hooks/useRouterLoading';
import styles from '@frontend/modules/find-statistics/FindStatisticsPage.module.scss';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import SortControls from '@frontend/modules/find-statistics/components/SortControls';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import classNames from 'classnames';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import React, { useEffect, useState } from 'react';

interface Props {
  paging: Paging;
  publications: PublicationSummaryWithRelease[];
  sortBy?: PublicationSortOptions;
}

const FindStatisticsPageNew: NextPage<Props> = ({
  paging,
  publications,
  sortBy,
}) => {
  const { page, totalPages, totalResults } = paging;
  const router = useRouter();
  const isRouterLoading = useRouterLoading();
  const { isMedia: isMobileMedia } = useMobileMedia();
  const [currentPage, setCurrentPage] = useState<number>(page);
  const [currentPublications, setCurrentPublications] = useState<
    PublicationSummaryWithRelease[]
  >(publications);
  const [currentSortBy, setCurrentSortBy] = useState<
    PublicationSortOptions | undefined
  >(sortBy ?? undefined);
  const [isLoadingPublications, toggleIsLoadingPublications] = useToggle(false);

  useEffect(() => {
    setCurrentPage(page);
    setCurrentPublications(publications);
  }, [page, publications]);

  const sortPublications = async (nextSortBy: PublicationSortOptions) => {
    toggleIsLoadingPublications.on();

    router.push(
      {
        pathname: '/find-statistics',
        query: {
          newDesign: true,
          sortBy: nextSortBy,
        },
      },
      undefined,
      { shallow: true },
    );

    // TODO EES-3517 - Fetch sorted publications here,
    // will need to take filters and search into account
    // const nextPublications = await publicationService.getPublications({
    //   sortBy: nextSortBy,
    // });
    // setCurrentPublications(nextPublications);

    setCurrentPage(1);
    setCurrentSortBy(nextSortBy);

    logEvent({
      category: 'Find statistics and data',
      action: 'Publications sorted',
      label: nextSortBy,
    });

    toggleIsLoadingPublications.off();
  };

  return (
    <>
      <Page
        metaTitle={
          totalPages > 1
            ? `Find statistics and data (page ${currentPage} of ${totalPages})`
            : undefined
        }
        title="Find statistics and data"
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <p className="govuk-body-l">
              Search and browse statistical summaries and download associated
              data to help you understand and analyse our range of statistics.
            </p>
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedInformation heading="Related information">
              <ul className="govuk-list">
                <li>
                  <a
                    href="https://www.gov.uk/government/organisations/ofsted/about/statistics"
                    rel="noopener noreferrer"
                    target="_blank"
                  >
                    Ofsted statistics
                  </a>
                </li>
                <li>
                  <a
                    href="https://www.education-ni.gov.uk/topics/statistics-and-research/statistics"
                    rel="noopener noreferrer"
                    target="_blank"
                  >
                    Educational statistics for Northern Ireland
                  </a>
                </li>
                <li>
                  <a
                    href="https://www.gov.scot/statistics-and-research/?cat=filter&amp;topics=Education"
                    rel="noopener noreferrer"
                    target="_blank"
                  >
                    Educational statistics for Scotland
                  </a>
                </li>
                <li>
                  <a
                    href="https://statswales.gov.wales/Catalogue/Education-and-Skills"
                    rel="noopener noreferrer"
                    target="_blank"
                  >
                    Educational statistics for Wales
                  </a>
                </li>
              </ul>
            </RelatedInformation>
          </div>
        </div>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-third">
            Search and filters here
          </div>
          <div className="govuk-grid-column-two-thirds">
            <div aria-live="polite" aria-atomic="true">
              <h2 className="govuk-!-margin-bottom-2">
                {`${totalResults} ${totalResults !== 1 ? 'results' : 'result'}`}
              </h2>

              <p className="govuk-!-margin-top-1">
                {`Page ${currentPage} of ${paging.totalPages}, showing all publications`}
                <VisuallyHidden>
                  {` Sorted by ${currentSortBy} publications`}
                </VisuallyHidden>
              </p>
            </div>

            <a href="#searchResults" className="govuk-skip-link ">
              Skip to search results
            </a>

            <div className={styles.sortControlsContainer}>
              {isMobileMedia && (
                <Button
                  className={classNames(
                    styles.mobileFilterButton,
                    'govuk-!-margin-bottom-0',
                  )}
                  variant="secondary"
                >
                  Filter results
                </Button>
              )}
              <SortControls
                initialValues={{ sortBy: currentSortBy ?? 'newest' }}
                onChange={sortPublications}
              />
            </div>

            <LoadingSpinner
              loading={isLoadingPublications || isRouterLoading}
              className="govuk-!-margin-top-4"
            >
              {/* TODO EES-3517 show different message if search / filter returns no results  */}
              {currentPublications.length === 0 ? (
                <div className="govuk-inset-text" id="searchResults">
                  No data currently published.
                </div>
              ) : (
                <ul className="govuk-list" id="searchResults">
                  {currentPublications.map(publication => (
                    <PublicationSummary
                      key={publication.id}
                      publication={publication}
                    />
                  ))}
                </ul>
              )}

              <Pagination
                currentPage={currentPage ?? 1}
                totalPages={totalPages}
              />
            </LoadingSpinner>
          </div>
        </div>
      </Page>
    </>
  );
};

export default FindStatisticsPageNew;
