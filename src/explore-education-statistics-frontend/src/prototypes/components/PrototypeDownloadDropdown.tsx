import React from 'react';
import Details from '../../components/Details';
import Link from '../../components/Link';

interface Props {
  viewType?: string;
  topic?: string;
  link?: string;
}

const PrototypeDownloadDropdown = ({ viewType, topic, link }: Props) => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third">
          <Link
            className="govuk-link govuk-!-margin-right-9 "
            to={link ? link : '/prototypes/publication'}
          >
            View statistics
          </Link>
        </div>
        <div className="govuk-grid-column-one-third">
          <Link
            className="govuk-link govuk-!-margin-right-9 "
            to="/prototypes/data-table-v3"
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
    </>
  );
};

export default PrototypeDownloadDropdown;
