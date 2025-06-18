import releaseDataFileService, {
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import React, { useCallback } from 'react';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import Tag from '@common/components/Tag';
import DataSetUploadSummaryList from './DataSetUploadSummaryList';
import dataSetUploadTabIds from '../utils/dataSetUploadTabIds';
import ScreenerResultsTable from './ScreenerResultsTable';
import styles from './DataFilesTable.module.scss';
import {
  getDataSetUploadStatusColour,
  getDataSetUploadStatusLabel,
} from './ImporterStatus';

interface Props {
  canUpdateRelease?: boolean;
  dataSetUpload: DataSetUpload;
  releaseVersionId: string;
  onConfirmDelete: (deletedUploadId: string) => void;
  onConfirmImport: (uploadIds: string[]) => void;
}

export default function DataFilesTableUploadRow({
  canUpdateRelease,
  dataSetUpload,
  releaseVersionId,
  onConfirmDelete,
  onConfirmImport,
}: Props) {
  const [openImportConfirm, toggleOpenImportConfirm] = useToggle(false);
  const [openDeleteConfirm, toggleOpenDeleteConfirm] = useToggle(false);

  const hasFailures = dataSetUpload.screenerResult.testResults.some(
    testResult => testResult.result === 'FAIL',
  );
  const hasWarnings = dataSetUpload.screenerResult.testResults.some(
    testResult => testResult.result === 'WARNING',
  );
  let tabTitle = '';

  if (hasFailures && hasWarnings) tabTitle = 'Failures & warnings';
  if (hasFailures && !hasWarnings) tabTitle = 'Failures';
  if (!hasFailures && hasWarnings) tabTitle = 'Warnings';

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

  const confirmText = hasWarnings
    ? 'Continue import with warnings'
    : 'Continue import';

  return (
    <tr key={dataSetUpload.dataSetTitle}>
      <td data-testid="Title" className={styles.title}>
        {dataSetUpload.dataSetTitle}
      </td>
      <td data-testid="Size" className={styles.fileSize}>
        {dataSetUpload.dataFileSizeInBytes}
      </td>
      <td data-testid="Status">
        <Tag colour={getDataSetUploadStatusColour(dataSetUpload.status)}>
          {getDataSetUploadStatusLabel(dataSetUpload.status)}
        </Tag>
      </td>
      <td data-testid="Actions">
        <ButtonGroup className={styles.actions}>
          <ModalConfirm
            title="Data set details"
            open={openImportConfirm}
            hideConfirm={hasFailures}
            onConfirm={() => onConfirmImport([dataSetUpload.id])}
            confirmText={confirmText}
            triggerButton={
              <ButtonText onClick={toggleOpenImportConfirm.on}>
                View details
              </ButtonText>
            }
          >
            <Tabs id="data-set-upload-tabs">
              {(hasFailures || hasWarnings) && (
                <TabsSection
                  id={dataSetUploadTabIds.screenerFailuresAndWarnings}
                  title={tabTitle}
                >
                  {hasFailures ? (
                    // TODO (EES-5353): This condition can be replaced with `overallResult` once it returns a reliable response
                    <>
                      <h3>Screener test failures</h3>
                      <WarningMessage>
                        You will need to delete this file (close this window,
                        and select "Delete files"), fix the failed tests and
                        upload again
                      </WarningMessage>
                    </>
                  ) : (
                    <>
                      <h3>Screener test warnings</h3>
                      <WarningMessage>
                        You will need to review each warning before continuing
                        the file upload
                      </WarningMessage>
                    </>
                  )}
                  <ScreenerResultsTable
                    screenerResult={dataSetUpload.screenerResult}
                    showAll={false}
                  />
                </TabsSection>
              )}
              <TabsSection
                id={dataSetUploadTabIds.screenerResults}
                title="All tests"
              >
                <h3>
                  Full breakdown of{' '}
                  {dataSetUpload.screenerResult.testResults.length} tests
                  checked against this file
                </h3>
                <ScreenerResultsTable
                  screenerResult={dataSetUpload.screenerResult}
                  showAll
                />
              </TabsSection>
              <TabsSection
                id={dataSetUploadTabIds.fileDetails}
                title="File details"
              >
                <DataSetUploadSummaryList dataSetUpload={dataSetUpload} />
              </TabsSection>
            </Tabs>
          </ModalConfirm>
          {canUpdateRelease && (
            <ModalConfirm
              open={openDeleteConfirm}
              title="Confirm deletion of selected data files"
              triggerButton={
                <ButtonText onClick={toggleOpenDeleteConfirm.on}>
                  Delete files
                </ButtonText>
              }
              onConfirm={handleDeleteConfirm}
            >
              <p>
                Are you sure you want to delete{' '}
                <strong>{dataSetUpload.dataSetTitle}</strong>?
              </p>
              <p>This data set has not yet been imported.</p>
            </ModalConfirm>
          )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
