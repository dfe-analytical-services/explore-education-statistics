import Pagination from '@common/components/Pagination';
import RelatedInformation from '@common/components/RelatedInformation';
import { PublicationSummaryWithRelease } from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { NextPage } from 'next';
import React, { useEffect, useRef, useState } from 'react';
import { useRouter } from 'next/router';

// TODO EES-3517 some fake paging data
const tempPaging = {
  page: 1,
  pageSize: 10,
  totalResults: 100,
  totalPages: 10,
};

interface Props {
  paging?: {
    // won't be optional
    page: number;
    pageSize: number;
    totalResults: number;
    totalPages: number;
  };
  publications?: PublicationSummaryWithRelease[]; // won't be optional
}

const FindStatisticsPageNew: NextPage<Props> = ({
  paging = tempPaging,
  publications = testPublications,
}) => {
  const router = useRouter();
  const resultsRef = useRef<HTMLDivElement>(null);

  // TODO EES-3517 - update totalResults when search / filter
  const { totalResults } = paging;
  // const [totalResults, setTotalResults] = useState<number>(paging.totalResults);
  const [currentPage, setCurrentPage] = useState<number>(paging.page);
  const [currentPublications, setCurrentPublications] = useState<
    PublicationSummaryWithRelease[]
  >(publications);

  useEffect(() => {
    // TODO EES-3517 - fetch selected page of publications here,
    // something like this:
    // async function fetchPublications(params) {
    //   const nextPublications = await publicationService.getPublications(params);
    //   setCurrentPublications(nextPublications);
    // }
    const queryPage =
      typeof router.query.page === 'string'
        ? parseInt(router.query.page, 10)
        : null;

    if (queryPage && queryPage !== currentPage) {
      setCurrentPage(queryPage);

      // fetchPublications({page: queryPage})

      resultsRef.current?.focus(); // Set focus to top of the results
    }
  }, [currentPage, router.query.page]);

  return (
    <>
      <Page
        metaTitle={
          paging.totalPages > 1
            ? `Find statistics and data (page ${currentPage} of ${paging.totalPages})`
            : undefined
        }
        title="Find statistics and data"
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <p className="govuk-body-l">
              Search and browse statistical summaries and download associated
              data to help you understand and analyse our range of statistics.
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
            Search and filters here
          </div>
          <div className="govuk-grid-column-two-thirds">
            <h2
              className="govuk-!-margin-bottom-2"
              aria-live="polite"
              aria-atomic="true"
            >
              {totalResults} {totalResults !== 1 ? 'results' : 'result'}
            </h2>

            <a href="#searchResults" className="govuk-skip-link ">
              Skip to search results
            </a>
            <p
              aria-live="polite"
              aria-atomic="true"
              className="govuk-!-margin-top-1"
            >{`Page ${currentPage} of ${paging.totalPages}, showing all publications`}</p>

            {/* TODO EES-3517
             <p className="govuk-visually-hidden">
              Sorted by newest publications
            </p>
            <p>Current filters here</p>
            <p>Sorting controls here</p>*/}

            <hr />

            <div
              className="govuk-visually-hidden"
              id="searchResults"
              ref={resultsRef}
              tabIndex={-1}
            >
              Search results
            </div>

            {/* TODO EES-3517 show different message if search / filter returns no results  */}
            {currentPublications.length === 0 ? (
              <div className="govuk-inset-text">
                No data currently published.
              </div>
            ) : (
              <>
                {currentPublications.map(publication => (
                  <PublicationSummary
                    key={publication.id}
                    publication={publication}
                  />
                ))}
              </>
            )}

            <Pagination
              currentPage={currentPage ?? 1}
              nextPrevLinkRenderer={({
                children,
                className,
                pageNumber,
                rel,
              }) => (
                <Link
                  className={className}
                  rel={rel}
                  to={`/find-statistics?page=${pageNumber}&newDesign=true`}
                >
                  {children}
                </Link>
              )}
              pageLinkRenderer={({
                ariaCurrent,
                ariaLabel,
                className,
                pageNumber,
              }) => (
                <Link
                  aria-current={ariaCurrent}
                  aria-label={ariaLabel}
                  className={className}
                  to={`/find-statistics?page=${pageNumber}&newDesign=true`}
                >
                  {pageNumber}
                </Link>
              )}
              totalPages={paging.totalPages}
            />
          </div>
        </div>
      </Page>
    </>
  );
};

export default FindStatisticsPageNew;
