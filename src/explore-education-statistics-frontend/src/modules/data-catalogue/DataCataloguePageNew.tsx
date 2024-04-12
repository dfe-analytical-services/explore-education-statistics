import GoToTopLink from '@common/components/GoToTopLink';
import TagGroup from '@common/components/TagGroup';
import AccordionToggleButton from '@common/components/AccordionToggleButton';
import useToggle from '@common/hooks/useToggle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import WarningMessage from '@common/components/WarningMessage';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import Button from '@common/components/Button';
import { releaseTypes } from '@common/services/types/releaseType';
import downloadService from '@common/services/downloadService';
import { Theme } from '@common/services/publicationService';
import { useMobileMedia } from '@common/hooks/useMedia';
import { SortDirection } from '@common/services/types/sort';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import DataSetFileSummary from '@frontend/modules/data-catalogue/components/DataSetFileSummary';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import {
  DataSetFileFilter,
  DataSetFileSortOption,
  dataSetFileFilters,
} from '@frontend/services/dataSetFileService';
import Filters from '@frontend/modules/data-catalogue/components/Filters';
import SearchForm from '@frontend/components/SearchForm';
import { getParamsFromQuery } from '@frontend/modules/data-catalogue/utils/createDataSetFileListRequest';
import styles from '@frontend/modules/data-catalogue/DataCataloguePage.module.scss';
import publicationQueries from '@frontend/queries/publicationQueries';
import FilterClearButton from '@frontend/components/FilterClearButton';
import FiltersMobile from '@frontend/components/FiltersMobile';
import getUpdatedQueryParams from '@frontend/modules/data-catalogue/utils/getUpdatedQueryParams';
import SortControls, { SortOption } from '@frontend/components/SortControls';
import compact from 'lodash/compact';
import omit from 'lodash/omit';
import { ParsedUrlQuery } from 'querystring';
import React, { useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/router';

export interface DataCataloguePageQuery {
  latestOnly?: string;
  page?: number;
  publicationId?: string;
  releaseId?: string;
  searchTerm?: string;
  sortBy?: DataSetFileSortOption;
  sortDirection?: SortDirection;
  themeId?: string;
}

export default function DataCataloguePageNew() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { isMedia: isMobileMedia } = useMobileMedia();

  const { latestOnly, sortBy, publicationId, releaseId, searchTerm, themeId } =
    getParamsFromQuery(router.query);

  const {
    data: dataSetsData,
    isError,
    isFetching,
    isLoading,
  } = useQuery({
    ...dataSetFileQueries.list(router.query),
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
  const publications = themes
    .find(theme => theme.id === themeId)
    ?.topics.flatMap(topic => topic.publications);
  const selectedPublication = publications?.find(
    publication => publication.id === publicationId,
  );

  const { data: releases = [] } = useQuery({
    ...publicationQueries.listReleases(selectedPublication?.slug ?? ''),
    staleTime: Infinity,
    enabled: !!releaseId && !!publicationId && !!themeId,
  });

  const selectedRelease = releases.find(release => release.id === releaseId);

  const { paging, results: dataSets = [] } = dataSetsData ?? {};
  const { page, totalPages, totalResults = 0 } = paging ?? {};
  const [showAllDetails, toggleAllDetails] = useToggle(false);

  const isFiltered = !!publicationId || !!searchTerm || !!themeId;

  const filteredByString = compact([
    searchTerm,
    selectedTheme?.title,
    selectedPublication?.title,
  ]).join(', ');

  const updateQueryParams = async (nextQuery: DataCataloguePageQuery) => {
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
        ?.topics.flatMap(topic => topic.publications)
        ?.find(publication => publication.id === publicationId)?.slug;

      const newParams = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: publicationId,
        sortBy,
        query: {
          ...router.query,
          themeId: publicationThemeId,
        },
        ...(releaseId && { releaseId }),
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
    } else if (releaseId) {
      updateQueryParams({ ...omit(router.query, 'releaseId') });
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

  const handleClearFilter = async ({
    filterType,
  }: {
    filterType: DataSetFileFilter | 'all';
  }) => {
    await updateQueryParams({
      ...(filterType === 'all'
        ? omit(router.query, 'page', ...dataSetFileFilters)
        : omit(router.query, getFiltersToRemove(filterType), 'page')),
      sortBy: searchTerm && sortBy === 'relevance' ? 'newest' : sortBy,
    });

    logEvent({
      category: 'Data catalogue',
      action: `Clear ${filterType} filter`,
    });
  };

  const handleDownload = async () => {
    if (!selectedPublication || !selectedRelease) {
      return;
    }
    const fileIds = dataSets.map(dataSet => dataSet.fileId);
    await downloadService.downloadFiles(selectedRelease.id, fileIds);

    logEvent({
      category: 'Data catalogue',
      action: 'Data set file download - all',
      label: `Publication: ${selectedPublication.title}, Release: ${selectedRelease.title}`,
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

  return (
    <Page
      title="Data catalogue"
      breadcrumbLabel="Data catalogue"
      metaTitle={
        totalPages && totalPages > 1
          ? `Data catalogue (page ${page} of ${totalPages})`
          : undefined
      }
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
                <Link to="/glossary">Glossary</Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <hr />
      <div className="govuk-grid-row">
        <div className={`govuk-grid-column-one-third ${styles.desktopFilters}`}>
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
                latestOnly={latestOnly}
                publicationId={publicationId}
                publications={publications}
                releaseId={releaseId}
                releases={releases}
                themeId={themeId}
                themes={themes}
                onChange={handleChangeFilter}
              />
            </LoadingSpinner>
          )}

          {!isMobileMedia && isFiltered && (
            <ButtonText
              onClick={() => handleClearFilter({ filterType: 'all' })}
            >
              Clear filters
            </ButtonText>
          )}
        </div>
        <div className="govuk-grid-column-two-thirds">
          <div className="dfe-border-bottom">
            <h2
              aria-live="polite"
              aria-atomic="true"
              className="govuk-!-margin-bottom-2"
              data-testid="total-results"
            >
              {`${totalResults} ${
                totalResults !== 1 ? 'data sets' : 'data set'
              }`}
            </h2>

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
                  onClick={() => handleClearFilter({ filterType: 'all' })}
                >
                  Clear filters
                </ButtonText>
              )}
            </div>

            {isFiltered && (
              <div className="govuk-!-padding-bottom-2 dfe-flex dfe-flex-wrap ">
                {searchTerm && (
                  <FilterClearButton
                    filterType="Search"
                    name={searchTerm}
                    onClick={() =>
                      handleClearFilter({ filterType: 'searchTerm' })
                    }
                  />
                )}
                {selectedTheme && (
                  <FilterClearButton
                    filterType="Theme"
                    name={selectedTheme.title}
                    onClick={() => handleClearFilter({ filterType: 'themeId' })}
                  />
                )}
                {selectedPublication && (
                  <FilterClearButton
                    filterType="Publication"
                    name={selectedPublication.title}
                    onClick={() =>
                      handleClearFilter({ filterType: 'publicationId' })
                    }
                  />
                )}
                {selectedRelease && (
                  <FilterClearButton
                    filterType="Release"
                    name={selectedRelease.title}
                    onClick={() =>
                      handleClearFilter({ filterType: 'releaseId' })
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
                  latestOnly={latestOnly}
                  publicationId={publicationId}
                  publications={publications}
                  releaseId={releaseId}
                  releases={releases}
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
                      {!releaseId && (
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
                              <div className="dfe-flex dfe-justify-content--space-between dfe-flex-wrap govuk-!-margin-bottom-6">
                                <TagGroup className="govuk-!-margin-right-2">
                                  <Tag>
                                    {releaseTypes[selectedRelease.type]}
                                  </Tag>
                                  <Tag
                                    colour={
                                      selectedRelease.latestRelease
                                        ? undefined
                                        : 'orange'
                                    }
                                  >
                                    {selectedRelease.latestRelease
                                      ? 'This is the latest data'
                                      : 'This is not the latest data'}
                                  </Tag>
                                </TagGroup>

                                <Link
                                  to={`/find-statistics/${selectedPublication.slug}/${selectedRelease.slug}`}
                                >
                                  View this release
                                </Link>
                              </div>

                              <p>
                                <Button
                                  className="govuk-!-margin-bottom-0"
                                  onClick={handleDownload}
                                >{`Download ${
                                  dataSets.length === 1
                                    ? '1 data set'
                                    : `all ${dataSets.length} data sets`
                                } (ZIP)`}</Button>
                              </p>
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
}

function getFiltersToRemove(filterType: DataSetFileFilter | 'all') {
  if (filterType === 'themeId') {
    return ['themeId', 'publicationId', 'releaseId'];
  }
  if (filterType === 'publicationId') {
    return ['publicationId', 'releaseId'];
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
    theme.topics
      .flatMap(topic => topic.publications)
      .find(pub => pub.id === publicationId),
  );
}
