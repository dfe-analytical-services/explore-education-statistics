import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import {
  PublicationListSummary,
  PublicationSortOption,
} from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import useRouterLoading from '@frontend/hooks/useRouterLoading';
import FilterClearButton from '@frontend/modules/find-statistics/components/FilterClearButton';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import SearchForm from '@frontend/modules/find-statistics/components/SearchForm';
import SortControls from '@frontend/modules/find-statistics/components/SortControls';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import omit from 'lodash/omit';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import { ParsedUrlQuery } from 'querystring';
import React from 'react';

interface FindStatisticsPageQuery {
  page?: number;
  releaseType?: ReleaseType;
  search?: string;
  sortBy?: PublicationSortOption;
  themeId?: string;
}

interface Props {
  paging: Paging;
  publications: PublicationListSummary[];
  searchTerm?: string;
  sortBy?: PublicationSortOption;
}

const FindStatisticsPageNew: NextPage<Props> = ({
  paging,
  publications,
  searchTerm,
  sortBy = 'newest',
}) => {
  const { page, totalPages, totalResults } = paging;
  const router = useRouter();
  const isLoading = useRouterLoading();
  const { isMedia: isMobileMedia } = useMobileMedia();

  // TODO EES-3517 - include filtering by theme etc here;
  const isFiltered = !!searchTerm;

  const updateQueryParams = async (query: FindStatisticsPageQuery) => {
    await router.push(
      {
        pathname: '/find-statistics',
        query: query as ParsedUrlQuery,
      },
      undefined,
      {
        scroll: false,
      },
    );
  };

  const handleSortPublications = async (nextSortBy: PublicationSortOption) => {
    await updateQueryParams({
      ...omit(router.query, 'page'),
      sortBy: nextSortBy,
    });

    logEvent({
      category: 'Find statistics and data',
      action: 'Publications sorted',
      label: nextSortBy,
    });
  };

  const handleSearchPublications = async (nextSearchTerm: string) => {
    await updateQueryParams({
      ...omit(router.query, 'page'),
      search: nextSearchTerm,
      sortBy: 'relevance',
    });

    logEvent({
      category: 'Find statistics and data',
      action: 'Publications searched',
      label: nextSearchTerm,
    });
  };

  const clearSearch = async () => {
    await updateQueryParams(omit(router.query, 'search', 'page'));

    logEvent({
      category: 'Find statistics and data',
      action: 'Clear search filter',
    });
  };

  const clearAllFilters = async () => {
    await updateQueryParams(
      omit(router.query, 'page', 'releaseType', 'search', 'themeId'),
    );

    logEvent({
      category: 'Find statistics and data',
      action: 'Clear all filters',
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
          <SearchForm
            searchTerm={searchTerm}
            onSubmit={handleSearchPublications}
          />
          <a href="#searchResults" className="govuk-skip-link">
            Skip to search results
          </a>
        </div>
        <div className="govuk-grid-column-two-thirds">
          <div>
            <h2
              aria-live="polite"
              aria-atomic="true"
              className="govuk-!-margin-bottom-2"
            >
              {`${totalResults} ${totalResults !== 1 ? 'results' : 'result'}`}
            </h2>

            <div className="dfe-flex dfe-justify-content--space-between dfe-align-items-start">
              <p className="govuk-!-margin-bottom-2">
                {`${
                  totalResults ? `Page ${page} of ${totalPages}` : '0 pages'
                }, ${
                  isFiltered ? 'filtered by: ' : 'showing all publications'
                }`}
                {searchTerm && <VisuallyHidden>{searchTerm}</VisuallyHidden>}

                <VisuallyHidden>{`. Sorted by ${
                  sortBy === 'title' ? 'A to Z' : sortBy
                }`}</VisuallyHidden>
              </p>

              {isFiltered && (
                <ButtonText onClick={clearAllFilters}>
                  Clear all filters
                </ButtonText>
              )}
            </div>

            {isFiltered && (
              <div className="govuk-!-margin-bottom-5">
                {searchTerm && (
                  <FilterClearButton name={searchTerm} onClick={clearSearch} />
                )}
              </div>
            )}
          </div>

          <a href="#searchResults" className="govuk-skip-link">
            Skip to search results
          </a>

          {isMobileMedia && <Button>Filter results</Button>}

          {publications.length > 0 && (
            <SortControls
              hasSearch={!!searchTerm}
              sortBy={sortBy}
              onChange={handleSortPublications}
            />
          )}

          <LoadingSpinner loading={isLoading} className="govuk-!-margin-top-4">
            {publications.length === 0 ? (
              <div className="govuk-!-margin-top-5" id="searchResults">
                {isFiltered ? (
                  <>
                    <p className="govuk-!-font-weight-bold">
                      There are no matching results.
                    </p>
                    <p>Improve your search results by:</p>
                    <ul>
                      <li>removing filters</li>
                      <li>double-checking your spelling</li>
                      <li>using fewer keywords</li>
                      <li>searching for something less specific</li>
                    </ul>
                  </>
                ) : (
                  <p>No data currently published.</p>
                )}
              </div>
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
