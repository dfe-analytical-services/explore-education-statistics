import Button from '@common/components/Button';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import {
  PublicationSummaryWithRelease,
  PublicationSortOption,
} from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import useRouterLoading from '@frontend/hooks/useRouterLoading';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import SortControls from '@frontend/modules/find-statistics/components/SortControls';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import React from 'react';

interface Props {
  paging: Paging;
  publications: PublicationSummaryWithRelease[];
  sortBy?: PublicationSortOption;
}

const FindStatisticsPageNew: NextPage<Props> = ({
  paging,
  publications,
  sortBy = 'newest',
}) => {
  const { page = 1, totalPages, totalResults } = paging;

  const router = useRouter();
  const isLoading = useRouterLoading();
  const { isMedia: isMobileMedia } = useMobileMedia();

  const handleSortPublications = async (nextSortBy: PublicationSortOption) => {
    await router.push(
      {
        pathname: '/find-statistics',
        query: {
          ...router.query,
          sortBy: nextSortBy,
        },
      },
      undefined,
      {
        scroll: false,
      },
    );

    logEvent({
      category: 'Find statistics and data',
      action: 'Publications sorted',
      label: nextSortBy,
    });
  };

  return (
    <Page
      title="Find statistics and data"
      metaTitle={
        totalPages > 1
          ? `Find statistics and data (page ${page} of ${totalPages})`
          : undefined
      }
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Search and browse statistical summaries and download associated data
            to help you understand and analyse our range of statistics.
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
              {`Page ${page} of ${paging.totalPages}, showing all publications`}
              <VisuallyHidden>{` sorted by ${sortBy}`}</VisuallyHidden>
            </p>
          </div>

          <a href="#searchResults" className="govuk-skip-link">
            Skip to search results
          </a>

          {isMobileMedia && <Button>Filter results</Button>}

          <SortControls sortBy={sortBy} onChange={handleSortPublications} />

          <LoadingSpinner loading={isLoading} className="govuk-!-margin-top-4">
            {/* TODO EES-3517 show different message if search / filter returns no results  */}
            {publications.length === 0 ? (
              <InsetText id="searchResults">
                No data currently published.
              </InsetText>
            ) : (
              <ul className="govuk-list" id="searchResults">
                {publications.map(publication => (
                  <PublicationSummary
                    key={publication.id}
                    publication={publication}
                  />
                ))}
              </ul>
            )}

            <Pagination currentPage={page} totalPages={totalPages} />
          </LoadingSpinner>
        </div>
      </div>
    </Page>
  );
};

export default FindStatisticsPageNew;
