/* eslint-disable no-restricted-syntax */
import React, { useState } from 'react';
import { GetServerSideProps, NextPage } from 'next';
import { useRouter } from 'next/router';
import { dehydrate, QueryClient, useQuery } from '@tanstack/react-query';
import compact from 'lodash/compact';
import omit from 'lodash/omit';
import { ParsedUrlQuery } from 'querystring';
import GoToTopLink from '@common/components/GoToTopLink';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import FilterResetButtonAzureSearch from '@frontend/components/FilterResetButtonAzureSearch';
import FilterResetButton from '@frontend/components/FilterResetButton';
import FiltersDesktop from '@frontend/components/FiltersDesktop';
import FiltersMobile from '@frontend/components/FiltersMobile';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import SearchForm from '@frontend/components/SearchForm';
import SortControls, { SortOption } from '@frontend/components/SortControls';
import Filters from '@frontend/modules/find-statistics/components/Filters';
import FiltersAzureSearch from '@frontend/modules/find-statistics/components/FiltersAzureSearch';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import { getParamsFromQuery } from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import {
  PublicationFilter,
  publicationFilters,
} from '@frontend/modules/find-statistics/utils/publicationFilters';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import publicationQueries from '@frontend/queries/publicationQueries';
import themeQueries from '@frontend/queries/themeQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';

const defaultPageTitle = 'Find statistics and data';

export interface FindStatisticsPageQuery {
  page?: number;
  releaseType?: ReleaseType;
  search?: string;
  sortBy?: PublicationSortOption;
  themeId?: string;
}

const FindStatisticsPage: NextPage = () => {
  const router = useRouter();
  const useAzureSearch = !!router.query.azsearch;
  const { isMedia: isMobileMedia } = useMobileMedia();

  const [pageTitle, setPageTitle] = useState<string>(defaultPageTitle);

  const {
    data: publicationsData,
    isError,
    isFetching,
    isLoading,
  } = useQuery({
    ...publicationQueries.list(router.query),
    keepPreviousData: true,
    staleTime: 60000,
  });

  const {
    data: publicationsDataAzure,
    isError: isErrorAzure,
    isFetching: isFetchingAzure,
    isLoading: isLoadingAzure,
  } = useQuery({
    ...publicationQueries.listAzure(router.query),
    keepPreviousData: true,
    staleTime: 60000,
  });

  const { data: themes = [] } = useQuery({
    ...themeQueries.list(),
    staleTime: Infinity,
  });

  const { paging, results: publications = [] } = publicationsData ?? {};
  const { page, totalPages, totalResults = 0 } = paging ?? {};

  const totalResultsAzure = publicationsDataAzure?.count || 0;

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

  console.log({ publicationsDataAzure });

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

    // TODO
    if (publicationsDataAzure?.results) {
      for await (const result of publicationsDataAzure.results) {
        const name = result.document.title;
        console.log(name);
      }
    }
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

  const totalResultsMessageAzure = `${totalResultsAzure} ${
    totalResultsAzure !== 1 ? 'results' : 'result'
  }`;

  // TODO check if this calculation is correct!
  const totalPagesAzure = Math.floor(totalResultsAzure / 10) + 1;

  return useAzureSearch ? (
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
              {`${totalResultsMessageAzure}, ${
                totalResultsAzure
                  ? `page ${router.query.page || 1} of ${totalPagesAzure}`
                  : '0 pages'
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
              showResetFiltersButton={!isMobileMedia && isFiltered}
              sortBy={sortBy}
              sortOptions={sortOptions}
              themeId={themeId}
              themes={themes}
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
                releaseType={releaseType}
                sortBy={sortBy}
                sortOptions={sortOptions}
                themeId={themeId}
                themes={themes}
                onChange={handleChangeFilter}
                onSortChange={handleSortBy}
              />
            </FiltersMobile>
          )}

          <LoadingSpinner
            loading={isLoadingAzure || isFetchingAzure}
            className="govuk-!-margin-top-4"
          >
            {isErrorAzure ? (
              <WarningMessage>
                Cannot load publications, please try again later.
              </WarningMessage>
            ) : (
              <>
                {totalResultsAzure === 0 ? (
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
                    <li>render results here</li>
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
  ) : (
    <Page
      title={defaultPageTitle}
      // Don't include the default meta title when filtered to prevent too much screen reader noise.
      includeDefaultMetaTitle={pageTitle === defaultPageTitle}
      metaTitle={pageTitle}
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
                <Link to="/data-catalogue">Data catalogue</Link>
              </li>
              <li>
                <Link to="/methodology">Methodology</Link>
              </li>
              <li>
                <Link to="/glossary">Glossary</Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <hr />
      <div className="govuk-grid-row">
        <FiltersDesktop>
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
          {!isMobileMedia && (
            <Filters
              releaseType={releaseType}
              showResetFiltersButton={!isMobileMedia && isFiltered}
              themeId={themeId}
              themes={themes}
              onChange={handleChangeFilter}
              onResetFilters={() => handleResetFilter({ filterType: 'all' })}
            />
          )}
        </FiltersDesktop>
        <div className="govuk-grid-column-two-thirds">
          <div>
            <h2
              aria-hidden
              className="govuk-!-margin-bottom-2"
              data-testid="total-results"
            >
              {totalResultsMessage}
            </h2>
            {/* Using ScreenReaderMessage here as the message is announced twice if have
            aria-live etc on the h2. */}
            <ScreenReaderMessage message={totalResultsMessage} />

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

              {isMobileMedia && isFiltered && (
                <ButtonText
                  onClick={() => handleResetFilter({ filterType: 'all' })}
                >
                  Reset filters
                </ButtonText>
              )}
            </div>

            {isFiltered && (
              <div className="govuk-!-margin-bottom-5 dfe-flex dfe-flex-wrap">
                {search && (
                  <FilterResetButton
                    filterType="Search"
                    name={search}
                    onClick={() => handleResetFilter({ filterType: 'search' })}
                  />
                )}
                {selectedTheme && (
                  <FilterResetButton
                    filterType="Theme"
                    name={selectedTheme.title}
                    onClick={() => handleResetFilter({ filterType: 'themeId' })}
                  />
                )}
                {selectedReleaseType && (
                  <FilterResetButton
                    filterType="Release type"
                    name={selectedReleaseType}
                    onClick={() =>
                      handleResetFilter({ filterType: 'releaseType' })
                    }
                  />
                )}
              </div>
            )}
          </div>

          {isMobileMedia && (
            <FiltersMobile
              title="Filter publications"
              totalResults={totalResults}
            >
              <Filters
                releaseType={releaseType}
                themeId={themeId}
                themes={themes}
                onChange={handleChangeFilter}
              />
            </FiltersMobile>
          )}

          {publications.length > 0 && (
            <SortControls
              className="dfe-border-bottom dfe-border-top"
              options={sortOptions}
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

export const getServerSideProps: GetServerSideProps = async ({ query }) => {
  const queryClient = new QueryClient();

  await Promise.all([
    queryClient.prefetchQuery(publicationQueries.list(query)),
    // queryClient.prefetchQuery(publicationQueries.listAzure(query)),
    queryClient.prefetchQuery(themeQueries.list()),
  ]);

  return {
    props: {
      dehydratedState: dehydrate(queryClient),
    },
  };
};

export default FindStatisticsPage;
