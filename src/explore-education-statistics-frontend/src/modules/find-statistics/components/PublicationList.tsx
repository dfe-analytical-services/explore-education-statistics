import Link from '@frontend/components/Link';
import React, { Component } from 'react';

export interface Publication {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

interface Props {
  publications: Publication[];
}

class PublicationList extends Component<Props> {
  public render() {
    const { publications } = this.props;

    return (
      <>
        {publications.length > 0 ? (
          publications.map(({ id, slug, summary, title }) => (
            <li key={id}>
              <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
                {title}
              </h3>
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
                    <details className="govuk-details govuk-!-display-inline-block">
                      <summary className="govuk-details__summary">
                        <span className="govuk-details__summary-text">
                          Download underlying data files
                        </span>
                      </summary>
                      <div className="govuk-details__text">
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
                      </div>
                    </details>
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
}

export default PublicationList;
