import { Theme } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import { GetServerSideProps, NextPage } from 'next';
import React, { useEffect, useState } from 'react';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import {
  QueryClient,
  dehydrate,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query';
import publicationQueries from '@frontend/queries/publicationQueries';
import Pagination from '@frontend/components/Pagination';
import Link from '@frontend/components/Link';
import AccordionToggleButton from '@common/components/AccordionToggleButton';
import ButtonText from '@common/components/ButtonText';
import GoToTopLink from '@common/components/GoToTopLink';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import useToggle from '@common/hooks/useToggle';
import { releaseTypes } from '@common/services/types/releaseType';
import { SortDirection } from '@common/services/types/sort';
import FilterResetButton from '@frontend/components/FilterResetButton';
import FiltersDesktop from '@frontend/components/FiltersDesktop';
import FiltersMobile from '@frontend/components/FiltersMobile';
import Page from '@frontend/components/Page';
import SearchForm from '@frontend/components/SearchForm';
import SortControls, { SortOption } from '@frontend/components/SortControls';
import DataSetFileSummary from '@frontend/modules/data-catalogue/components/DataSetFileSummary';
import Filters from '@frontend/modules/data-catalogue/components/Filters';
import createDataSetFileListRequest, {
  getParamsFromQuery,
} from '@frontend/modules/data-catalogue/utils/createDataSetFileListRequest';
import getUpdatedQueryParams from '@frontend/modules/data-catalogue/utils/getUpdatedQueryParams';
import {
  DataSetFileFilter,
  dataSetFileFilters,
} from '@frontend/modules/data-catalogue/utils/dataSetFileFilters';
import { DataSetFileSortOption } from '@frontend/modules/data-catalogue/utils/dataSetFileSortOptions';
import { DataSetType } from '@frontend/services/dataSetFileService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import compact from 'lodash/compact';
import isEqual from 'lodash/isEqual';
import omit from 'lodash/omit';
import { useRouter } from 'next/router';
import { ParsedUrlQuery } from 'querystring';
import {
  GeographicLevelCode,
  geographicLevelCodesMap,
} from '@common/utils/locationLevelsMap';
import downloadService from '@frontend/services/downloadService';
import Button from '@common/components/Button';

const defaultPageTitle = 'Data catalogue';

export interface DataCataloguePageQuery {
  dataSetType?: DataSetType;
  latestOnly?: string;
  page?: number;
  publicationId?: string;
  releaseVersionId?: string;
  geographicLevel?: GeographicLevelCode;
  searchTerm?: string;
  sortBy?: DataSetFileSortOption;
  sortDirection?: SortDirection;
  themeId?: string;
}

interface Props {
  showTypeFilter?: boolean;
  themes?: Theme[];
}

const DataCataloguePage: NextPage<Props> = ({ showTypeFilter }) => {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { isMedia: isMobileMedia } = useMobileMedia();

  const [pageTitle, setPageTitle] = useState<string>(defaultPageTitle);

  const {
    dataSetType,
    latestOnly,
    sortBy,
    publicationId,
    releaseId: releaseVersionId,
    geographicLevel,
    searchTerm,
    themeId,
  } = getParamsFromQuery(router.query);

  const {
    data: dataSetsData,
    isError,
    isFetching,
    isLoading,
  } = useQuery({
    ...dataSetFileQueries.list(createDataSetFileListRequest(router.query)),
    keepPreviousData: true,
    staleTime: 60000,
  });

  const { data: themes = [], isLoading: isLoadingThemes } = useQuery({
    ...publicationQueries.getPublicationTree({
      publicationFilter: 'DataCatalogue',
    }),
    staleTime: Infinity,
  });

  const selectedTheme = themes.find(theme => theme.id === themeId);
  const publications = themes.find(theme => theme.id === themeId)?.publications;
  const selectedPublication = publications?.find(
    publication => publication.id === publicationId,
  );

  const { data: releases = [] } = useQuery({
    ...publicationQueries.listReleases(selectedPublication?.slug ?? ''),
    staleTime: Infinity,
    enabled: !!releaseVersionId && !!publicationId && !!themeId,
  });

  const selectedRelease = releases.find(
    release => release.id === releaseVersionId,
  );

  const selectedGeographicLevel = geographicLevel
    ? geographicLevelCodesMap[geographicLevel]
    : undefined;

  const { paging, results: dataSets = [] } = dataSetsData ?? {};
  const { page, totalPages, totalResults = 0 } = paging ?? {};
  const [showAllDetails, toggleAllDetails] = useToggle(false);

  const isFiltered =
    !!publicationId || !!searchTerm || !!themeId || !!geographicLevel;

  const filteredByString = compact([
    searchTerm,
    selectedTheme?.title,
    selectedPublication?.title,
    geographicLevel,
  ]).join(', ');

  const updateQueryParams = async (nextQuery: DataCataloguePageQuery) => {
    if (isEqual(nextQuery, router.query)) {
      return;
    }
    // When a query param is changed for the first time Next announces the page title,
    // even if it hasn't changed.
    // Set the title here so it's more useful the just announcing the default title.
    // Have to set it before the route change otherwise the previous title will be announced.
    // It won't be reannounced on subsequent query param changes.
    setPageTitle(`${defaultPageTitle} - Search results`);
    await router.push(
      {
        pathname: '/data-catalogue',
        query: nextQuery as ParsedUrlQuery,
      },
      undefined,
      {
        shallow: true,
      },
    );
  };

  useEffect(() => {
    async function handleParamsOnLoad() {
      if (!publicationId) {
        return;
      }
      const publicationThemeId =
        themeId ??
        getThemeForPublication({
          publicationId,
          themes,
        })?.id;

      const publicationSlug = themes
        .find(theme => theme.id === publicationThemeId)
        ?.publications?.find(publication => publication.id === publicationId)
        ?.slug;

      const newParams = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: publicationId,
        sortBy,
        query: {
          ...router.query,
          themeId: publicationThemeId,
        },
        ...(!latestOnly && releaseVersionId && { releaseVersionId }),
        onFetchReleases: () =>
          queryClient.fetchQuery(
            publicationQueries.listReleases(publicationSlug ?? ''),
          ),
      });

      await updateQueryParams(newParams);
    }

    if (isLoadingThemes) {
      return;
    }
    if (publicationId) {
      handleParamsOnLoad();
    } else if (releaseVersionId) {
      updateQueryParams({ ...omit(router.query, 'releaseVersionId') });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isLoadingThemes]);

  const handleChangeFilter = async ({
    filterType,
    nextValue,
  }: {
    filterType: DataSetFileFilter;
    nextValue: string;
  }) => {
    const newParams = await getUpdatedQueryParams({
      filterType,
      nextValue,
      sortBy,
      query: router.query,
      ...(filterType === 'publicationId' && {
        onFetchReleases: () =>
          queryClient.fetchQuery(
            publicationQueries.listReleases(
              publications?.find(publication => publication.id === nextValue)
                ?.slug ?? '',
            ),
          ),
      }),
    });

    await updateQueryParams(newParams);

    logEvent({
      category: 'Data catalogue',
      action: `Data sets filtered by ${filterType}`,
      label: nextValue,
    });
  };

  const handleResetFilter = async ({
    filterType,
  }: {
    filterType: DataSetFileFilter | 'all';
  }) => {
    await updateQueryParams({
      ...(filterType === 'all'
        ? omit(router.query, 'page', 'latestOnly', ...dataSetFileFilters)
        : omit(router.query, getFiltersToRemove(filterType), 'page')),
      sortBy: searchTerm && sortBy === 'relevance' ? 'newest' : sortBy,
      ...(filterType === 'releaseVersionId' && { latestOnly: 'false' }),
    });

    logEvent({
      category: 'Data catalogue',
      action: `Reset ${filterType} filter`,
    });
  };

  const handleSortByChange = async (nextSortBy: DataSetFileSortOption) => {
    await updateQueryParams({
      ...omit(router.query, 'page'),
      sortBy: nextSortBy,
    });

    logEvent({
      category: 'Data catalogue',
      action: 'Data sets sorted',
      label: nextSortBy,
    });
  };

  const totalResultsMessage = `${totalResults} ${
    totalResults !== 1 ? 'data sets' : 'data set'
  }`;

  return (
    <Page
      title={defaultPageTitle}
      // Don't include the default meta title when filtered to prevent too much screen reader noise.
      includeDefaultMetaTitle={pageTitle === defaultPageTitle}
      metaTitle={pageTitle}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Find and download data sets with associated guidance files.
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Related information">
            <ul className="govuk-list">
              <li>
                <Link to="/find-statistics">Find statistics and data</Link>
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
            label="Search data sets"
            value={searchTerm}
            onSubmit={nextValue =>
              handleChangeFilter({ filterType: 'searchTerm', nextValue })
            }
          />
          <a href="#searchResults" className="govuk-skip-link">
            Skip to search results
          </a>

          {!isMobileMedia && (
            <LoadingSpinner loading={isLoadingThemes}>
              <Filters
                dataSetType={dataSetType}
                latestOnly={latestOnly}
                publicationId={publicationId}
                publications={publications}
                releaseVersionId={releaseVersionId}
                geographicLevel={geographicLevel}
                releases={releases}
                showResetFiltersButton={!isMobileMedia && isFiltered}
                showTypeFilter={showTypeFilter}
                themeId={themeId}
                themes={themes}
                onChange={handleChangeFilter}
                onResetFilters={() => handleResetFilter({ filterType: 'all' })}
              />
            </LoadingSpinner>
          )}
        </FiltersDesktop>
        <div className="govuk-grid-column-two-thirds">
          <div className="dfe-border-bottom">
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
                  isFiltered
                    ? 'filtered by: '
                    : 'showing all available data sets'
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
              <div className="govuk-!-padding-bottom-2 dfe-flex dfe-flex-wrap dfe-gap-2">
                {searchTerm && (
                  <FilterResetButton
                    filterType="Search"
                    name={searchTerm}
                    onClick={() =>
                      handleResetFilter({ filterType: 'searchTerm' })
                    }
                  />
                )}
                {selectedTheme && (
                  <FilterResetButton
                    filterType="Theme"
                    name={selectedTheme.title}
                    onClick={() => handleResetFilter({ filterType: 'themeId' })}
                  />
                )}
                {selectedPublication && (
                  <FilterResetButton
                    filterType="Publication"
                    name={selectedPublication.title}
                    onClick={() =>
                      handleResetFilter({ filterType: 'publicationId' })
                    }
                  />
                )}
                {selectedRelease && (
                  <FilterResetButton
                    filterType="Release"
                    name={selectedRelease.title}
                    onClick={() =>
                      handleResetFilter({ filterType: 'releaseVersionId' })
                    }
                  />
                )}
                {selectedGeographicLevel && (
                  <FilterResetButton
                    filterType="Geographic level"
                    name={selectedGeographicLevel.filterLabel}
                    onClick={() =>
                      handleResetFilter({ filterType: 'geographicLevel' })
                    }
                  />
                )}
              </div>
            )}
          </div>

          {isMobileMedia && (
            <LoadingSpinner loading={isLoadingThemes}>
              <FiltersMobile
                title="Filter data sets"
                totalResults={totalResults}
              >
                <Filters
                  dataSetType={dataSetType}
                  latestOnly={latestOnly}
                  publicationId={publicationId}
                  publications={publications}
                  releaseVersionId={releaseVersionId}
                  geographicLevel={geographicLevel}
                  releases={releases}
                  showTypeFilter={showTypeFilter}
                  themeId={themeId}
                  themes={themes}
                  onChange={handleChangeFilter}
                />
              </FiltersMobile>
            </LoadingSpinner>
          )}

          <LoadingSpinner
            loading={isLoading || isFetching}
            className="govuk-!-margin-top-4"
          >
            {isError ? (
              <WarningMessage>
                Cannot load data sets, please try again later.
              </WarningMessage>
            ) : (
              <>
                {dataSets.length === 0 ? (
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
                  <>
                    <div className="dfe-border-bottom">
                      {!releaseVersionId && (
                        <SortControls
                          options={[
                            { label: 'Newest', value: 'newest' },
                            { label: 'Oldest', value: 'oldest' },
                            { label: 'A to Z', value: 'title' },
                            ...(searchTerm
                              ? [
                                  {
                                    label: 'Relevance',
                                    value: 'relevance',
                                  } as SortOption,
                                ]
                              : []),
                          ]}
                          sortBy={sortBy}
                          onChange={handleSortByChange}
                        />
                      )}
                      {selectedPublication && selectedRelease && (
                        <div
                          className="govuk-!-padding-top-4"
                          data-testid="release-info"
                        >
                          <h3>{`${selectedPublication.title} - ${selectedRelease?.title} downloads`}</h3>
                          {selectedRelease && (
                            <>
                              <TagGroup>
                                <Tag>{releaseTypes[selectedRelease.type]}</Tag>
                                <Tag
                                  colour={
                                    selectedRelease.latestRelease &&
                                    !selectedPublication.isSuperseded
                                      ? undefined
                                      : 'orange'
                                  }
                                >
                                  {selectedRelease.latestRelease &&
                                  !selectedPublication.isSuperseded
                                    ? 'This is the latest data'
                                    : 'This is not the latest data'}
                                </Tag>
                              </TagGroup>
                              <div className="govuk-!-margin-bottom-5 govuk-!-margin-top-3">
                                <Link
                                  to={`/find-statistics/${selectedPublication.slug}/${selectedRelease.slug}`}
                                >
                                  View this release
                                </Link>
                              </div>

                              {!searchTerm && (
                                <p>
                                  <Button
                                    className="govuk-!-margin-bottom-2"
                                    onClick={async () => {
                                      await downloadService.downloadZip(
                                        selectedRelease.id,
                                        'DataCatalogue',
                                      );

                                      logEvent({
                                        category: 'Data catalogue',
                                        action: 'Data set file download - all',
                                        label: `Publication: ${selectedPublication.title}, Release: ${selectedRelease.title}`,
                                      });
                                    }}
                                  >
                                    {`Download ${
                                      totalResults === 1
                                        ? '1 data set'
                                        : `all ${totalResults} data sets`
                                    } (ZIP)`}
                                  </Button>
                                  <br />
                                  <span>
                                    Download includes data guidance and
                                    supporting files.
                                  </span>
                                </p>
                              )}
                            </>
                          )}
                        </div>
                      )}

                      <AccordionToggleButton
                        expanded={showAllDetails}
                        label={
                          showAllDetails
                            ? 'Hide all details'
                            : 'Show all expanded details'
                        }
                        onClick={() => {
                          toggleAllDetails();
                          logEvent({
                            category: 'Data catalogue',
                            action: 'All data set details toggled',
                          });
                        }}
                      />
                    </div>

                    <ul
                      className="govuk-list"
                      id="searchResults"
                      data-testid="data-set-file-list"
                    >
                      {dataSets.map(dataSet => (
                        <DataSetFileSummary
                          key={dataSet.fileId}
                          dataSetFile={dataSet}
                          expanded={showAllDetails}
                          headingTag={selectedPublication ? 'h4' : 'h3'}
                          showLatestDataTag={!selectedRelease}
                          showPublicationTitle={!selectedPublication}
                        />
                      ))}
                    </ul>
                  </>
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
                    category: 'Data catalogue',
                    action: `Pagination clicked`,
                    label: `Page ${pageNumber}`,
                  });
                }}
              />
            )}
          </LoadingSpinner>

          {dataSets.length > 0 && (
            <GoToTopLink className="govuk-!-margin-top-7" />
          )}
        </div>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async context => {
  const { publicationSlug = '', releaseSlug = '' } =
    context.query as Dictionary<string>;

  let showTypeFilter = context.query.dataSetType === 'api';

  const queryClient = new QueryClient();

  const themes = await queryClient.fetchQuery(
    publicationQueries.getPublicationTree({
      publicationFilter: 'DataCatalogue',
    }),
  );

  // Redirect old slug based links to new format.
  if (publicationSlug) {
    const selectedPublication = themes
      .flatMap(option => option.publications)
      .find(option => option.slug === publicationSlug);

    if (selectedPublication) {
      let selectedReleaseVersionId: string | undefined;
      if (releaseSlug) {
        const releases = await queryClient.fetchQuery(
          publicationQueries.listReleases(selectedPublication.slug),
        );
        selectedReleaseVersionId = releases.find(
          rel => rel.slug === releaseSlug,
        )?.id;
      }
      const redirectUrlQuery = selectedReleaseVersionId
        ? `publicationId=${selectedPublication.id}&releaseVersionId=${selectedReleaseVersionId}`
        : `publicationId=${selectedPublication.id}`;
      return {
        redirect: {
          destination: `/data-catalogue?${redirectUrlQuery}`,
          permanent: true,
        },
      };
    }
    return {
      redirect: {
        destination: `/data-catalogue`,
        permanent: true,
      },
    };
  }

  await queryClient.prefetchQuery(
    dataSetFileQueries.list(createDataSetFileListRequest(context.query)),
  );

  if (!showTypeFilter) {
    const apiDataSets = await queryClient.fetchQuery(
      dataSetFileQueries.list({
        dataSetType: 'api',
        latestOnly: 'false',
        page: 1,
      }),
    );
    showTypeFilter = !!apiDataSets.results.length;
  }

  const props: Props = {
    showTypeFilter,
    themes,
  };

  return {
    props: { ...props, dehydratedState: dehydrate(queryClient) },
  };
};

export default DataCataloguePage;

function getFiltersToRemove(filterType: DataSetFileFilter | 'all') {
  if (filterType === 'themeId') {
    return ['themeId', 'publicationId', 'releaseVersionId'];
  }
  if (filterType === 'publicationId') {
    return ['publicationId', 'releaseVersionId'];
  }
  return filterType;
}

function getThemeForPublication({
  publicationId,
  themes,
}: {
  publicationId: string;
  themes: Theme[];
}) {
  return themes.find(theme =>
    theme.publications.find(pub => pub.id === publicationId),
  );
}
