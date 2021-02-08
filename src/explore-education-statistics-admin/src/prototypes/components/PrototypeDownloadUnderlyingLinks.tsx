import React from 'react';
import PrototypeMoreDetails from './PrototypeMoreDetails';

const PrototypeDownloadUnderlyingLinks = () => {
  return (
    <>
      <h4 className="govuk-heading-s">Local authority (LA)</h4>
      <ul className="govuk-list">
        <li>
          <PrototypeMoreDetails title="Accomodation of care leavers" />
        </li>
        <li>
          <PrototypeMoreDetails title="Activity of care leavers" />
        </li>
        <li>
          <PrototypeMoreDetails title="Care leavers by whether their accommodation is suitable" />
        </li>
        <li>
          <PrototypeMoreDetails title="Care leavers in contact with local authority" />
        </li>
        <li>
          <PrototypeMoreDetails
            title="Care leavers who left foster care aged 18 and who were eligible for
            care leaver support"
          />
        </li>
      </ul>
      <h4 className="govuk-heading-s">National</h4>
      <ul className="govuk-list govuk-!-margin-bottom-6">
        <li>
          <PrototypeMoreDetails title="Accomodation of care leavers" />
        </li>
        <li>
          <PrototypeMoreDetails title="Activity of care leavers" />
        </li>
        <li>
          <PrototypeMoreDetails title="Care leavers by whether their accommodation is suitable" />
        </li>
        <li>
          <PrototypeMoreDetails title="Care leavers in contact with local authority" />
        </li>
        <li>
          <PrototypeMoreDetails
            title="Care leavers who left foster care aged 18 and who were eligible for
            care leaver support"
          />
        </li>
      </ul>
    </>
  );
};

export default PrototypeDownloadUnderlyingLinks;
