import DataUploadsPermissions from '@admin/pages/release/data/types/dataUploadsPermissions';
import ImporterStatus, {
  terminalImportStatuses,
} from '@admin/pages/release/data/components/ImporterStatus';
import {
  DataFile,
  DataFileImportStatus,
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import React from 'react';
import styles from '@admin/pages/release/data/components/data-uploads/DataFilesTable.module.scss';
import DataFileDeleteBlockedModal from './DataFileDeleteBlockedModal';
import DataFileDeleteModal from './DataFileDeleteModal';
import DataFileDetailsModal from './DataFileDetailsModal';
import DataFileTitleEditModal from './DataFileTitleEditModal';
import DataFileReplacementModal from './DataFileReplacementModal';
import DataFileImportCancelButton from './DataFileImportCancelButton';

interface Props {
  permissions: DataUploadsPermissions;
  dataFile: DataFile;
  publicationId: string;
  releaseVersionId: string;
  onDeleteFile: (deletedFileId: string) => void;
  onEditFile: () => void;
  onReplaceFile: () => void;
  onStatusChange: (
    dataFile: DataFile,
    importStatus: DataFileImportStatus,
  ) => Promise<void>;
}

export default function DataFileTableRow({
  permissions,
  dataFile,
  publicationId,
  releaseVersionId,
  onEditFile,
  onReplaceFile,
  onDeleteFile,
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
          <DataFileDetailsModal
            dataFile={dataFile}
            releaseVersionId={releaseVersionId}
            onStatusChange={onStatusChange}
          />
          {permissions.canUpdateRelease &&
            terminalImportStatuses.includes(dataFile.status) && (
              <>
                {dataFile.status === 'COMPLETE' && (
                  <>
                    <DataFileTitleEditModal
                      releaseVersionId={releaseVersionId}
                      dataFileId={dataFile.id}
                      dataFileTitle={dataFile.title}
                      onConfirm={onEditFile}
                    />
                    <DataFileReplacementModal
                      releaseVersionId={releaseVersionId}
                      dataFileId={dataFile.id}
                      dataFileTitle={dataFile.title}
                      onReplaceFile={onReplaceFile}
                    />
                  </>
                )}
                {dataFile.publicApiDataSetId ? (
                  <DataFileDeleteBlockedModal
                    canManagePublicApiDataSets={
                      permissions.canManagePublicApiDataSets
                    }
                    dataFileTitle={dataFile.title}
                    publicApiDataSetId={dataFile.publicApiDataSetId}
                    publicationId={publicationId}
                    releaseVersionId={releaseVersionId}
                  />
                ) : (
                  <DataFileDeleteModal
                    dataFileId={dataFile.id}
                    dataFileTitle={dataFile.title}
                    releaseVersionId={releaseVersionId}
                    onDeleteFile={onDeleteFile}
                  />
                )}
              </>
            )}
          {dataFile.permissions.canCancelImport && (
            <DataFileImportCancelButton
              releaseVersionId={releaseVersionId}
              fileId={dataFile.id}
            />
          )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
