import WarningMessage from '@common/components/WarningMessage';
import React from 'react';

interface Props {
  hasFailures: boolean;
  hasWarnings: boolean;
}

/**
 * The notice shown above every tab of the data set upload details modal.
 * Failures take precedence over warnings.
 */
export default function ScreenerNoticeMessage({
  hasFailures,
  hasWarnings,
}: Props) {
  if (hasFailures) {
    return (
      <WarningMessage>
        You will need to delete this file (close this window, and select "Delete
        files"), fix the failed tests and upload again. If you have any
        questions, please get in touch with the
        explore.statistics@education.gov.uk team.
      </WarningMessage>
    );
  }

  if (hasWarnings) {
    return (
      <WarningMessage>
        You will need to review each warning before continuing the file upload
      </WarningMessage>
    );
  }

  return null;
}
