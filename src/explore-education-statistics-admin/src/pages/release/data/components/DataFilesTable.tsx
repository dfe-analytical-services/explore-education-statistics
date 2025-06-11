import {
  DataFile,
  DataFileImportStatus,
} from '@admin/services/releaseDataFileService';
import styles from '@admin/pages/release/data/components/DataFilesTable.module.scss';
import DataFilesTableRow from '@admin/pages/release/data/components/DataFilesTableRow';
import React from 'react';

interface Props {
  canUpdateRelease?: boolean;
  caption: string;
  dataFiles: DataFile[];
  publicationId: string;
  releaseVersionId: string;
  testId?: string;
  onDeleteFile: (deletedFileId: string) => void;
  onStatusChange: (
    dataFile: DataFile,
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
            key={dataFile.title}
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
