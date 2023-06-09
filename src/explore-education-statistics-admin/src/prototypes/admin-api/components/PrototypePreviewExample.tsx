import React, { useState } from 'react';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import Button from '@common/components/Button';
import UrlContainer from '@common/components/UrlContainer';
import ChangelogExample from './PrototypeChangelogExamples';

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
const time = `${today.getHours() + 2}:${
  today.getMinutes() < 10 ? '0' : ''
}${today.getMinutes()}`;

const PrototypePreviewExample = ({ initialVersion }: Props) => {
  const [showToken, setShowToken] = useState(false);
  return (
    <>
      <Tabs id="api-preview-tabs">
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
            {!initialVersion && (
              <ChangelogExample versionUpdate="Minor" showNotes />
            )}
          </>
        </TabsSection>
        <TabsSection title="Data guidance">
          <h5 className="govuk-heading-m">Data guidance</h5>
          <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/">
            Explore education statistics API documentation
          </a>
        </TabsSection>
        <TabsSection title="Generate API data set preview token">
          <div>
            <h5 className="govuk-heading-m">Generate a preview token</h5>
            <p>This API data set is currently staged ready for publishing.</p>
            <p className="govuk-width-container govuk-!-margin-0 govuk-!-margin-bottom-9">
              You can preview the data by generating a token. The token allows
              you to test the API and query data using any tool of your choice.
              The URL can only be used by you, and is valid for 2 hours after
              creation.
            </p>

            {showToken && (
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
                  }}
                >
                  Delete token
                </Button>
              </>
            )}
            {!showToken && (
              <Button
                onClick={() => {
                  setShowToken(true);
                }}
              >
                Generate token
              </Button>
            )}
          </div>
        </TabsSection>
      </Tabs>
    </>
  );
};

export default PrototypePreviewExample;
