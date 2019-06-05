import Details from '@common/components/Details';
import React from 'react';

const DownloadDetails = () => {
  return (
    <Details summary="Download this data">
      <ul className="govuk-list">
        <li>
          <a href="#">Download as .csv</a>
        </li>
        <li>
          <a href="#">Download as .json</a>
        </li>
        <li>
          <a href="#">Download as Excel</a>
        </li>
        <li>
          <a href="#">Access API</a> - <a href="#">What is an API?</a>
        </li>
      </ul>
    </Details>
  );
};

export default DownloadDetails;
