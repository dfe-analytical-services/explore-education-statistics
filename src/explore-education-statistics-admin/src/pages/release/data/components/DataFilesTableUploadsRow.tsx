import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import releaseDataFileService, {
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import React, { useCallback, useMemo, useState } from 'react';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { useQuery } from '@tanstack/react-query';
import DataSetUploadSummaryList from './DataSetUploadSummaryList';
import dataSetUploadTabIds from '../utils/dataSetUploadTabIds';
import ScreenerResultsTable from './ScreenerResultsTable';
import styles from './DataFilesTable.module.scss';
import ScreenerStatus, { terminalScreeningStatuses } from './ScreenerStatus';

interface Props {
  canUpdateRelease?: boolean;
  dataSetUpload: DataSetUpload;
  releaseVersionId: string;
  onConfirmDelete: (deletedUploadId: string) => void;
  onConfirmImport: (uploadIds: string[]) => void;
  onRefreshUploads: () => void;
  testId?: string;
}

export default function DataFilesTableUploadRow({
  canUpdateRelease,
  dataSetUpload,
  releaseVersionId,
  onConfirmDelete,
  onConfirmImport,
  onRefreshUploads,
  testId,
}: Props) {
  const [openImportConfirm, toggleOpenImportConfirm] = useToggle(false);
  const [openDeleteConfirm, toggleOpenDeleteConfirm] = useToggle(false);
  const { user } = useAuthContext();

  // The screener updates this upload's status out-of-band, so poll for it
  // rather than relying on the status the list was fetched with.
  const { data: screenerProgress } = useQuery({
    ...releaseDataFileQueries.screeningStatus(
      releaseVersionId,
      dataSetUpload.id,
    ),
    // No point polling an upload that has already finished screening.
    enabled: !terminalScreeningStatuses.includes(dataSetUpload.screeningStatus),
    // Poll every 5 seconds until screening reaches a status it can't move on from.
    refetchInterval: progress =>
      progress && terminalScreeningStatuses.includes(progress.status)
        ? false
        : 5000,
    onSuccess: progress => {
      // Refresh the list so the row picks up the screener results that come
      // with the finished upload.
      if (
        progress.status !== dataSetUpload.screeningStatus &&
        terminalScreeningStatuses.includes(progress.status)
      ) {
        onRefreshUploads();
      }
    },
  });

  const screeningStatus =
    screenerProgress?.status ?? dataSetUpload.screeningStatus;

  const hasFailures = dataSetUpload.screenerResult?.testResults.some(
    testResult => testResult.result === 'FAIL',
  );
  const warnings = useMemo(
    () =>
      dataSetUpload.screenerResult?.testResults.filter(
        testResult => testResult.result === 'WARNING',
      ) ?? [],
    [dataSetUpload.screenerResult],
  );
  const hasWarnings = warnings.length > 0;

  const [acknowledgedWarnings, setAcknowledgedWarnings] = useState<Set<string>>(
    new Set(),
  );

  const canOverride = user?.permissions.isBauUser ?? false;

  const importBlocked =
    !canUpdateRelease ||
    !dataSetUpload.screenerResult ||
    screeningStatus === 'ScreenerError' ||
    screeningStatus === 'FailedScreening' ||
    hasFailures;

  const importUnavailable = warnings.some(
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
        dataSetUpload.id,
      );
      onConfirmDelete(dataSetUpload.id);
    } catch (err) {
      logger.error(err);
    } finally {
      toggleOpenDeleteConfirm.off();
    }
  }, [
    releaseVersionId,
    dataSetUpload.id,
    toggleOpenDeleteConfirm,
    onConfirmDelete,
  ]);

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
    <tr key={dataSetUpload.dataSetTitle}>
      <td
        data-testid={`${dataSetUpload.dataSetTitle}-title`}
        className={styles.title}
      >
        {dataSetUpload.dataSetTitle}
      </td>
      <td
        data-testid={`${dataSetUpload.dataSetTitle}-size`}
        className={styles.fileSize}
      >
        {dataSetUpload.dataFileSize}
      </td>
      <td data-testid={`${dataSetUpload.dataSetTitle}-status`}>
        <ScreenerStatus
          dataSetTitle={dataSetUpload.dataSetTitle}
          percentageComplete={screenerProgress?.percentageComplete ?? 0}
          status={screeningStatus}
        />
      </td>
      <td data-testid={`${dataSetUpload.dataSetTitle}-actions`}>
        <ButtonGroup className={styles.actions}>
          <ModalConfirm
            title="Data set details"
            open={openImportConfirm}
            hideConfirm={
              screeningStatus === 'Screening' || (importBlocked && !canOverride)
            }
            disableConfirm={
              importUnavailable && !(importBlocked && canOverride)
            }
            onConfirm={() => onConfirmImport([dataSetUpload.id])}
            confirmText={confirmText}
            triggerButton={
              <ButtonText
                testId={testId && `${testId}-view-details`}
                onClick={toggleOpenImportConfirm.on}
              >
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
                    hasFailures
                      ? 'Screener test failures'
                      : 'Screener test warnings'
                  }
                >
                  {hasFailures && failuresNoticeMessage}
                  {hasWarnings && !hasFailures && warningsNoticeMessage}
                  <ScreenerResultsTable
                    screenerResult={dataSetUpload.screenerResult}
                    showAll={false}
                    onAcknowledgeWarning={acknowledgeWarning}
                    warningAcknowledgements={warningAcknowledgements}
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
                  {hasFailures && failuresNoticeMessage}
                  {hasWarnings && !hasFailures && warningsNoticeMessage}
                  <ScreenerResultsTable
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
                {hasFailures && failuresNoticeMessage}
                {hasWarnings && !hasFailures && warningsNoticeMessage}
                <DataSetUploadSummaryList
                  releaseVersionId={releaseVersionId}
                  dataSetUpload={dataSetUpload}
                />
              </TabsSection>
            </Tabs>
          </ModalConfirm>
          {screeningStatus !== 'Screening' && canUpdateRelease && (
            <ModalConfirm
              open={openDeleteConfirm}
              title={
                dataSetUpload.replacingFileId
                  ? 'Cancel replacement'
                  : 'Confirm deletion of selected data files'
              }
              triggerButton={
                <ButtonText
                  onClick={toggleOpenDeleteConfirm.on}
                  variant="warning"
                >
                  {dataSetUpload.replacingFileId
                    ? 'Cancel replacement'
                    : 'Delete files'}
                  <VisuallyHidden>{` for ${dataSetUpload.dataSetTitle}`}</VisuallyHidden>
                </ButtonText>
              }
              onConfirm={handleDeleteConfirm}
            >
              {dataSetUpload.replacingFileId ? (
                <p>
                  Are you sure you want to cancel this data replacement? The
                  pending replacement data file will be deleted.
                </p>
              ) : (
                <>
                  <p>
                    Are you sure you want to delete{' '}
                    <strong>{dataSetUpload.dataSetTitle}</strong>?
                  </p>
                  <p>This version of the data set has not yet been imported.</p>
                </>
              )}
            </ModalConfirm>
          )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
