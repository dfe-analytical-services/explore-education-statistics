import React from 'react';
import Details from '../../components/Details';
import Link from '../../components/Link';

interface Props {
  viewType?: string;
  topic?: string;
}

const PrototypeDownloadDropdown = ({ viewType, topic }: Props) => {
  return (
    <>
      <details className="govuk-details govuk-!-margin-right-9 govuk-!-display-inline-block">
        <summary className="govuk-details__summary">
          <span className="govuk-details__summary-text">
            Download underlying data files
          </span>
        </summary>
        <div className="govuk-details__text">
          <p>
            Includes authorised, overall, persistent and unauthorised absence
            data
          </p>
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
      <Link className="govuk-link" to="/prototypes/data-table-v1/national">
        Explore statistics and data
      </Link>
    </>
  );
};

export default PrototypeDownloadDropdown;
