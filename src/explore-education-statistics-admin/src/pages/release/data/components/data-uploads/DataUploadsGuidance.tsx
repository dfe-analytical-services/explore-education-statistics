import InsetText from '@common/components/InsetText';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';

export default function DataUploadsGuidance() {
  return (
    <>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Data files will be displayed in the table tool and can be used to
          create data blocks. They will also be attached to the release for
          users to download. Please ensure:
        </p>
        <ul>
          <li>
            your data files have passed the checks in our{' '}
            <a
              href="https://rsconnect/rsc/dfe-published-data-qa/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              screening app (opens in new tab)
            </a>
          </li>
          <li>
            your data files meets these standards - if not you won't be able to
            upload it to your release
          </li>
          <li>
            if you have any issues uploading data files, or questions about data
            standards contact:{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </li>
        </ul>
        <h4>Data replacement</h4>
        <p>
          Files are expected to have a unique title, any files that are uploaded
          with a title that matches an existing file will start a data
          replacement instead of importing as a separate file.
        </p>
      </InsetText>
      <WarningMessage>
        The system runs some basic screening checks during data import. Analysts
        should still ensure that all data files undergo the full screening check
        suite prior to being uploaded, as provided by the external screener app.
      </WarningMessage>
    </>
  );
}
