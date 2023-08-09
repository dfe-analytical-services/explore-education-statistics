import React, { useState } from 'react';
import classNames from 'classnames';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import UrlContainer from '@common/components/UrlContainer';
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
  const [changelog] = useStorageItem<Changelog>('changelog');
  const [deleteToken, setDeleteToken] = useState(false);

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
                className="govuk-!-margin-bottom-6"
                url=" https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/9eee125b-5538-49b8-aa49-4fda877b5e57"
              />

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

      <TabsSection title="Changelog">
        <>
          {initialVersion && (
            <div className="govuk-width-container govuk-!-margin-0">
              <h5 className="govuk-heading-m">Changelog</h5>
              <p>
                This is the initial version of the API data set, changes will
                only appear on this page when future versions of this data set
                are published.
              </p>
            </div>
          )}
          {!initialVersion && changelog && (
            <ChangelogExample changelog={changelog} />
          )}
        </>
      </TabsSection>
      <TabsSection title="Notifications">
        <div style={{ height: '1200px' }}>
          <h5 className="govuk-heading-m">
            Public notification of upcoming API changes
          </h5>
          <div className="govuk-width-container govuk-!-margin-0">
            <p>
              If you wish to publish a notification of upcoming changes
              describing the changes in this data set please complete the form
              below
            </p>
          </div>
        </div>
      </TabsSection>
    </Tabs>
  );
};

export default PrototypePreviewExample;
