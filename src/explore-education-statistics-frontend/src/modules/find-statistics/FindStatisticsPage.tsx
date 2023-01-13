import GoToTopLink from '@common/components/GoToTopLink';
import {
  publicationFilters,
  PublicationFilter,
  PublicationSortOption,
} from '@common/services/publicationService';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import publicationQueries from '@frontend/queries/publicationQueries';
import themeQueries from '@frontend/queries/themeQueries';
import { GetServerSideProps, NextPage } from 'next';
import React, { useRef } from 'react';
import { dehydrate, QueryClient, useQuery } from '@tanstack/react-query';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import FilterClearButton from '@frontend/modules/find-statistics/components/FilterClearButton';
import Filters from '@frontend/modules/find-statistics/components/Filters';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import SearchForm from '@frontend/modules/find-statistics/components/SearchForm';
import SortControls from '@frontend/modules/find-statistics/components/SortControls';
import { getParamsFromQuery } from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import compact from 'lodash/compact';
import omit from 'lodash/omit';
import { useRouter } from 'next/router';
import { ParsedUrlQuery } from 'querystring';

export interface FindStatisticsPageQuery {
  page?: number;
  releaseType?: ReleaseType;
  search?: string;
  sortBy?: PublicationSortOption;
  themeId?: string;
}

const FindStatisticsPage: NextPage = () => {
  const router = useRouter();
  const { isMedia: isMobileMedia } = useMobileMedia();
  const mobileFilterButtonRef = useRef<HTMLButtonElement>(null);
  const [showMobileFilters, toggleMobileFilters] = useToggle(false);

  const { data: publicationsData, isError, isFetching, isLoading } = useQuery({
    ...publicationQueries.list(router.query),
    keepPreviousData: true,
    staleTime: 60000,
  });

  const { data: themes = [] } = useQuery({
    ...themeQueries.list(),
    staleTime: Infinity,
  });

  const { paging, results: publications = [] } = publicationsData ?? {};
  const { page, totalPages, totalResults = 0 } = paging ?? {};

  const { releaseType, search, sortBy, themeId } = getParamsFromQuery(
    router.query,
  );

  const isFiltered = !!search || !!releaseType || !!themeId;

  const selectedTheme = themes.find(theme => theme.id === themeId);
  const selectedReleaseType = releaseTypes[releaseType as ReleaseType];

  const filteredByString = compact([
    search,
    selectedTheme?.title,
    selectedReleaseType,
  ]).join(', ');

  const updateQueryParams = async (nextQuery: FindStatisticsPageQuery) => {
    await router.push(
      {
        pathname: '/find-statistics',
        query: nextQuery as ParsedUrlQuery,
      },
      undefined,
      {
        shallow: true,
      },
    );
  };

  const handleSortBy = async (nextSortBy: PublicationSortOption) => {
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

  const handleChangeFilter = async ({
    filterType,
    nextValue,
  }: {
    filterType: PublicationFilter;
    nextValue: string;
  }) => {
    const newParams =
      nextValue === 'all'
        ? omit(router.query, 'page', filterType)
        : {
            ...omit(router.query, 'page'),
            [filterType]: nextValue,
            sortBy: filterType === 'search' ? 'relevance' : sortBy,
          };

    await updateQueryParams(newParams);

    logEvent({
      category: 'Find statistics and data',
      action: `Publications filtered by ${filterType}`,
      label: nextValue,
    });
  };

  const handleClearFilter = async ({
    filterType,
  }: {
    filterType: PublicationFilter | 'all';
  }) => {
    // Reset sortBy to newest if removing a search and sorting by relevance
    const nextSortBy = search && sortBy === 'relevance' ? 'newest' : sortBy;

    const newParams =
      filterType === 'all'
        ? {
            ...omit(router.query, 'page', ...publicationFilters),
            sortBy: nextSortBy,
          }
        : {
            ...omit(router.query, filterType, 'page'),
            sortBy: nextSortBy,
          };
    await updateQueryParams(newParams);

    logEvent({
      category: 'Find statistics and data',
      action: `Clear ${filterType} filter`,
    });
  };

  return (
    <Page
      title="Find statistics and data"
      metaTitle={
        totalPages && totalPages > 1
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
                <Link to="/data-catalogue">
                  Education statistics: data catalogue
                </Link>
              </li>
              <li>
                <Link to="/methodology">Education statistics: methodology</Link>
              </li>
              <li>
                <Link to="/glossary">Education statistics: glossary</Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third">
          <SearchForm
            searchTerm={search}
            onSubmit={nextValue =>
              handleChangeFilter({ filterType: 'search', nextValue })
            }
          />
          <a href="#searchResults" className="govuk-skip-link">
            Skip to search results
          </a>
          <Filters
            releaseType={releaseType}
            showMobileFilters={showMobileFilters}
            themeId={themeId}
            themes={themes}
            totalResults={totalResults}
            onChange={handleChangeFilter}
            onCloseMobileFilters={() => {
              mobileFilterButtonRef.current?.focus();
              toggleMobileFilters.off();
            }}
          />
        </div>
        <div className="govuk-grid-column-two-thirds">
          <div>
            <h2
              aria-live="polite"
              aria-atomic="true"
              className="govuk-!-margin-bottom-2"
              data-testid="total-results"
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

                {isFiltered && (
                  <VisuallyHidden>{filteredByString}</VisuallyHidden>
                )}

                <VisuallyHidden>{` Sorted by ${
                  sortBy === 'title' ? 'A to Z' : sortBy
                }`}</VisuallyHidden>
              </p>

              {isFiltered && (
                <ButtonText
                  onClick={() => handleClearFilter({ filterType: 'all' })}
                >
                  Clear all filters
                </ButtonText>
              )}
            </div>

            {isFiltered && (
              <div className="govuk-!-margin-bottom-5">
                {search && (
                  <FilterClearButton
                    name={search}
                    onClick={() => handleClearFilter({ filterType: 'search' })}
                  />
                )}
                {selectedTheme && (
                  <FilterClearButton
                    name={selectedTheme.title}
                    onClick={() => handleClearFilter({ filterType: 'themeId' })}
                  />
                )}
                {selectedReleaseType && (
                  <FilterClearButton
                    name={selectedReleaseType}
                    onClick={() =>
                      handleClearFilter({ filterType: 'releaseType' })
                    }
                  />
                )}
              </div>
            )}
          </div>

          {isMobileMedia && (
            <Button
              ref={mobileFilterButtonRef}
              onClick={toggleMobileFilters.on}
            >
              Filter results
            </Button>
          )}

          {publications.length > 0 && (
            <SortControls
              hasSearch={!!search}
              sortBy={sortBy}
              onChange={handleSortBy}
            />
          )}

          <LoadingSpinner
            loading={isLoading || isFetching}
            className="govuk-!-margin-top-4"
          >
            {isError ? (
              <WarningMessage>
                Cannot load publications, please try again later.
              </WarningMessage>
            ) : (
              <>
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
                  <ul
                    className="govuk-list"
                    id="searchResults"
                    data-testid="publicationsList"
                  >
                    {publications.map(publication => (
                      <PublicationSummary
                        key={publication.id}
                        publication={publication}
                      />
                    ))}
                  </ul>
                )}
              </>
            )}
            {page && totalPages && (
              <Pagination currentPage={page} shallow totalPages={totalPages} />
            )}
          </LoadingSpinner>

          {publications.length > 0 && (
            <GoToTopLink className="govuk-!-margin-top-7" />
          )}
        </div>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps = async ({ query }) => {
  const queryClient = new QueryClient();

  await Promise.all([
    queryClient.prefetchQuery(publicationQueries.list(query)),
    queryClient.prefetchQuery(themeQueries.list()),
  ]);

  return {
    props: {
      dehydratedState: dehydrate(queryClient),
    },
  };
};

export default FindStatisticsPage;
