import DataUploadsPermissions from '@admin/pages/release/data/types/dataUploadsPermissions';
import {
  DataFile,
  DataFileImportStatus,
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import DataFileTableRow from '@admin/pages/release/data/components/data-uploads/data-files/DataFileTableRow';
import React from 'react';
import DataSetUploadTableRow from '@admin/pages/release/data/components/data-uploads/data-set-uploads/DataSetUploadTableRow';
import styles from './DataFilesTable.module.scss';

interface Props {
  permissions: DataUploadsPermissions;
  caption: string;
  dataFiles: DataFile[];
  dataSetUploads: DataSetUpload[];
  publicationId: string;
  releaseVersionId: string;
  testId?: string;
  onDeleteFile: (deletedFileId: string) => void;
  onDeleteUpload: (deletedUploadId: string) => void;
  onImportDataSets: (dataSetUploadIds: string[]) => void;
  onEditFile: () => void;
  onReplaceFile: () => void;
  onStatusChange: (
    dataFile: DataFile,
    importStatus: DataFileImportStatus,
  ) => Promise<void>;
  onRefreshUploads: () => void;
}

export default function DataFilesTable({
  permissions,
  caption,
  dataFiles,
  dataSetUploads,
  publicationId,
  releaseVersionId,
  testId,
  onDeleteFile,
  onDeleteUpload,
  onImportDataSets,
  onEditFile,
  onReplaceFile,
  onRefreshUploads,
  onStatusChange,
}: Props) {
  return (
    <div className="table-container">
      <table className={styles.table} data-testid={testId}>
        <caption className="govuk-table__caption--m">{caption}</caption>

        <thead>
          <tr>
            <th scope="col">Title</th>
            <th scope="col">Size</th>
            <th scope="col">Status</th>
            <th className={styles.actionsColumn} scope="col">
              Actions
            </th>
          </tr>
        </thead>

        <tbody>
          {dataFiles.map(dataFile => (
            <DataFileTableRow
              permissions={permissions}
              dataFile={dataFile}
              key={dataFile.id}
              publicationId={publicationId}
              releaseVersionId={releaseVersionId}
              onDeleteFile={onDeleteFile}
              onEditFile={onEditFile}
              onReplaceFile={onReplaceFile}
              onStatusChange={onStatusChange}
            />
          ))}
          {dataSetUploads.map(upload => (
            <DataSetUploadTableRow
              permissions={permissions}
              dataSetUpload={upload}
              key={upload.id}
              releaseVersionId={releaseVersionId}
              isReplacement={false}
              onDeleteUpload={onDeleteUpload}
              onImportDataSets={onImportDataSets}
              onRefreshUploads={onRefreshUploads}
            />
          ))}
        </tbody>
      </table>
    </div>
  );
}
