import BackToTopLink from '@common/components/BackToTopLink';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import {
  ReleaseType,
  releaseTypes as statisticsReleaseTypes,
} from '@common/services/types/releaseType';
import getAsArray from '@common/utils/getAsArray';
import locationLevelsMap, {
  GeographicLevelCode,
  geographicLevelCodesMap,
} from '@common/utils/locationLevelsMap';
import FilterResetButton from '@frontend/components/FilterResetButton';
import FiltersMobile from '@frontend/components/FiltersMobile';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import { SortOption } from '@frontend/components/SortControls';
import DataSetFileSummary from '@frontend/modules/data-catalogue/components/DataSetFileSummary';
import DataModal from '@frontend/modules/find-statistics/components/DataModal';
import FindStatisticsSearchForm from '@frontend/modules/find-statistics/components/FindStatisticsSearchForm';
import PublicationResultSummary from '@frontend/modules/find-statistics/components/PublicationResultSummary';
import StatisticalReleasesModal from '@frontend/modules/find-statistics/components/StatisticalReleasesModal';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import SearchDataFilters from '@frontend/modules/search-data/components/SearchDataFilters';
import styles from '@frontend/modules/search-data/SearchDataPage.module.scss';
import { getParamsFromQuery } from '@frontend/modules/search-data/utils/createDataSetListRequest';
import {
  SearchDataFilter,
  searchDataFilters,
} from '@frontend/modules/search-data/utils/searchDataFilters';
import azureDataSetQueries from '@frontend/queries/azureDataSetQueries';
import azurePublicationQueries from '@frontend/queries/azurePublicationQueries';
import publicationQueries from '@frontend/queries/publicationQueries';
import themeQueries from '@frontend/queries/themeQueries';
import { DataSetType } from '@frontend/services/dataSetFileService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { dehydrate, QueryClient, useQuery } from '@tanstack/react-query';
import classNames from 'classnames';
import compact from 'lodash/compact';
import omit from 'lodash/omit';
import { GetServerSideProps, NextPage } from 'next';
import { useRouter } from 'next/router';
import React, { useState } from 'react';

const defaultPageTitle = 'Explore our education statistics';

type RoutePath = 'search-releases' | 'search-data';

export interface SearchDataPageQuery {
  dataSetType?: DataSetType;
  geographicLevel?: GeographicLevelCode | GeographicLevelCode[];
  latestDataOnly?: string;
  page?: number;
  publicationId?: string | string[];
  releaseType?: ReleaseType | ReleaseType[];
  search?: string;
  sortBy?: PublicationSortOption;
  themeId?: string | string[];
}

const SearchDataPage: NextPage = () => {
  const router = useRouter();
  const { isMedia: isMobileMedia } = useMobileMedia();

  const [pageTitle, setPageTitle] = useState<string>(defaultPageTitle);
  const currentRoute = router.pathname as RoutePath;
  const isPublicationsSearch = currentRoute.includes('search-releases');

  const {
    data: dataSetsData,
    isError: isDataSetsError,
    isFetching: isDataSetsFetching,
    isLoading: isDataSetsLoading,
  } = useQuery({
    ...azureDataSetQueries.list(router.query),
    keepPreviousData: true,
    refetchOnWindowFocus: false,
    staleTime: 60000,
    enabled: !isPublicationsSearch,
  });

  const {
    data: publicationsData,
    isError: isPublicationsError,
    isFetching: isPublicationsFetching,
    isLoading: isPublicationsLoading,
  } = useQuery({
    ...azurePublicationQueries.listStatisticalReleases(router.query),
    keepPreviousData: true,
    refetchOnWindowFocus: false,
    staleTime: 60000,
    enabled: isPublicationsSearch,
  });

  const { data: themes = [] } = useQuery({
    ...themeQueries.listProdThemes(),
    staleTime: Infinity,
  });

  const { data: publicationTree = [] } = useQuery({
    ...publicationQueries.getPrototypePublicationTree({
      filter: 'DataCatalogue',
    }),
    staleTime: Infinity,
  });

  const { paging: dataSetsPaging, results: dataSets = [] } = dataSetsData ?? {};

  const { paging: publicationsPaging, results: publications = [] } =
    publicationsData ?? {};

  const {
    page,
    totalPages,
    totalResults = 0,
  } = (isPublicationsSearch ? publicationsPaging : dataSetsPaging) ?? {};

  const {
    dataSetType,
    geographicLevels,
    latestDataOnly,
    publicationIds,
    releaseTypes,
    search,
    sortBy,
    themeIds,
  } = getParamsFromQuery(router.query);

  const themeOptions = themes.map(theme => {
    return {
      label: theme.title,
      value: theme.id,
    };
  });

  const releaseTypeOptions = Object.keys(statisticsReleaseTypes).map(type => {
    return {
      label: statisticsReleaseTypes[type as ReleaseType],
      value: type,
    };
  });

  const geographicLevelOptions = Object.values(locationLevelsMap).map(
    location => {
      return {
        label: location.filterLabel,
        value: location.code,
      };
    },
  );

  const selectedThemes = themes.filter(theme => themeIds?.includes(theme.id));
  const selectedReleaseTypes = releaseTypes
    ? getAsArray(releaseTypes)!.map(type => ({
        id: type,
        title: statisticsReleaseTypes[type as ReleaseType],
      }))
    : [];

  const selectedGeographicLevels = geographicLevels
    ? getAsArray(geographicLevels)!
        .map(code => geographicLevelCodesMap[code as GeographicLevelCode])
        .filter(Boolean)
    : [];

  const allPublications = publicationTree.flatMap(theme => theme.publications);
  const selectedPublications = allPublications.filter(
    publication => publicationIds?.includes(publication.id),
  );

  const isFilteredByDataSetType =
    !isPublicationsSearch && dataSetType === 'api';
  const isFilteredAllReleases = !isPublicationsSearch && !latestDataOnly;
  const isFilteredByGeographicLevel =
    !isPublicationsSearch && selectedGeographicLevels.length > 0;
  const isFilteredByPublicationId =
    !isPublicationsSearch && selectedPublications.length > 0;

  const isFiltered =
    !!search ||
    selectedReleaseTypes.length > 0 ||
    selectedThemes.length > 0 ||
    isFilteredByDataSetType ||
    isFilteredAllReleases ||
    isFilteredByGeographicLevel ||
    isFilteredByPublicationId;

  const filteredByString = compact(
    [
      search,
      ...selectedThemes.map(t => t.title),
      ...selectedReleaseTypes.map(rt => rt.title),
      isFilteredByGeographicLevel
        ? [...selectedGeographicLevels.map(gl => gl.filterLabel)]
        : undefined,
      isFilteredByPublicationId
        ? [...selectedPublications.map(p => p.title)]
        : undefined,
      isFilteredByDataSetType ? 'API data sets only' : undefined,
      isFilteredAllReleases ? 'All releases' : undefined,
    ].flat(),
  ).join(', ');

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

  const updateQueryParams = async (nextQuery: SearchDataPageQuery) => {
    // When a query param is changed for the first time Next announces the page title,
    // even if it hasn't changed.
    // Set the title here so it's more useful the just announcing the default title.
    // Have to set it before the route change otherwise the previous title will be announced.
    // It won't be reannounced on subsequent query param changes.
    setPageTitle(`${defaultPageTitle} - Search results`);
    await router.push(
      {
        query: { ...nextQuery },
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

  const multiValueFilters: string[] = [
    'themeId',
    'releaseType',
    'geographicLevel',
    'publicationId',
  ];

  const handleBatchChangeFilters = async (
    updates: Record<string, string[]>,
  ) => {
    let newQuery: Record<string, string | string[] | undefined> = {
      ...omit(router.query, 'page'),
    };

    // Apply batch updates
    Object.entries(updates).forEach(([key, values]) => {
      if (values.length === 0) {
        newQuery = omit(newQuery, key);
      } else {
        newQuery[key] = values;
      }
    });

    await updateQueryParams(newQuery as SearchDataPageQuery);

    logEvent({
      category: 'Find statistics and data',
      action: `Data filtered by themes or release`,
    });
  };

  const handleChangeFilter = async ({
    filterType,
    nextValue,
  }: {
    filterType: SearchDataFilter;
    nextValue: string;
  }) => {
    // 1. If performing a search (by search term), reset sortBy to relevance
    const nextSortBy =
      filterType === 'search' && nextValue.length > 0 ? 'relevance' : sortBy;

    // 2. Start building the new query.
    // We use a Record here so TypeScript allows dynamic key assignments.
    let newQuery: Record<string, string | string[] | undefined> = {
      ...omit(router.query, 'page'),
      sortBy: nextSortBy,
    };

    // 3. Determine if this filter should be handled as an array (checkboxes) or a single string (radios/selects)
    if (multiValueFilters.includes(filterType)) {
      // Safely grab the current query param and ensure it's an array.
      const currentParam = router.query[filterType] as
        | string
        | string[]
        | undefined;
      const currentValues = getAsArray(currentParam) ?? [];

      let updatedValues: string[];
      if (currentValues.includes(nextValue)) {
        // Remove it if already selected
        updatedValues = currentValues.filter(v => v !== nextValue);
      } else {
        // Add it if not selected
        updatedValues = [...currentValues, nextValue];
      }

      // If the array is empty, wipe it from the URL entirely. Otherwise, set it.
      if (updatedValues.length === 0) {
        newQuery = omit(newQuery, filterType);
      } else {
        newQuery[filterType] = updatedValues;
      }
    } else if (nextValue === 'all' && filterType !== 'dataSetType') {
      // Wipe the parameter from the URL if 'all' is selected (and it's not the dataSetType value of 'all')
      newQuery = omit(newQuery, filterType);
    } else {
      // Set the specific single string value
      newQuery[filterType] = nextValue;
    }

    await updateQueryParams(newQuery as SearchDataPageQuery);

    logEvent({
      category: 'Find statistics and data',
      action: `Publications filtered by ${filterType}`,
      label: nextValue,
    });
  };

  const handleResetFilter = async ({
    filterType,
  }: {
    filterType: SearchDataFilter | 'all';
  }) => {
    // Reset sortBy to newest if removing a search and sorting by relevance
    const nextSortBy = search && sortBy === 'relevance' ? 'newest' : sortBy;

    const newParams =
      filterType === 'all'
        ? {
            ...omit(router.query, 'page', ...searchDataFilters),
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

  const isLoading =
    (!isPublicationsSearch && isDataSetsLoading) ||
    (isPublicationsSearch && isPublicationsLoading);
  const isFetching =
    (!isPublicationsSearch && isDataSetsFetching) ||
    (isPublicationsSearch && isPublicationsFetching);
  const isError =
    (!isPublicationsSearch && isDataSetsError) ||
    (isPublicationsSearch && isPublicationsError);

  return (
    <Page
      title={defaultPageTitle}
      // Don't include the default meta title when filtered to prevent too much screen reader noise.
      includeDefaultMetaTitle={pageTitle === defaultPageTitle}
      metaTitle={pageTitle}
      width="wide"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <FindStatisticsSearchForm
            label={isPublicationsSearch ? 'Search releases' : 'Search data'}
            onSubmit={nextValue =>
              handleChangeFilter({ filterType: 'search', nextValue })
            }
            isSearchData={!isPublicationsSearch}
          />
          <a href="#searchResults" className="govuk-skip-link">
            Skip to search results
          </a>

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
        <div className="govuk-grid-column-one-third">
          <div className={styles.helpColumn}>
            <RelatedInformation heading="Help and related information">
              <ul className="govuk-list">
                <li>
                  <StatisticalReleasesModal />
                </li>
                <li>
                  <DataModal />
                </li>
                <li>
                  <Link to="/glossary">Glossary</Link>
                </li>
              </ul>
            </RelatedInformation>
          </div>
        </div>
      </div>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-full">
          <nav aria-label="Search mode">
            <ul className={styles.nav}>
              <li>
                <Link
                  to={{
                    pathname: '/search-releases',
                    query: {
                      ...router.query,
                    },
                  }}
                  shallow
                  unvisited
                  aria-current={isPublicationsSearch ? 'page' : undefined}
                  className={styles.navItemLink}
                >
                  Statistical releases
                </Link>
              </li>
              <li>
                <Link
                  to={{
                    pathname: '/search-data',
                    query: { ...router.query },
                  }}
                  shallow
                  unvisited
                  aria-current={!isPublicationsSearch ? 'page' : undefined}
                  className={styles.navItemLink}
                >
                  Data
                </Link>
              </li>
            </ul>
          </nav>
        </div>
      </div>
      <div className="govuk-grid-row">
        <div
          className={classNames([
            'govuk-grid-column-one-third',
            styles.stickyFilters,
          ])}
        >
          {!isMobileMedia && (
            <SearchDataFilters
              dataSetType={dataSetType}
              geographicLevels={geographicLevels}
              geographicLevelOptions={geographicLevelOptions}
              includeDataFilters={!isPublicationsSearch}
              latestDataOnly={latestDataOnly}
              publicationIds={publicationIds}
              publicationTree={publicationTree}
              releaseTypes={releaseTypes}
              releaseTypeOptions={releaseTypeOptions}
              sortBy={sortBy}
              sortOptions={sortOptions}
              themeIds={themeIds}
              themes={themes}
              themeOptions={themeOptions}
              onChange={handleChangeFilter}
              onChangeBatch={handleBatchChangeFilters}
              onSortChange={handleSortBy}
            />
          )}
        </div>
        <div className="govuk-grid-column-two-thirds">
          {isMobileMedia && (
            <FiltersMobile title="Filter and sort" totalResults={totalResults}>
              <SearchDataFilters
                dataSetType={dataSetType}
                geographicLevels={geographicLevels}
                geographicLevelOptions={geographicLevelOptions}
                includeDataFilters={!isPublicationsSearch}
                latestDataOnly={latestDataOnly}
                publicationIds={publicationIds}
                publicationTree={publicationTree}
                releaseTypeOptions={releaseTypeOptions}
                releaseTypes={releaseTypes}
                sortBy={sortBy}
                sortOptions={sortOptions}
                themeIds={themeIds}
                themes={themes}
                themeOptions={themeOptions}
                onChange={handleChangeFilter}
                onChangeBatch={handleBatchChangeFilters}
                onSortChange={handleSortBy}
              />
            </FiltersMobile>
          )}

          <h3
            className="govuk-!-margin-top-3 govuk-!-margin-bottom-0"
            data-testid="total-results"
          >
            {totalResultsMessage}
          </h3>
          <div className="dfe-flex dfe-flex-wrap dfe-gap-2 dfe-align-items--center">
            <p className="govuk-!-margin-bottom-0">
              {totalResults ? `Page ${page} of ${totalPages}` : '0 pages'},
              sorted by {sortBy === 'title' ? 'A to Z' : sortBy}
              {isFiltered ? ', filtered by: ' : ''}
            </p>

            {isFiltered && <VisuallyHidden>{filteredByString}</VisuallyHidden>}

            <div className="dfe-flex dfe-flex-wrap dfe-gap-2 dfe-align-items--center">
              {search && (
                <FilterResetButton
                  filterType="Search"
                  name={search}
                  onClick={() => handleResetFilter({ filterType: 'search' })}
                />
              )}

              {selectedThemes.map(theme => (
                <FilterResetButton
                  key={theme.id}
                  filterType="Theme"
                  name={theme.title}
                  onClick={() =>
                    handleChangeFilter({
                      filterType: 'themeId',
                      nextValue: theme.id,
                    })
                  }
                />
              ))}

              {isFilteredByGeographicLevel &&
                selectedGeographicLevels.map(gl => (
                  <FilterResetButton
                    key={gl.code}
                    filterType="Geographic level"
                    name={gl.filterLabel}
                    onClick={() =>
                      handleChangeFilter({
                        filterType: 'geographicLevel',
                        nextValue: gl.code,
                      })
                    }
                  />
                ))}

              {isFilteredByPublicationId &&
                selectedPublications.map(pub => (
                  <FilterResetButton
                    key={pub.id}
                    filterType="Publication"
                    name={pub.title}
                    onClick={() =>
                      handleChangeFilter({
                        filterType: 'publicationId',
                        nextValue: pub.id,
                      })
                    }
                  />
                ))}

              {selectedReleaseTypes.map(rt => (
                <FilterResetButton
                  key={rt.id}
                  filterType="Release type"
                  name={rt.title}
                  onClick={() =>
                    handleChangeFilter({
                      filterType: 'releaseType',
                      nextValue: rt.id,
                    })
                  }
                />
              ))}

              {isFilteredByDataSetType && (
                <FilterResetButton
                  filterType="Data set type"
                  name="API"
                  onClick={() =>
                    handleResetFilter({ filterType: 'dataSetType' })
                  }
                />
              )}
              {isFilteredAllReleases && (
                <FilterResetButton
                  filterType="Releases"
                  name="all"
                  onClick={() =>
                    handleResetFilter({ filterType: 'latestDataOnly' })
                  }
                />
              )}
            </div>

            {isFiltered && (
              <div className="govuk-!-width-full">
                <ButtonText
                  onClick={() => handleResetFilter({ filterType: 'all' })}
                >
                  Clear all filters
                </ButtonText>
              </div>
            )}
          </div>

          <LoadingSpinner
            loading={isLoading || isFetching}
            className="govuk-!-margin-top-4"
          >
            {isError ? (
              <WarningMessage>
                Cannot load {isPublicationsSearch ? 'releases' : 'data sets'},
                please try again later.
              </WarningMessage>
            ) : (
              <>
                {totalResults === 0 ? (
                  <div
                    className="govuk-!-margin-top-5 dfe-border-top govuk-!-padding-top-4"
                    id="searchResults"
                  >
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
                    className="govuk-list govuk-!-margin-top-5 dfe-border-top"
                    id="searchResults"
                    data-testid={
                      isPublicationsSearch
                        ? 'publicationsList'
                        : 'data-set-file-list'
                    }
                  >
                    {isPublicationsSearch
                      ? publications.map(pub => (
                          <PublicationResultSummary
                            key={pub.id}
                            publication={pub}
                          />
                        ))
                      : dataSets.map(dataSet => (
                          <DataSetFileSummary
                            key={dataSet.fileId}
                            dataSetFile={dataSet}
                            expandable={false}
                            expanded
                          />
                        ))}
                  </ul>
                )}
              </>
            )}
            {page && !!totalPages && (
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

          {(isPublicationsSearch && publications.length > 0) ||
            (!isPublicationsSearch && dataSets.length > 0 && (
              <BackToTopLink className="govuk-!-margin-top-7" />
            ))}
        </div>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps = async ({
  query,
  resolvedUrl,
}) => {
  if (process.env.APP_ENV === 'Production') {
    return {
      notFound: true,
    };
  }

  const isPublicationsSearch = resolvedUrl.startsWith('/search-releases');

  const queryClient = new QueryClient();

  await Promise.all([
    isPublicationsSearch
      ? queryClient.prefetchQuery(
          azurePublicationQueries.listStatisticalReleases(query),
        )
      : queryClient.prefetchQuery(azureDataSetQueries.list(query)),
    isPublicationsSearch
      ? queryClient.prefetchQuery(themeQueries.listProdThemes())
      : queryClient.prefetchQuery(
          publicationQueries.getPrototypePublicationTree({
            filter: 'DataCatalogue',
          }),
        ),
  ]);

  return {
    props: {
      dehydratedState: dehydrate(queryClient),
    },
  };
};

export default SearchDataPage;
