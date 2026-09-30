import DataUploadsPermissions from '@admin/pages/release/data/types/dataUploadsPermissions';
import {
  DataSetUpload,
  DataSetUploadScreeningStatus,
} from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useCallback, useMemo, useState } from 'react';
import dataSetUploadTabIds from '@admin/pages/release/data/utils/dataSetUploadTabIds';
import DataSetUploadSummaryList from './DataSetUploadSummaryList';
import ScreenerNoticeMessage from './ScreenerNoticeMessage';
import ScreenerResultsTabContent from './ScreenerResultsTabContent';

interface Props {
  dataSetUpload: DataSetUpload;
  permissions: DataUploadsPermissions;
  releaseVersionId: string;
  screeningStatus: DataSetUploadScreeningStatus;
  testId?: string;
  onImportDataSets: (uploadIds: string[]) => void;
}

export default function DataSetUploadDetailsModal({
  dataSetUpload,
  permissions,
  releaseVersionId,
  screeningStatus,
  testId,
  onImportDataSets,
}: Props) {
  const hasFailures =
    dataSetUpload.screenerResult?.testResults.some(
      testResult => testResult.result === 'FAIL',
    ) ?? false;

  const warnings = useMemo(
    () =>
      dataSetUpload.screenerResult?.testResults.filter(
        testResult => testResult.result === 'WARNING',
      ) ?? [],
    [dataSetUpload.screenerResult],
  );
  const hasWarnings = warnings.length > 0;

  // Held here rather than inside the modal body: `ModalConfirm` renders its
  // children in a portal that unmounts on close, so state kept in there would
  // reset every time the user reopens the modal.
  const [acknowledgedWarnings, setAcknowledgedWarnings] = useState<Set<string>>(
    new Set(),
  );

  const { canOverrideScreenerResult: canOverride } = permissions;

  const screenerBlocksImport =
    !permissions.canUpdateRelease ||
    !dataSetUpload.screenerResult ||
    screeningStatus === 'ScreenerError' ||
    screeningStatus === 'FailedScreening' ||
    hasFailures;

  const hasUnacknowledgedWarnings = warnings.some(
    warning => !acknowledgedWarnings.has(warning.id),
  );

  const warningAcknowledgements = useMemo(
    () =>
      Object.fromEntries(
        warnings.map(warning => [
          warning.id,
          acknowledgedWarnings.has(warning.id),
        ]),
      ),
    [warnings, acknowledgedWarnings],
  );

  const acknowledgeWarning = useCallback((key: string, value: boolean) => {
    setAcknowledgedWarnings(acknowledged => {
      const next = new Set(acknowledged);

      if (value) {
        next.add(key);
      } else {
        next.delete(key);
      }

      return next;
    });
  }, []);

  let tabTitle = '';

  if (hasFailures && hasWarnings) tabTitle = 'Failures & warnings';
  if (hasFailures && !hasWarnings) tabTitle = 'Failures';
  if (!hasFailures && hasWarnings) tabTitle = 'Warnings';

  let confirmText = hasWarnings
    ? 'Continue import with warnings'
    : 'Continue import';

  if (hasFailures) {
    confirmText = 'Continue import (override failures)';
  }

  if (screeningStatus === 'ScreenerError') {
    confirmText = 'Continue import (bypass screening)';
  }

  return (
    <ModalConfirm
      title="Data set details"
      hideConfirm={
        screeningStatus === 'Screening' ||
        (screenerBlocksImport && !canOverride)
      }
      disableConfirm={
        hasUnacknowledgedWarnings && !(screenerBlocksImport && canOverride)
      }
      onConfirm={() => onImportDataSets([dataSetUpload.id])}
      confirmText={confirmText}
      triggerButton={
        <ButtonText testId={testId && `${testId}-view-details`}>
          View details
          <VisuallyHidden>{` for ${dataSetUpload.dataSetTitle}`}</VisuallyHidden>
        </ButtonText>
      }
    >
      <Tabs id="data-set-upload-tabs" modifyHash={false}>
        {(hasFailures || hasWarnings) && (
          <TabsSection
            id={dataSetUploadTabIds.screenerFailuresAndWarnings}
            testId={dataSetUploadTabIds.screenerFailuresAndWarnings}
            title={tabTitle}
            headingTitle={
              hasFailures ? 'Screener test failures' : 'Screener test warnings'
            }
          >
            <ScreenerResultsTabContent
              hasFailures={hasFailures}
              hasWarnings={hasWarnings}
              screenerResult={dataSetUpload.screenerResult}
              showAll={false}
              warningAcknowledgements={warningAcknowledgements}
              onAcknowledgeWarning={acknowledgeWarning}
            />
          </TabsSection>
        )}
        {screeningStatus !== 'Screening' && (
          <TabsSection
            id={dataSetUploadTabIds.screenerResults}
            testId={dataSetUploadTabIds.screenerResults}
            title="All tests"
            headingTitle={
              !dataSetUpload.screenerResult &&
              screeningStatus === 'ScreenerError'
                ? 'No tests checked against this file'
                : `Full breakdown of ${dataSetUpload.screenerResult?.testResults.length} tests checked against this file`
            }
          >
            <ScreenerResultsTabContent
              hasFailures={hasFailures}
              hasWarnings={hasWarnings}
              screenerResult={dataSetUpload.screenerResult}
              showAll
            />
          </TabsSection>
        )}
        <TabsSection
          id={dataSetUploadTabIds.fileDetails}
          testId={dataSetUploadTabIds.fileDetails}
          title="File details"
          headingTitle="File details"
        >
          <ScreenerNoticeMessage
            hasFailures={hasFailures}
            hasWarnings={hasWarnings}
          />
          <DataSetUploadSummaryList
            releaseVersionId={releaseVersionId}
            dataSetUpload={dataSetUpload}
          />
        </TabsSection>
      </Tabs>
    </ModalConfirm>
  );
}
