import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import useToggle from '@common/hooks/useToggle';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import publicationService, {
  publicationFilters,
  PublicationFilter,
  PublicationListSummary,
  PublicationSortOption,
} from '@common/services/publicationService';
import { ThemeSummary } from '@common/services/themeService';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import { Paging } from '@common/services/types/pagination';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import useRouterLoading from '@frontend/hooks/useRouterLoading';
import FilterClearButton from '@frontend/modules/find-statistics/components/FilterClearButton';
import Filters from '@frontend/modules/find-statistics/components/Filters';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import { FindStatisticsPageQuery } from '@frontend/modules/find-statistics/FindStatisticsPage';
import SearchForm from '@frontend/modules/find-statistics/components/SearchForm';
import SortControls from '@frontend/modules/find-statistics/components/SortControls';
import createPublicationListRequest from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import compact from 'lodash/compact';
import isEqual from 'lodash/isEqual';
import omit from 'lodash/omit';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import { ParsedUrlQuery } from 'querystring';
import React, { useState, useEffect, useRef } from 'react';

interface Props {
  paging: Paging;
  publications: PublicationListSummary[];
  query: FindStatisticsPageQuery;
  themes: ThemeSummary[];
}

const FindStatisticsPageNew: NextPage<Props> = ({
  paging: initialPaging,
  publications: initialPublications,
  query,
  themes,
}) => {
  const router = useRouter();
  const isLoading = useRouterLoading();

  const [currentQuery, setQuery] = useState<FindStatisticsPageQuery>(query);
  const { releaseType, search, sortBy = 'newest', themeId } = currentQuery;

  const [publications, setPublications] = useState<PublicationListSummary[]>(
    initialPublications,
  );
  const [paging, setPaging] = useState<Paging>(initialPaging);
  const { page, totalPages, totalResults } = paging;

  const { isMedia: isMobileMedia } = useMobileMedia();
  const mobileFilterButtonRef = useRef<HTMLButtonElement>(null);
  const [showMobileFilters, toggleMobileFilters] = useToggle(false);

  const isFiltered = !!search || !!releaseType || !!themeId;

  const selectedTheme = themes.find(theme => theme.id === themeId);
  const selectedReleaseType = releaseTypes[releaseType as ReleaseType];

  const filteredByString = compact([
    search,
    selectedTheme?.title,
    selectedReleaseType,
  ]).join(', ');

  useEffect(() => {
    async function fetchPublications(nextQuery: FindStatisticsPageQuery) {
      const updatedPublications = await publicationService.listPublications(
        nextQuery,
      );
      setPublications(updatedPublications.results);
      setPaging(updatedPublications.paging);
    }
    if (
      Object.keys(router.query).length &&
      !isEqual(router.query, currentQuery)
    ) {
      setQuery(router.query);
      fetchPublications(createPublicationListRequest(router.query));
    }
  }, [router.query, currentQuery]);

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

            <Pagination
              currentPage={page}
              scroll
              shallow
              totalPages={totalPages}
            />
          </LoadingSpinner>
        </div>
      </div>
    </Page>
  );
};

export default FindStatisticsPageNew;
