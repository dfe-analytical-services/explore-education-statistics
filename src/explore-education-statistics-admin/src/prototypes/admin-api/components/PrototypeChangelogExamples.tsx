import React from 'react';

interface Props {
  versionUpdate?: string;
  showNotes?: boolean;
}

const PrototypeChangelogExample = ({ showNotes, versionUpdate }: Props) => {
  return (
    <>
      {(versionUpdate === 'Minor' || versionUpdate === '') && (
        <>
          {showNotes && (
            <>
              <h3>Changelog</h3>
              <h4 className="govuk-!-margin-bottom-0">Version notes</h4>
              <p>
                This is a minor update on the previous version, some new
                locations, filters and indicators have been added to the data
                set since the previous release, please see the details in the
                changelog below.
              </p>
            </>
          )}
          <h4 className="govuk-!-margin-bottom-0">New locations</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Leeds</li>
            <li>Doncaster</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">Mapped locations</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Darlington --{'>'} Darlington (new)</li>
            <li>
              Kingston upon Hull, City of --{'>'} Kingston upon Hull, City of
              (new)
            </li>
            <li>Northumberland --{'>'} Nortumberland (new)</li>
            <li>Sheffield--{'>'} Sheffield (new)</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">New filters</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Age 11</li>
            <li>Age 12</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">Mapped filters</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Age 10 --{'>'} Age 10 (new)</li>
            <li>
              Ethnicity Major Asian Total --{'>'} Ethnicity Major Asian Total
              (new)
            </li>
            <li>
              Ethnicity Major Black Total --{'>'} Ethnicity Major Black Total
              (new)
            </li>
            <li>Age 4 and under --{'>'} Age 4 and under (new)</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">New indicators</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Number of authorised other sessions</li>
            <li>Number of authorised reasons sessions</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">Mapped indicators</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>
              Number of authorised holiday sessions --{'>'} Number of authorised
              holiday sessions (new)
            </li>
            <li>
              Authorised absence rate --{'>'} Number of Authorised absence rate
              (new)
            </li>
          </ul>
        </>
      )}
      {versionUpdate === 'Major' && (
        <>
          <h3>Changelog</h3>
          <h4 className="govuk-!-margin-bottom-0">New locations</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Leeds</li>
            <li>Doncaster</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">Mapped locations</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Darlington --{'>'} Darlington (new)</li>
            <li>
              Kingston upon Hull, City of --{'>'} Kingston upon Hull, City of
              (new)
            </li>
            <li>Northumberland --{'>'} Nortumberland (new)</li>
            <li>Sheffield--{'>'} Sheffield (new)</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">New filters</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Age 11</li>
            <li>Age 12</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">**UNMAPPED FILTERS**</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Ethnicity Major Asian Total</li>
            <li>Ethnicity Major Black Total</li>
            <li>Age 4 and under</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">New indicators</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Number of authorised other sessions</li>
            <li>Number of authorised reasons sessions</li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">Mapped indicators</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>
              Authorised absence rate --{'>'} Number of Authorised absence rate
              (new)
            </li>
          </ul>
          <h4 className="govuk-!-margin-bottom-0">**UNMAPPED INDICATORS**</h4>
          <ul className="govuk-!-margin-bottom-6">
            <li>Number of authorised holiday sessions</li>
          </ul>
        </>
      )}
    </>
  );
};

export default PrototypeChangelogExample;
