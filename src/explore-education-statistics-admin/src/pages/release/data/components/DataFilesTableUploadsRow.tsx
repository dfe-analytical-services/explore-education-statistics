import Link from '@admin/components/Link';
import {
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import { DataSetUpload } from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import React from 'react';
import { generatePath } from 'react-router';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import DataSetUploadSummaryList from './DataSetUploadSummaryList';
import dataSetUploadTabIds from '../utils/dataSetUploadTabIds';
import ScreenerResultSummaryList from './ScreenerResultSummaryList';
import { terminalImportStatuses } from './ImporterStatus';
import styles from './DataFilesTable.module.scss';

interface Props {
  canUpdateRelease?: boolean;
  dataSetUpload: DataSetUpload;
  publicationId: string;
  releaseVersionId: string;
}

export default function DataFilesTableUploadRow({
  canUpdateRelease,
  dataSetUpload,
  publicationId,
  releaseVersionId,
}: Props) {
  const hasFailures = dataSetUpload.screenerResult.testResults.some(
    result => result.testResult === 'FAIL',
  );
  const hasWarnings = dataSetUpload.screenerResult.testResults.some(
    result => result.testResult === 'WARNING',
  );
  let tabTitle = '';

  if (hasFailures && hasWarnings) tabTitle = 'Failures & warnings';
  if (hasFailures && !hasWarnings) tabTitle = 'Failures';
  if (!hasFailures && hasWarnings) tabTitle = 'Warnings';

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
            title="Data file details"
            triggerButton={<ButtonText>View details</ButtonText>}
          >
            <Tabs id="data-and-files-tabs">
              <TabsSection
                id={dataSetUploadTabIds.screenerFailuresAndWarnings}
                title={tabTitle}
              >
                {dataSetUpload.screenerResult.result === 'Failed' ? (
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
                <ScreenerResultSummaryList
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
                <ScreenerResultSummaryList
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
          {canUpdateRelease &&
            terminalImportStatuses.includes(dataSetUpload.status) && (
              <Link
                to={generatePath<ReleaseDataFileRouteParams>(
                  releaseDataFileRoute.path,
                  {
                    publicationId,
                    releaseVersionId,
                    fileId: dataSetUpload.id,
                  },
                )}
              >
                Edit title
              </Link>
            )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
