import React from 'react';
import Details from '../../components/Details';

const PrototypeDownloadDropdown = () => (
  <Details summary="Explore and download data files">
    <ul className="govuk-list">
      <li>
        <a href="#" className="govuk-link">
          Explore data
        </a>
      </li>
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
);

export default PrototypeDownloadDropdown;
