import dataSetQueries from '@frontend/queries/dataSetQueries';
import GoToTopLink from '@common/components/GoToTopLink';
import React from 'react';
import { useQuery } from '@tanstack/react-query';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import WarningMessage from '@common/components/WarningMessage';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import DataSetSummary from '@frontend/modules/data-catalogue/components/DataSetSummary';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { useRouter } from 'next/router';
import AccordionToggleButton from '@common/components/AccordionToggleButton';
import useToggle from '@common/hooks/useToggle';
import {
  DataSetOrderParam,
  DataSetSortParam,
} from '@frontend/services/dataSetService';

export interface DataCataloguePageQuery {
  page?: number;
  searchTerm?: string;
  sort?: DataSetSortParam;
  orderBy?: DataSetOrderParam;
}

export default function DataCataloguePageNew() {
  const router = useRouter();

  const {
    data: dataSetsData,
    isError,
    isFetching,
    isLoading,
  } = useQuery({
    ...dataSetQueries.list(router.query),
    keepPreviousData: true,
    staleTime: 60000,
  });

  const { paging, results: dataSets = [] } = dataSetsData ?? {};
  const { page, totalPages, totalResults = 0 } = paging ?? {};
  const [showAllDetails, toggleAllDetails] = useToggle(false);

  const isFiltered = false;

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
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third">
          <p>FILTERS</p>
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
                  isFiltered
                    ? 'filtered by: '
                    : 'showing all available data sets'
                }`}

                {/* {isFiltered && (
                  <VisuallyHidden>{filteredByString}</VisuallyHidden>
                )}

                <VisuallyHidden>{` Sorted by ${
                  sortBy === 'title' ? 'A to Z' : sortBy
                }`}</VisuallyHidden> */}
              </p>

              {/* {isFiltered && (
                <ButtonText
                  onClick={() => handleClearFilter({ filterType: 'all' })}
                >
                  Clear all filters
                </ButtonText>
              )} */}
            </div>

            {/* {isFiltered && (
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
            )} */}
          </div>

          {/* {isMobileMedia && (
            <FiltersMobile
              releaseType={releaseType}
              themeId={themeId}
              themes={themes}
              totalResults={totalResults}
              onChange={handleChangeFilter}
            />
          )} */}

          {/* {dataSets.length > 0 && (
            <SortControls
              hasSearch={!!search}
              sortBy={sortBy}
              onChange={handleSortBy}
            />
          )} */}

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
                    <ul
                      className="govuk-list"
                      id="searchResults"
                      data-testid="data-sets-list"
                    >
                      {dataSets.map(dataSet => (
                        <DataSetSummary
                          key={dataSet.fileId}
                          dataSet={dataSet}
                          expanded={showAllDetails}
                        />
                      ))}
                    </ul>
                  </>
                )}
              </>
            )}
            {page && totalPages && (
              <Pagination currentPage={page} shallow totalPages={totalPages} />
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
