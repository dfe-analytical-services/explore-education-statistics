import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import { PublicationSummaryWithRelease } from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import Page from '@frontend/components/Page';
import Pagination from '@frontend/components/Pagination';
import useRouterLoading from '@frontend/hooks/useRouterLoading';
import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import { NextPage } from 'next';
import React, { useEffect, useState } from 'react';

interface Props {
  paging: Paging;
  publications: PublicationSummaryWithRelease[];
}

const FindStatisticsPageNew: NextPage<Props> = ({ paging, publications }) => {
  // TODO EES-3517 - update totalResults when search / filter
  const { totalResults } = paging;
  // const [totalResults, setTotalResults] = useState<number>(paging.totalResults);
  const [currentPage, setCurrentPage] = useState<number>(paging.page);
  const [currentPublications, setCurrentPublications] = useState<
    PublicationSummaryWithRelease[]
  >(publications);

  useEffect(() => {
    setCurrentPage(paging.page);
    setCurrentPublications(publications);
  }, [paging.page, publications]);

  const isLoading = useRouterLoading();

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
            <LoadingSpinner loading={isLoading}>
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

              {/* TODO EES-3517 show different message if search / filter returns no results  */}
              {currentPublications.length === 0 ? (
                <div className="govuk-inset-text" id="searchResults">
                  No data currently published.
                </div>
              ) : (
                <ul className="govuk-list" id="searchResults">
                  {currentPublications.map(publication => (
                    <PublicationSummary
                      key={publication.id}
                      publication={publication}
                    />
                  ))}
                </ul>
              )}

              <Pagination
                baseUrl="/find-statistics"
                queryParams={{ newDesign: true }} // TO DO EES-3517 make sure params for filters / search are included here
                currentPage={currentPage ?? 1}
                totalPages={paging.totalPages}
              />
            </LoadingSpinner>
          </div>
        </div>
      </Page>
    </>
  );
};

export default FindStatisticsPageNew;
