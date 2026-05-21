import releaseDataFileService, {
  DataSetUpload,
  DataSetScreenerProgress,
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import React, { useCallback, useEffect, useState } from 'react';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import { useAuthContext } from '@admin/contexts/AuthContext';
import DataSetUploadSummaryList from './DataSetUploadSummaryList';
import dataSetUploadTabIds from '../utils/dataSetUploadTabIds';
import ScreenerResultsTable from './ScreenerResultsTable';
import styles from './DataFilesTable.module.scss';
import ScreenerStatus from './ScreenerStatus';

interface Props {
  canUpdateRelease?: boolean;
  dataSetUpload: DataSetUpload;
  releaseVersionId: string;
  onConfirmDelete: (deletedUploadId: string) => void;
  onConfirmImport: (uploadIds: string[]) => void;
  testId?: string;
}

export default function DataFilesTableUploadRow({
  canUpdateRelease,
  dataSetUpload,
  releaseVersionId,
  onConfirmDelete,
  onConfirmImport,
  testId,
}: Props) {
  const [openImportConfirm, toggleOpenImportConfirm] = useToggle(false);
  const [openDeleteConfirm, toggleOpenDeleteConfirm] = useToggle(false);
  const { user } = useAuthContext();

  const [currentUpload, setCurrentUpload] =
    useState<DataSetUpload>(dataSetUpload);

  useEffect(() => {
    setCurrentUpload(dataSetUpload);
  }, [dataSetUpload]);

  const handleScreenerStatusChange = useCallback(
    (_upload: DataSetUpload, progress: DataSetScreenerProgress) => {
      setCurrentUpload(upload => ({ ...upload, status: progress.status }));
    },
    [setCurrentUpload],
  );

  const hasFailures = currentUpload.screenerResult?.testResults.some(
    testResult => testResult.result === 'FAIL',
  );
  const hasWarnings = currentUpload.screenerResult?.testResults.some(
    testResult => testResult.result === 'WARNING',
  );

  const [warningAcknowledgements, setWarningAcknowledgements] = useState<
    Dictionary<boolean>
  >({});

  const canOverride = user?.permissions.isBauUser ?? false;

  const importBlocked =
    !canUpdateRelease ||
    !currentUpload.screenerResult ||
    currentUpload.status === 'ScreenerError' ||
    currentUpload.status === 'FailedScreening' ||
    hasFailures;

  const importUnavailable = !Object.values(warningAcknowledgements).every(
    acknowledgement => acknowledgement,
  );

  useEffect(() => {
    const warnings = currentUpload.screenerResult?.testResults.filter(
      testResult => testResult.result === 'WARNING',
    );

    if (warnings) {
      setWarningAcknowledgements(acknowledgements =>
        Object.fromEntries(
          warnings.map(warning => [
            warning.id,
            acknowledgements?.[warning.id] ?? false,
          ]),
        ),
      );
    }
  }, [currentUpload]);

  const acknowledgeWarning = useCallback(
    (key: string, value: boolean) => {
      setWarningAcknowledgements({ ...warningAcknowledgements, [key]: value });
    },
    [setWarningAcknowledgements, warningAcknowledgements],
  );

  let tabTitle = '';

  if (hasFailures && hasWarnings) tabTitle = 'Failures & warnings';
  if (hasFailures && !hasWarnings) tabTitle = 'Failures';
  if (!hasFailures && hasWarnings) tabTitle = 'Warnings';

  const failuresNoticeMessage = (
    <WarningMessage>
      You will need to delete this file (close this window, and select "Delete
      files"), fix the failed tests and upload again. If you have any questions,
      please get in touch with the explore.statistics@education.gov.uk team.
    </WarningMessage>
  );

  const warningsNoticeMessage = (
    <WarningMessage>
      You will need to review each warning before continuing the file upload
    </WarningMessage>
  );

  const handleDeleteConfirm = useCallback(async () => {
    try {
      await releaseDataFileService.deleteDataSetUpload(
        releaseVersionId,
        currentUpload.id,
      );
      onConfirmDelete(currentUpload.id);
    } catch (err) {
      logger.error(err);
    } finally {
      toggleOpenDeleteConfirm.off();
    }
  }, [
    releaseVersionId,
    currentUpload.id,
    toggleOpenDeleteConfirm,
    onConfirmDelete,
  ]);

  let confirmText = hasWarnings
    ? 'Continue import with warnings'
    : 'Continue import';

  if (hasFailures) {
    confirmText = 'Continue import (override failures)';
  }

  if (currentUpload.status === 'ScreenerError') {
    confirmText = 'Continue import (bypass screening)';
  }

  return (
    <tr key={currentUpload.dataSetTitle}>
      <td data-testid="Title" className={styles.title}>
        {currentUpload.dataSetTitle}
      </td>
      <td data-testid="Size" className={styles.fileSize}>
        {currentUpload.dataFileSize}
      </td>
      <td data-testid="Status">
        <ScreenerStatus
          dataSetUpload={currentUpload}
          releaseVersionId={releaseVersionId}
          onStatusChange={handleScreenerStatusChange}
        />
      </td>
      <td data-testid="Actions">
        <ButtonGroup className={styles.actions}>
          <ModalConfirm
            title="Data set details"
            open={openImportConfirm}
            hideConfirm={importBlocked && !canOverride}
            disableConfirm={
              importUnavailable && !(importBlocked && canOverride)
            }
            onConfirm={() => onConfirmImport([currentUpload.id])}
            confirmText={confirmText}
            triggerButton={
              <ButtonText
                testId={testId && `${testId}-view-details`}
                onClick={toggleOpenImportConfirm.on}
              >
                View details
                <VisuallyHidden>{` for ${currentUpload.dataSetTitle}`}</VisuallyHidden>
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
                    hasFailures
                      ? 'Screener test failures'
                      : 'Screener test warnings'
                  }
                >
                  {hasFailures && failuresNoticeMessage}
                  {hasWarnings && !hasFailures && warningsNoticeMessage}
                  <ScreenerResultsTable
                    screenerResult={currentUpload.screenerResult}
                    showAll={false}
                    onAcknowledgeWarning={acknowledgeWarning}
                    warningAcknowledgements={warningAcknowledgements}
                  />
                </TabsSection>
              )}
              {currentUpload.status !== 'Screening' && (
                <TabsSection
                  id={dataSetUploadTabIds.screenerResults}
                  testId={dataSetUploadTabIds.screenerResults}
                  title="All tests"
                  headingTitle={
                    currentUpload.screenerResult &&
                    currentUpload.status !== 'ScreenerError' // this bit is still showing the wrong "All tests" before a page refresh
                      ? `Full breakdown of ${currentUpload.screenerResult?.testResults.length} tests checked against this file`
                      : 'No tests checked against this file'
                  }
                >
                  {hasFailures && failuresNoticeMessage}
                  {hasWarnings && !hasFailures && warningsNoticeMessage}
                  <ScreenerResultsTable
                    screenerResult={currentUpload.screenerResult}
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
                {hasFailures && failuresNoticeMessage}
                {hasWarnings && !hasFailures && warningsNoticeMessage}
                <DataSetUploadSummaryList
                  releaseVersionId={releaseVersionId}
                  dataSetUpload={currentUpload}
                />
              </TabsSection>
            </Tabs>
          </ModalConfirm>
          {canUpdateRelease && (
            <ModalConfirm
              open={openDeleteConfirm}
              title="Confirm deletion of selected data files"
              triggerButton={
                <ButtonText
                  onClick={toggleOpenDeleteConfirm.on}
                  variant="warning"
                >
                  Delete files
                  <VisuallyHidden>{` for ${currentUpload.dataSetTitle}`}</VisuallyHidden>
                </ButtonText>
              }
              onConfirm={handleDeleteConfirm}
            >
              <p>
                Are you sure you want to delete{' '}
                <strong>{currentUpload.dataSetTitle}</strong>?
              </p>
              <p>This version of the data set has not yet been imported.</p>
            </ModalConfirm>
          )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
