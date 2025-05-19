import React, { useState } from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import { useQuery } from '@tanstack/react-query';
import compact from 'lodash/compact';
import omit from 'lodash/omit';
import { ParsedUrlQuery } from 'querystring';
import GoToTopLink from '@common/components/GoToTopLink';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import FilterResetButtonAzureSearch from '@frontend/components/FilterResetButtonAzureSearch';
import FiltersMobile from '@frontend/components/FiltersMobile';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import SearchForm from '@frontend/components/SearchForm';
import { SortOption } from '@frontend/components/SortControls';
import FiltersAzureSearch from '@frontend/modules/find-statistics/components/FiltersAzureSearch';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import { getParamsFromQuery } from '@frontend/modules/find-statistics/utils/createAzurePublicationListRequest';
import {
  PublicationFilter,
  publicationFilters,
} from '@frontend/modules/find-statistics/utils/publicationFilters';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import publicationQueries from '@frontend/queries/azurePublicationQueries';
import themeQueries from '@frontend/queries/themeQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { FindStatisticsPageQuery } from './FindStatisticsPage';

const defaultPageTitle = 'Find statistics and data';

const FindStatisticsPage: NextPage = () => {
  const router = useRouter();
  const { isMedia: isMobileMedia } = useMobileMedia();

  const [pageTitle, setPageTitle] = useState<string>(defaultPageTitle);

  const {
    data: publicationsData,
    isError,
    isFetching,
    isLoading,
  } = useQuery({
    ...publicationQueries.listAzure(router.query),
    keepPreviousData: true,
    staleTime: 60000,
  });

  const { data: themes = [] } = useQuery({
    ...themeQueries.list(),
    staleTime: Infinity,
  });

  const {
    paging,
    results: publications = [],
    facets = { themeId: [], releaseType: [] },
  } = publicationsData ?? {};

  const { page, totalPages, totalResults = 0 } = paging ?? {};

  const { themeId: themeFacetResults, releaseType: releaseTypeFacetResults } =
    facets;

  const themesWithResultCounts = themes
    .map(theme => {
      const facetedResult = themeFacetResults.find(
        result => theme.id === result.value,
      );
      const count = facetedResult?.count ?? 0;
      return {
        label: `${theme.title} (${count})`,
        value: theme.id,
        count,
      };
    })
    .sort((a, b) => b.count - a.count);

  const releaseTypesWithResultCounts = Object.keys(releaseTypes)
    .map(type => {
      const facetedResult = releaseTypeFacetResults.find(
        result => type === result.value,
      );

      const title = releaseTypes[type as ReleaseType];
      const count = facetedResult?.count ?? 0;
      return {
        label: `${title} (${count})`,
        value: type,
        count,
      };
    })
    .sort((a, b) => b.count - a.count);

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

  const sortOptions: SortOption[] = [
    { label: 'Newest', value: 'newest' },
    { label: 'Oldest', value: 'oldest' },
    { label: 'A to Z', value: 'title' },
    ...(search
      ? [
          {
            label: 'Relevance',
            value: 'relevance',
          } as SortOption,
        ]
      : []),
  ];

  const updateQueryParams = async (nextQuery: FindStatisticsPageQuery) => {
    // When a query param is changed for the first time Next announces the page title,
    // even if it hasn't changed.
    // Set the title here so it's more useful the just announcing the default title.
    // Have to set it before the route change otherwise the previous title will be announced.
    // It won't be reannounced on subsequent query param changes.
    setPageTitle(`${defaultPageTitle} - Search results`);
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
            sortBy,
          };

    await updateQueryParams(newParams);

    logEvent({
      category: 'Find statistics and data',
      action: `Publications filtered by ${filterType}`,
      label: nextValue,
    });
  };

  const handleResetFilter = async ({
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
      action: `Reset ${filterType} filter`,
    });
  };

  const totalResultsMessage = `${totalResults} ${
    totalResults !== 1 ? 'results' : 'result'
  }`;

  return (
    <Page
      title={defaultPageTitle}
      // Don't include the default meta title when filtered to prevent too much screen reader noise.
      includeDefaultMetaTitle={pageTitle === defaultPageTitle}
      metaTitle={pageTitle}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-full">
          <p className="govuk-body-l">
            Search and browse statistical summaries and download associated data
            to help you understand and analyse our range of statistics.
          </p>
          <SearchForm
            label="Search publications"
            value={search}
            onSubmit={nextValue =>
              handleChangeFilter({ filterType: 'search', nextValue })
            }
          />
          <a href="#searchResults" className="govuk-skip-link">
            Skip to search results
          </a>

          <div className="govuk-!-margin-top-3 dfe-flex dfe-flex-wrap dfe-gap-2 dfe-align-items--center">
            <p className="govuk-!-margin-bottom-0">
              {`${totalResultsMessage}, ${
                totalResults ? `page ${page} of ${totalPages}` : '0 pages'
              }, ${isFiltered ? 'filtered by: ' : 'showing all publications'}`}
            </p>

            {isFiltered && <VisuallyHidden>{filteredByString}</VisuallyHidden>}

            <div className="dfe-flex dfe-flex-wrap dfe-gap-2 dfe-align-items--center">
              {search && (
                <FilterResetButtonAzureSearch
                  filterType="Search"
                  name={search}
                  onClick={() => handleResetFilter({ filterType: 'search' })}
                />
              )}

              {selectedTheme && (
                <FilterResetButtonAzureSearch
                  filterType="Theme"
                  name={selectedTheme.title}
                  onClick={() => handleResetFilter({ filterType: 'themeId' })}
                />
              )}
              {selectedReleaseType && (
                <FilterResetButtonAzureSearch
                  filterType="Release type"
                  name={selectedReleaseType}
                  onClick={() =>
                    handleResetFilter({ filterType: 'releaseType' })
                  }
                />
              )}
            </div>

            <VisuallyHidden>{` Sorted by ${
              sortBy === 'title' ? 'A to Z' : sortBy
            }`}</VisuallyHidden>
          </div>

          {isMobileMedia && isFiltered && (
            <ButtonText
              onClick={() => handleResetFilter({ filterType: 'all' })}
              className="govuk-!-margin-top-2"
            >
              Reset filters
            </ButtonText>
          )}
          {/* Using ScreenReaderMessage here as the message is announced twice if have
          aria-live etc on the h2. */}
          <ScreenReaderMessage message={totalResultsMessage} />
        </div>
      </div>
      <hr />
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third">
          {!isMobileMedia && (
            <FiltersAzureSearch
              releaseType={releaseType}
              releaseTypesWithResultCounts={releaseTypesWithResultCounts}
              showResetFiltersButton={!isMobileMedia && isFiltered}
              sortBy={sortBy}
              sortOptions={sortOptions}
              themeId={themeId}
              themes={themes}
              themesWithResultCounts={themesWithResultCounts}
              onChange={handleChangeFilter}
              onSortChange={handleSortBy}
              onResetFilters={() => handleResetFilter({ filterType: 'all' })}
            />
          )}
        </div>
        <div className="govuk-grid-column-two-thirds">
          {isMobileMedia && (
            <FiltersMobile
              title="Sort and filter publications"
              totalResults={totalResults}
            >
              <FiltersAzureSearch
                releaseTypesWithResultCounts={releaseTypesWithResultCounts}
                releaseType={releaseType}
                sortBy={sortBy}
                sortOptions={sortOptions}
                themeId={themeId}
                themes={themes}
                themesWithResultCounts={themesWithResultCounts}
                onChange={handleChangeFilter}
                onSortChange={handleSortBy}
              />
            </FiltersMobile>
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
                {totalResults === 0 ? (
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
                    {publications.map(pub => (
                      <PublicationSummary key={pub.id} publication={pub} />
                    ))}
                  </ul>
                )}
              </>
            )}
            {page && totalPages && (
              <Pagination
                currentPage={page}
                shallow
                totalPages={totalPages}
                onClick={pageNumber => {
                  // Make sure the page title is updated before the route change,
                  // otherwise the wrong page number is announced.
                  setPageTitle(`${defaultPageTitle} - page ${pageNumber}`);
                  logEvent({
                    category: 'Find statistics and data',
                    action: `Pagination clicked`,
                    label: `Page ${pageNumber}`,
                  });
                }}
              />
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

export default FindStatisticsPage;
