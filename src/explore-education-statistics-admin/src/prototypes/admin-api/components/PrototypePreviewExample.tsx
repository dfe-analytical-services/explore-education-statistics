import React, { useState } from 'react';
import classNames from 'classnames';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonLink from '@common/components/ButtonLink';
import ButtonText from '@common/components/ButtonText';
import UrlContainer from '@common/components/UrlContainer';
import Details from '@common/components/Details';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import useStorageItem from '@common/hooks/useStorageItem';
import ChangelogExample from './PrototypeChangelogExamples';
import { Changelog } from '../contexts/PrototypeNextSubjectContext';

export const apiPreviewTabIds = {
  dataGuidance: 'data-guidance',
  changeLog: 'change-log',
  generateToken: 'generate-token',
};

interface Props {
  initialVersion?: boolean;
  // versionUpdate?: string;
}

const today = new Date();
const monthNames = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
];

const timeNow = `${today.getHours()}:${
  today.getMinutes() < 10 ? '0' : ''
}${today.getMinutes()}`;

const time = `${today.getHours() + 2}:${
  today.getMinutes() < 10 ? '0' : ''
}${today.getMinutes()}`;

const date = `${today.getDate() < 10 ? '0' : ''} ${today.getDate()}  ${
  monthNames[today.getMonth()]
}
 ${today.getFullYear()}`;

const PrototypePreviewExample = ({ initialVersion }: Props) => {
  const [tokenTerms, setTokenTerms] = useState(false);
  const [showToken, setShowToken] = useState(false);
  const [deleteToken, setDeleteToken] = useState(false);
  const [fullTable, setFullTable] = useState(false);
  const [screenReaderMessage, setScreenReaderMessage] = useState('');

  const apiTokenUrl =
    'https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/9eee125b-5538-49b8-aa49-4fda877b5e57';

  const handleCopyClick = () => {
    navigator.clipboard.writeText(apiTokenUrl);
    setScreenReaderMessage('Link copied to the clipboard.');
  };

  return (
    <Tabs id="api-preview-tabs">
      <TabsSection title="Preview API data set">
        <div>
          <h5 className="govuk-heading-m">
            Generate API data set preview token
          </h5>
          <div className="govuk-width-container govuk-!-margin-0 govuk-!-margin-bottom-9">
            <p>This API data set is currently staged ready for publishing.</p>
            <p>
              You can preview the data by generating a token. The token allows
              you to test the API and query data using any tool of your choice.
              The URL can only be used by you, and is valid for 2 hours after
              creation.
            </p>
            <p>
              <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/">
                View API documentation
              </a>
            </p>
          </div>
          {!showToken && (
            <div className="govuk-form-group">
              <fieldset className="govuk-fieldset">
                <legend>
                  <h5 className="govuk-heading-m">Terms of usage</h5>
                </legend>
                <div className="govuk-checkboxes">
                  <div className="govuk-checkboxes__item">
                    <input
                      type="checkbox"
                      className="govuk-checkboxes__input"
                      id="tokenAgreeTerms"
                      onClick={() => {
                        setTokenTerms(!tokenTerms);
                      }}
                    />{' '}
                    <label
                      htmlFor="tokenAgreeTerms"
                      className="govuk-label govuk-checkboxes__label"
                    >
                      I agree not to share this token with anyone outside of the
                      department
                    </label>
                  </div>
                </div>
              </fieldset>
            </div>
          )}

          {showToken && tokenTerms && (
            <>
              <h5 className="govuk-heading-m">API preview token</h5>
              <p>
                Token expiry time: <strong>Today at {time}</strong>
              </p>

              <UrlContainer
                className="govuk-!-margin-bottom-1"
                url={apiTokenUrl}
              />
              <ButtonGroup>
                <Button variant="secondary" onClick={handleCopyClick}>
                  Copy preview link
                </Button>
              </ButtonGroup>

              <p>
                Please delete the token as soon as you have finished checking
                the API data set.
              </p>

              <Button
                variant="warning"
                onClick={() => {
                  setShowToken(false);
                  setTokenTerms(false);
                }}
              >
                Delete token
              </Button>
            </>
          )}
          {!showToken && (
            <Button
              disabled={!tokenTerms}
              onClick={() => {
                setShowToken(true);
                setDeleteToken(false);
              }}
            >
              Generate token
            </Button>
          )}
        </div>
      </TabsSection>

      <TabsSection title="API token log">
        <>
          <table>
            <caption>
              <h5 className="govuk-heading-m">API token log</h5>
            </caption>
            <thead>
              <tr>
                <th>Generated by</th>
                <th>Date generated</th>
                <th>Status</th>
                <th>Expiry / deletion date</th>
                <th className="govuk-table__header--numeric">Actions</th>
              </tr>
            </thead>
            <tbody>
              {showToken && (
                <tr>
                  <td>John Smith</td>
                  <td>
                    {date}, {timeNow}
                  </td>
                  <td>
                    <span
                      className={classNames('govuk-tag', [
                        deleteToken && 'govuk-tag--red',
                      ])}
                    >
                      {deleteToken ? `Deleted` : 'Active'}
                    </span>
                  </td>
                  <td>
                    {deleteToken && (
                      <>
                        {' '}
                        {date}, {timeNow}
                      </>
                    )}
                  </td>
                  <td className="govuk-table__header--numeric">
                    {!deleteToken ? (
                      <ButtonText
                        variant="warning"
                        onClick={_ => {
                          setDeleteToken(true);
                        }}
                      >
                        Delete token
                      </ButtonText>
                    ) : (
                      <>N/A</>
                    )}
                  </td>
                </tr>
              )}
              <tr>
                <td>John Smith</td>
                <td>
                  <span>22 June 2023, 14:30</span>
                </td>
                <td>
                  <span className="govuk-tag govuk-tag--grey">Expired</span>
                </td>
                <td>
                  <span>22 June 2023, 16:30</span>
                </td>
                <td className="govuk-table__header--numeric">N/A</td>
              </tr>
              <tr>
                <td>John Smith</td>
                <td>
                  <span>20 June 2023, 10:25</span>
                </td>
                <td>
                  <span className="govuk-tag govuk-tag--grey">Expired</span>
                </td>
                <td>20 June 2023, 12:25</td>
                <td className="govuk-table__header--numeric">N/A</td>
              </tr>
              <tr>
                <td>John Smith</td>
                <td>
                  <span>16 June 2023, 11:27</span>
                </td>
                <td>
                  <span className="govuk-tag govuk-tag--grey">Expired</span>
                </td>
                <td>
                  <span>16 June 2023, 13:27</span>
                </td>
                <td className="govuk-table__header--numeric">N/A</td>
              </tr>
            </tbody>
          </table>
          <a href="#">View next 10 entries</a>
        </>
      </TabsSection>

      <TabsSection title="Data set snapshot">
        <h5 className="govuk-heading-m" id="dataPreview">
          Data set snapshot
        </h5>
        <div className="govuk-!-margin-bottom-6">
          <p className="govuk-hint">
            Snapshot showing {fullTable ? 'first 5 rows' : 'first row'} of
            XXXXX, taken from underlying data
          </p>
          <a
            href="#"
            onClick={e => {
              e.preventDefault();
              setFullTable(!fullTable);
            }}
          >
            {fullTable ? 'Show first row only' : 'Show more rows'}
          </a>
        </div>
        <div
          style={{
            maxWidth: '100%',
            overflow: 'auto',
            marginBottom: '2rem',
          }}
        >
          <table className="govuk-table" style={{ width: '3500px' }}>
            <caption
              className="govuk-!-margin-bottom-3 govuk-visually-hidden"
              style={{ fontWeight: 'normal' }}
            >
              Snapshot showing {fullTable ? 'first 5 rows' : 'first row'} of
              XXXXX, taken from underlying data
            </caption>
            <thead>
              <tr>
                <th>time_period</th>
                <th>time_identifier</th>
                <th>geographic_level</th>
                <th>country_code</th>
                <th>country_name</th>
                <th>group</th>
                <th>standard</th>
                <th>age</th>
                <th>apprenticeship level</th>
                <th>demographic</th>
                <th>sector_subject_area</th>
                <th>overall_leavers</th>
                <th>overall_achievers</th>
                <th>overall_completers</th>
                <th>overall_acheievement_rate</th>
                <th>overall_retention_rate</th>
                <th>overall_pass_rate</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>201819</td>
                <td>Academic year</td>
                <td>National</td>
                <td>E92000001</td>
                <td>England</td>
                <td>Ethnicity group</td>
                <td>Framework</td>
                <td>16-18</td>
                <td>Advanced</td>
                <td>Ethnic minorities (excluding white minorities)</td>
                <td>Agriculture, Horticulture and Animal Care</td>
                <td>10</td>
                <td>~</td>
                <td>~</td>
                <td>~</td>
                <td>~</td>
                <td>~</td>
              </tr>
              {fullTable && (
                <>
                  <tr>
                    <td>201819</td>
                    <td>Academic year</td>
                    <td>National</td>
                    <td>E92000001</td>
                    <td>England</td>
                    <td>Ethnicity group</td>
                    <td>Framework</td>
                    <td>16-18</td>
                    <td>Advanced</td>
                    <td>Ethnic minorities (excluding white minorities)</td>
                    <td>Arts, Media and Publishing</td>
                    <td>10</td>
                    <td>10</td>
                    <td>10</td>
                    <td>58.3</td>
                    <td>75</td>
                    <td>77.8</td>
                  </tr>
                  <tr>
                    <td>201819</td>
                    <td>Academic year</td>
                    <td>National</td>
                    <td>E92000001</td>
                    <td>England</td>
                    <td>Ethnicity group</td>
                    <td>Framework</td>
                    <td>16-18</td>
                    <td>Advanced</td>
                    <td>Ethnic minorities (excluding white minorities)</td>
                    <td>Business, Administration and Law</td>
                    <td>440</td>
                    <td>300</td>
                    <td>300</td>
                    <td>67.6</td>
                    <td>68.2</td>
                    <td>99</td>
                  </tr>
                  <tr>
                    <td>201819</td>
                    <td>Academic year</td>
                    <td>National</td>
                    <td>E92000001</td>
                    <td>England</td>
                    <td>Ethnicity group</td>
                    <td>Framework</td>
                    <td>16-18</td>
                    <td>Advanced</td>
                    <td>Ethnic minorities (excluding white minorities)</td>
                    <td>Construction, Planning and the Built Environment</td>
                    <td>60</td>
                    <td>40</td>
                    <td>40</td>
                    <td>74.5</td>
                    <td>76.4</td>
                    <td>97.6</td>
                  </tr>
                  <tr>
                    <td>201819</td>
                    <td>Academic year</td>
                    <td>National</td>
                    <td>E92000001</td>
                    <td>England</td>
                    <td>Ethnicity group</td>
                    <td>Framework</td>
                    <td>16-18</td>
                    <td>Advanced</td>
                    <td>Ethnic minorities (excluding white minorities)</td>
                    <td>Education and Training</td>
                    <td>50</td>
                    <td>30</td>
                    <td>30</td>
                    <td>62.5</td>
                    <td>70.8</td>
                    <td>88.2</td>
                  </tr>
                </>
              )}
            </tbody>
          </table>
        </div>
        <Details summary="Variable names and descriptions">
          <h3 className="govuk-heading-m" id="dataPreview">
            Variable names and descriptions
          </h3>

          <table>
            <thead>
              <tr>
                <th>Variable name</th>
                <th>Variable description </th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>age</td>
                <td>Age Group</td>
              </tr>

              <tr>
                <td>apprenticeship_level</td>
                <td>Apprenticeship level</td>
              </tr>

              <tr>
                <td>demographic</td>
                <td>Demographic</td>
              </tr>
              <tr>
                <td>group</td>
                <td>Group</td>
              </tr>
              <tr>
                <td>overall_achievement_rate</td>
                <td>Achievement Rate</td>
              </tr>
              <tr>
                <td>overall_achievers</td>
                <td>Achievers</td>
              </tr>
              <tr>
                <td>overall_completers</td>
                <td>Completers</td>
              </tr>
              <tr>
                <td>overall_leavers</td>
                <td>Leavers</td>
              </tr>
              <tr>
                <td>overall_pass_rate</td>
                <td>Pass Rate</td>
              </tr>
              <tr>
                <td>overall_retention_rate</td>
                <td>Retention Rate</td>
              </tr>
              <tr>
                <td>sector_subject_area</td>
                <td>Subject area (tier 1)</td>
              </tr>
              <tr>
                <td>standard</td>
                <td>Standard/ Framework Flag</td>
              </tr>
            </tbody>
          </table>
        </Details>
        <Details summary="Footnotes">
          <h3 className="govuk-heading-m" id="dataPreview">
            Footnotes
          </h3>
          <ul className="govuk-list govuk-list--number">
            <li>
              Retention rates are based on the individual aims that were
              successfully completed in the relevant year (the Hybrid End Year).
              They are calculated as the number of learning aims completed
              divided by the number of leavers.
            </li>
            <li>
              Further guidance for the Apprenticeships National Achievement Rate
              Tables can be found on the gov.uk website:
              https://www.gov.uk/government/collections/sfa-national-success-rates-tables
            </li>
            <li>
              Volumes are rounded to the nearest 10 and 'low' indicates a base
              value of fewer than 5. Where data shows 'x' this indicates data is
              unavailable, 'z' indicates data is not applicable, and 'c'
              indicates data is suppressed.
            </li>
            <li>
              Sector Subject Area (SSA) codes are available on the qualification
              description page:
              https://www.gov.uk/government/publications/types-of-regulated-qualifications/qualification-descriptions
            </li>
            <li>
              Achievement rates are based on the individual apprenticeship
              programmes that were completed in the relevant year (Hybrid End
              Year). They are calculated as the number of programme aims
              achieved divided by the number started, excluding the programme
              aims of any learners that transferred onto another qualification
              within the same institution.
            </li>
            <li>
              Figures include all funded and unfunded apprenticeship programmes
              reported on the ILR.
            </li>
            <li>
              Pass rates are based on the individual aims that were successfully
              completed in the relevant year (the Hybrid End Year). They are
              calculated as the number of learning aims achieved divided by the
              number successfully completed.
            </li>
            <li>
              The data source for all tables is the apprenticeships achievement
              rate dataset.
            </li>
            <li>
              The overall achievement rate is based on the Hybrid End Year. The
              Hybrid End Year is the later of the Achievement Year, Expected End
              Year, Actual End Year or Reporting Year of a programme.
            </li>
            <li>
              Percentages are rounded to one decimal place and calculated on
              unrounded volumes. 'low' indicates a percentage below 0.5%. Where
              data shows 'x' this indicates data is unavailable, 'z' indicates
              data is not applicable, and 'c' indicates data is suppressed.
            </li>
            <li>
              Age, sex, learners with learning difficulties and/or disabilities
              and ethnicity are based upon self-declaration by the learner.
            </li>
            <li>
              Achievement rates are calculated according to the Apprenticeship
              Qualification Achievement Rate business rules. These documents are
              available on the gov.uk website:
              https://www.gov.uk/government/collections/qualification-achievement-rates-and-minimum-standards
            </li>
          </ul>
        </Details>
      </TabsSection>
    </Tabs>
  );
};

export default PrototypePreviewExample;
