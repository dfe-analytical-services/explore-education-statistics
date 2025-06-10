import releaseDataFileService, {
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import React, { useCallback } from 'react';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import DataSetUploadSummaryList from './DataSetUploadSummaryList';
import dataSetUploadTabIds from '../utils/dataSetUploadTabIds';
import ScreenerResultsList from './ScreenerResultsList';
import styles from './DataFilesTable.module.scss';

interface Props {
  canUpdateRelease?: boolean;
  dataSetUpload: DataSetUpload;
  releaseVersionId: string;
}

export default function DataFilesTableUploadRow({
  canUpdateRelease,
  dataSetUpload,
  releaseVersionId,
}: Props) {
  const [open, toggleOpen] = useToggle(false);

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
    } catch (err) {
      logger.error(err);
    } finally {
      toggleOpen.off();
    }
  }, [releaseVersionId, dataSetUpload.id, toggleOpen]);

  return (
    <tr key={dataSetUpload.dataSetTitle}>
      <td data-testid="Title" className={styles.title}>
        {dataSetUpload.dataSetTitle}
      </td>
      <td data-testid="Size" className={styles.fileSize}>
        999 Kb
      </td>
      <td data-testid="Status">{dataSetUpload.status}</td>
      <td data-testid="Actions">
        <ButtonGroup className={styles.actions}>
          <Modal
            showClose
            title="Data set details"
            triggerButton={<ButtonText>View details</ButtonText>}
          >
            <Tabs id="data-set-upload-tabs">
              <TabsSection
                id={dataSetUploadTabIds.screenerFailuresAndWarnings}
                title={tabTitle}
              >
                {dataSetUpload.screenerResult.overallResult === 'Failed' ? (
                  <>
                    <h3>Screener test failures</h3>
                    <WarningMessage>
                      You will need to delete this file, fix the failed tests
                      and upload again
                    </WarningMessage>
                  </>
                ) : (
                  <>
                    <h3>Screener test warnings</h3>
                    <WarningMessage>
                      You will need to review each warning before continuing the
                      file upload
                    </WarningMessage>
                  </>
                )}
                <ScreenerResultsList
                  screenerResult={dataSetUpload.screenerResult}
                  showAll={false}
                />
              </TabsSection>
              <TabsSection
                id={dataSetUploadTabIds.screenerResults}
                title="All tests"
              >
                <h3>
                  Full breakdown of{' '}
                  {dataSetUpload.screenerResult.testResults.length} tests
                  checked against this file
                </h3>
                <ScreenerResultsList
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
          </Modal>
          {canUpdateRelease && (
            <ModalConfirm
              open={open}
              title="Confirm deletion of selected data files"
              triggerButton={
                <ButtonText onClick={toggleOpen.on}>Delete file</ButtonText>
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
