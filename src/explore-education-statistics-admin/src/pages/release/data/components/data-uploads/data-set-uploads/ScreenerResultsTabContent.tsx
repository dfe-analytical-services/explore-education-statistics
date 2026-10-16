import { ScreenerResult } from '@admin/services/releaseDataFileService';
import { Dictionary } from '@common/types';
import React from 'react';
import ScreenerNoticeMessage from './ScreenerNoticeMessage';
import ScreenerResultsTable from './ScreenerResultsTable';

interface Props {
  hasFailures: boolean;
  hasWarnings: boolean;
  screenerResult?: ScreenerResult;
  showAll: boolean;
  warningAcknowledgements?: Dictionary<boolean>;
  onAcknowledgeWarning?: (key: string, value: boolean) => void;
}

/**
 * The body shared by the two screener tabs in the data set upload details
 * modal. The `TabsSection` elements themselves stay in the modal, because
 * `Tabs` matches its children by strict component identity and silently
 * discards anything that is not a `TabsSection`.
 */
export default function ScreenerResultsTabContent({
  hasFailures,
  hasWarnings,
  screenerResult,
  showAll,
  warningAcknowledgements,
  onAcknowledgeWarning,
}: Props) {
  return (
    <>
      <ScreenerNoticeMessage
        hasFailures={hasFailures}
        hasWarnings={hasWarnings}
      />
      <ScreenerResultsTable
        screenerResult={screenerResult}
        showAll={showAll}
        warningAcknowledgements={warningAcknowledgements}
        onAcknowledgeWarning={onAcknowledgeWarning}
      />
    </>
  );
}
