import Link from '@admin/components/Link';
import DataFileSummaryList from '@admin/pages/release/data/components/DataFileSummaryList';
import DataUploadCancelButton from '@admin/pages/release/data/components/DataUploadCancelButton';
import ImporterStatus, {
  terminalImportStatuses,
} from '@admin/pages/release/data/components/ImporterStatus';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import {
  DataFile,
  DataFileImportStatus,
} from '@admin/services/releaseDataFileService';
import DataFilesTableRowReplaceModal from '@admin/pages/release/data/components/DataFilesTableRowReplaceModal';
import DataFilesTableRowDeleteModal from '@admin/pages/release/data/components/DataFilesTableRowDeleteModal';
import DataFilesTableRowEditTitleModal from '@admin/pages/release/data/components/DataFilesTableRowEditTitleModal';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';
import { generatePath } from 'react-router';
import styles from './DataFilesTable.module.scss';

interface Props {
  canUpdateRelease?: boolean;
  dataFile: DataFile;
  publicationId: string;
  releaseVersionId: string;
  onConfirmDelete: (deletedFileId: string) => void;
  onEditFile: () => void;
  onReplaceFile: () => void;
  onStatusChange: (
    dataFile: DataFile,
    importStatus: DataFileImportStatus,
  ) => Promise<void>;
}

export default function DataFilesTableRow({
  canUpdateRelease,
  dataFile,
  publicationId,
  releaseVersionId,
  onEditFile,
  onReplaceFile,
  onConfirmDelete,
  onStatusChange,
}: Props) {
  return (
    <tr key={dataFile.title}>
      <td data-testid={`${dataFile.title}-title`} className={styles.title}>
        {dataFile.title}
      </td>
      <td data-testid={`${dataFile.title}-size`} className={styles.fileSize}>
        {dataFile.fileSize.size.toLocaleString()} {dataFile.fileSize.unit}
      </td>
      <td data-testid={`${dataFile.title}-status`}>
        <ImporterStatus
          className={styles.fileStatus}
          dataFile={dataFile}
          hideErrors
          releaseVersionId={releaseVersionId}
          onStatusChange={onStatusChange}
        />
      </td>
      <td data-testid={`${dataFile.title}-actions`}>
        <ButtonGroup className={styles.actions}>
          <Modal
            showClose
            title="Data file details"
            triggerButton={
              <ButtonText>
                View details
                <VisuallyHidden>{` for ${dataFile.title}`}</VisuallyHidden>
              </ButtonText>
            }
          >
            <DataFileSummaryList
              dataFile={dataFile}
              releaseVersionId={releaseVersionId}
              onStatusChange={onStatusChange}
            />
          </Modal>
          {canUpdateRelease &&
            terminalImportStatuses.includes(dataFile.status) && (
              <>
                {dataFile.status === 'COMPLETE' && (
                  <>
                    <DataFilesTableRowEditTitleModal
                      releaseVersionId={releaseVersionId}
                      dataFileId={dataFile.id}
                      dataFileTitle={dataFile.title}
                      onConfirm={onEditFile}
                    />
                    <DataFilesTableRowReplaceModal
                      releaseVersionId={releaseVersionId}
                      dataFileId={dataFile.id}
                      dataFileTitle={dataFile.title}
                      onReplaceFile={onReplaceFile}
                    />
                  </>
                )}
                {dataFile.publicApiDataSetId ? (
                  <Modal
                    showClose
                    title="Cannot delete files"
                    triggerButton={
                      <ButtonText variant="warning">
                        Delete files
                        <VisuallyHidden>{` for ${dataFile.title}`}</VisuallyHidden>
                      </ButtonText>
                    }
                  >
                    <p>
                      This data file has an API data set linked to it. Please
                      remove the API data set before deleting.
                    </p>
                    <p>
                      <Link
                        to={generatePath<ReleaseDataSetRouteParams>(
                          releaseApiDataSetDetailsRoute.path,
                          {
                            publicationId,
                            releaseVersionId,
                            dataSetId: dataFile.publicApiDataSetId,
                          },
                        )}
                      >
                        Go to API data set
                      </Link>
                    </p>
                  </Modal>
                ) : (
                  <DataFilesTableRowDeleteModal
                    dataFileId={dataFile.id}
                    dataFileTitle={dataFile.title}
                    releaseVersionId={releaseVersionId}
                    onConfirm={() => onConfirmDelete(dataFile.id)}
                  />
                )}
              </>
            )}
          {dataFile.permissions.canCancelImport && (
            <DataUploadCancelButton
              releaseVersionId={releaseVersionId}
              fileId={dataFile.id}
            />
          )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
