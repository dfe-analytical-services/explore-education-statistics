import classNames from 'classnames';
import React from 'react';

const PrototypeDownloadAncillary = () => {
  return (
    <>
      <h3 className="govuk-heading-m">Ancillary files</h3>
      <p>
        <a
          href="#"
          className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0 govuk-!-margin-right-3"
        >
          Download all as zip
        </a>
      </p>
      <ul className="govuk-list govuk-list--bullet govuk-list--spaced govuk-!-margin-bottom-6">
        <li>
          <a href="#" className="govuk-link">
            File 1
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            File 2
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            File 3
          </a>
        </li>
      </ul>
    </>
  );
};

export default PrototypeDownloadAncillary;
