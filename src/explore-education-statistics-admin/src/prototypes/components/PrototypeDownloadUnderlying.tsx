import classNames from 'classnames';
import React from 'react';

const PrototypeDownloadUnderlying = () => {
  return (
    <>
      <h3 className="govuk-heading-m">Underlying data</h3>
      <p>
        <a
          href="#"
          className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0 govuk-!-margin-right-3"
        >
          Download all as csv
        </a>
        <a
          href="#"
          className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
        >
          Download all as ods
        </a>
      </p>
      <h4 className="govuk-heading-s">Local authority (LA)</h4>
      <ul className="govuk-list govuk-list--bullet govuk-list--spaced">
        <li>
          <a href="#" className="govuk-link">
            Accommodation of care leavers
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Activity of care leavers{' '}
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Care leavers by whether their accommodation is suitable
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Care leavers in contact with local authority
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Care leavers who left foster care aged 18 and who were eligible for
            care leaver support{' '}
          </a>
        </li>
      </ul>
      <h4 className="govuk-heading-s">National</h4>
      <ul className="govuk-list govuk-list--bullet govuk-list--spaced govuk-!-margin-bottom-6">
        <li>
          <a href="#" className="govuk-link">
            Accommodation of care leavers
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Activity of care leavers{' '}
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Care leavers by whether their accommodation is suitable
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Care leavers in contact with local authority
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            Care leavers who left foster care aged 18 and who were eligible for
            care leaver support{' '}
          </a>
        </li>
      </ul>
    </>
  );
};

export default PrototypeDownloadUnderlying;
