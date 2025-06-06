import {
  DataFile,
  DataFileImportStatus,
  DataSetInfo,
} from '@admin/services/releaseDataFileService';
import React from 'react';
import styles from './DataFilesTable.module.scss';
import DataFilesTableRow from './DataFilesTableRow';

interface Props {
  canUpdateRelease?: boolean;
  caption: string;
  dataFiles: DataSetInfo[];
  publicationId: string;
  releaseVersionId: string;
  testId?: string;
  onDeleteFile: (deletedFileId: string) => void;
  onStatusChange: (
    dataFile: DataSetInfo,
    importStatus: DataFileImportStatus,
  ) => Promise<void>;
}

export default function DataFilesTable({
  canUpdateRelease,
  caption,
  dataFiles,
  publicationId,
  releaseVersionId,
  testId,
  onDeleteFile,
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
            key={dataFile.dataSetTitle}
            publicationId={publicationId}
            releaseVersionId={releaseVersionId}
            onConfirmDelete={onDeleteFile}
            onStatusChange={onStatusChange}
          />
        ))}
      </tbody>
    </table>
  );
}
