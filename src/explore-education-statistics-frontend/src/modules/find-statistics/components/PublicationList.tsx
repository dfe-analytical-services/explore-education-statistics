import Details from '@common/components/Details';
import Link from '@frontend/components/Link';
import React from 'react';

export interface Publication {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

interface Props {
  publications: Publication[];
}

function PublicationList({ publications }: Props) {
  return (
    <>
      {publications.length > 0 ? (
        publications.map(({ id, slug, summary, title }) => (
          <li key={id}>
            <h3 className="govuk-heading-m govuk-!-margin-bottom-0">{title}</h3>
            <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-1">
              {summary}
            </p>
            <div className="govuk-!-margin-top-0 govuk-!-margin-bottom-5">
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-third">
                  <Link
                    className="govuk-link govuk-!-margin-right-9"
                    to={`/statistics/publication?publication=${slug}`}
                    as={`/statistics/${slug}`}
                    data-testid={`view-stats-${slug}`}
                  >
                    View statistics
                  </Link>
                </div>
                <div className="govuk-grid-column-one-third">
                  <Link
                    className="govuk-link govuk-!-margin-right-9"
                    to={`/table-tool/${slug}`}
                    data-testid={`create-table-${slug}`}
                  >
                    Create charts and tables
                  </Link>
                </div>
                <div className="govuk-grid-column-one-third">
                  <Details summary="Download underlying data files">
                    <ul className="govuk-list-bullet">
                      <li>
                        <a href="#" className="govuk-link">
                          Download Excel files
                        </a>
                      </li>
                      <li>
                        <a href="#" className="govuk-link">
                          Download .csv files
                        </a>
                      </li>
                      <li>
                        <a href="#" className="govuk-link">
                          Access API
                        </a>
                      </li>
                    </ul>
                  </Details>
                </div>
              </div>
            </div>
          </li>
        ))
      ) : (
        <div className="govuk-inset-text">
          No publications currently released.
        </div>
      )}
    </>
  );
}

export default PublicationList;
