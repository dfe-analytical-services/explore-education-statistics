import {
  DataFile,
  DataFileImportStatus,
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import DataFilesTableRow from '@admin/pages/release/data/components/DataFilesTableRow';
import React from 'react';
import styles from './DataFilesTable.module.scss';
import DataFilesTableUploadRow from './DataFilesTableUploadsRow';

interface Props {
  canUpdateRelease?: boolean;
  caption: string;
  dataFiles: DataFile[];
  dataSetUploads: DataSetUpload[];
  publicationId: string;
  releaseVersionId: string;
  testId?: string;
  onDeleteFile: (deletedFileId: string) => void;
  onDeleteUpload: (deletedUploadId: string) => void;
  onDataSetImport: (dataSetImportIds: string[]) => void;
  onStatusChange: (
    dataFile: DataFile,
    importStatus: DataFileImportStatus,
  ) => Promise<void>;
}

export default function DataFilesTable({
  canUpdateRelease,
  caption,
  dataFiles,
  dataSetUploads,
  publicationId,
  releaseVersionId,
  testId,
  onDeleteFile,
  onDeleteUpload,
  onDataSetImport,
  onStatusChange,
}: Props) {
  return (
    <table className={styles.table} data-testid={testId}>
      <caption className="govuk-table__caption--m">{caption}</caption>

      <thead>
        <tr>
          <th scope="col">Title</th>
          <th scope="col">Size</th>
          <th scope="col">Status</th>
          <th scope="col">Actions</th>
        </tr>
      </thead>

      <tbody>
        {dataFiles.map(dataFile => (
          <DataFilesTableRow
            canUpdateRelease={canUpdateRelease}
            dataFile={dataFile}
            key={dataFile.id}
            publicationId={publicationId}
            releaseVersionId={releaseVersionId}
            onConfirmDelete={onDeleteFile}
            onStatusChange={onStatusChange}
          />
        ))}
        {dataSetUploads.map(upload => (
          <DataFilesTableUploadRow
            canUpdateRelease={canUpdateRelease}
            dataSetUpload={upload}
            key={upload.id}
            releaseVersionId={releaseVersionId}
            onConfirmDelete={onDeleteUpload}
            onConfirmImport={onDataSetImport}
          />
        ))}
      </tbody>
    </table>
  );
}
