import BackToTopLink from '@common/components/BackToTopLink';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import RelatedInformation from '@common/components/RelatedInformation';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import FilterResetButton from '@frontend/components/FilterResetButton';
import FiltersMobile from '@frontend/components/FiltersMobile';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import { SortOption } from '@frontend/components/SortControls';
import DataSetFileSummary from '@frontend/modules/data-catalogue/components/DataSetFileSummary';
import FindStatisticsSearchForm from '@frontend/modules/find-statistics/components/FindStatisticsSearchForm';
import PublicationResultSummary from '@frontend/modules/find-statistics/components/PublicationResultSummary';
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
import themeQueries from '@frontend/queries/themeQueries';
import { DataSetType } from '@frontend/services/dataSetFileService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { dehydrate, QueryClient, useQuery } from '@tanstack/react-query';
import compact from 'lodash/compact';
import omit from 'lodash/omit';
import { GetServerSideProps, NextPage } from 'next';
import { useRouter } from 'next/router';
import React, { useState } from 'react';

const defaultPageTitle = 'Explore our education statistics';

type RoutePath = 'search-releases' | 'search-data';

export interface SearchDataPageQuery {
  dataSetType?: DataSetType;
  latestDataOnly?: string;
  page?: number;
  releaseType?: ReleaseType;
  search?: string;
  sortBy?: PublicationSortOption;
  themeId?: string;
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
    ...azurePublicationQueries.list(router.query),
    keepPreviousData: true,
    refetchOnWindowFocus: false,
    staleTime: 60000,
    enabled: isPublicationsSearch,
  });

  const { data: themes = [] } = useQuery({
    ...themeQueries.listProdThemes(),
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

  const { dataSetType, latestDataOnly, releaseType, search, sortBy, themeId } =
    getParamsFromQuery(router.query);

  const themeOptions = themes.map(theme => {
    return {
      label: theme.title,
      value: theme.id,
    };
  });

  const releaseTypeOptions = Object.keys(releaseTypes).map(type => {
    return {
      label: releaseTypes[type as ReleaseType],
      value: type,
    };
  });

  const isFiltered =
    !!search ||
    !!releaseType ||
    !!themeId ||
    (!isPublicationsSearch && dataSetType === 'api');

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

  const handleChangeFilter = async ({
    filterType,
    nextValue,
  }: {
    filterType: SearchDataFilter;
    nextValue: string;
  }) => {
    // If performing a search (by search term), reset sortBy to relevance
    const nextSortBy =
      filterType === 'search' && nextValue.length > 0 ? 'relevance' : sortBy;

    const newParams =
      nextValue === 'all' && filterType !== 'dataSetType'
        ? omit(router.query, 'page', filterType)
        : {
            ...omit(router.query, 'page'),
            [filterType]: nextValue,
            sortBy: nextSortBy,
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
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <FindStatisticsSearchForm
            label={isPublicationsSearch ? 'Search publications' : 'Search data'}
            onSubmit={nextValue =>
              handleChangeFilter({ filterType: 'search', nextValue })
            }
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
                  <Modal
                    showClose
                    title="What are statistical releases?"
                    triggerButton={
                      <ButtonText>
                        What are statistical releases?{' '}
                        <InfoIcon description="Information on statistical releases" />
                      </ButtonText>
                    }
                  >
                    <p>Information about statistical releases</p>
                  </Modal>
                </li>
                <li>
                  <Modal
                    showClose
                    title="What is data?"
                    triggerButton={
                      <ButtonText>
                        What is data?{' '}
                        <InfoIcon description="Information on data" />
                      </ButtonText>
                    }
                  >
                    <p>Information about what is data</p>
                  </Modal>
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
        <div className="govuk-grid-column-one-third">
          {!isMobileMedia && (
            <SearchDataFilters
              dataSetType={dataSetType as DataSetType}
              includeDataFilters={!isPublicationsSearch}
              latestDataOnly={latestDataOnly}
              releaseType={releaseType}
              releaseTypeOptions={releaseTypeOptions}
              showResetFiltersButton={!isMobileMedia && isFiltered}
              sortBy={sortBy}
              sortOptions={sortOptions}
              themeId={themeId}
              themes={themes}
              themeOptions={themeOptions}
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
              <SearchDataFilters
                dataSetType={dataSetType as DataSetType}
                includeDataFilters={!isPublicationsSearch}
                latestDataOnly={latestDataOnly}
                releaseTypeOptions={releaseTypeOptions}
                releaseType={releaseType}
                sortBy={sortBy}
                sortOptions={sortOptions}
                themeId={themeId}
                themes={themes}
                themeOptions={themeOptions}
                onChange={handleChangeFilter}
                onSortChange={handleSortBy}
              />
            </FiltersMobile>
          )}

          <div className="govuk-!-margin-top-3 dfe-flex dfe-flex-wrap dfe-gap-2 dfe-align-items--center">
            <p className="govuk-!-margin-bottom-0" data-testid="total-results">
              {`${totalResultsMessage}, ${
                totalResults ? `page ${page} of ${totalPages}` : '0 pages'
              }, ${
                isFiltered
                  ? 'filtered by: '
                  : `showing ${
                      isPublicationsSearch ? 'all publications' : 'data sets'
                    }`
              }`}
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
              {!isPublicationsSearch && dataSetType === 'api' && (
                <FilterResetButton
                  filterType="Data set type"
                  name="API"
                  onClick={() =>
                    handleResetFilter({ filterType: 'dataSetType' })
                  }
                />
              )}
            </div>

            <VisuallyHidden>{` Sorted by ${
              sortBy === 'title' ? 'A to Z' : sortBy
            }`}</VisuallyHidden>
          </div>

          <LoadingSpinner
            loading={isLoading || isFetching}
            className="govuk-!-margin-top-4"
          >
            {isError ? (
              <WarningMessage>
                Cannot load{' '}
                {isPublicationsSearch ? 'publications' : 'data sets'}, please
                try again later.
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
      ? queryClient.prefetchQuery(azurePublicationQueries.list(query))
      : queryClient.prefetchQuery(azureDataSetQueries.list(query)),
    queryClient.prefetchQuery(themeQueries.listProdThemes()),
  ]);

  return {
    props: {
      dehydratedState: dehydrate(queryClient),
    },
  };
};

export default SearchDataPage;
