import {
  DataFile,
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import styles from '@admin/pages/release/data/components/DataFilesTable.module.scss';
import DataFileReplacementTableRow from '@admin/pages/release/data/components/DataFilesReplacementTableRow';
import React from 'react';
import DataFilesTableUploadRow from './DataFilesTableUploadsRow';

interface Props {
  canUpdateRelease?: boolean;
  caption: string;
  dataFiles: DataFile[];
  dataSetUploads: DataSetUpload[];
  publicationId: string;
  releaseVersionId: string;
  testId?: string;
  onConfirmReplacement?: () => void;
  onDeleteUpload: (deletedUploadId: string) => void;
  onDataSetImport: (dataSetImportIds: string[]) => void;
  onReplacementStatusChange: (updatedDataFile: DataFile) => void;
}

export default function DataFilesReplacementTable({
  canUpdateRelease,
  caption,
  dataFiles,
  dataSetUploads,
  publicationId,
  releaseVersionId,
  testId,
  onConfirmReplacement,
  onDeleteUpload,
  onDataSetImport,
  onReplacementStatusChange,
}: Props) {
  return (
    <table className={styles.table} data-testid={testId}>
      <caption className="govuk-table__caption--m">{caption}</caption>

      <thead>
        <tr>
          <th scope="col">Title</th>
          <th scope="col">Size</th>
          <th scope="col">Replacement status</th>
          <th className={styles.actionsColumn} scope="col">
            Actions
          </th>
        </tr>
      </thead>

      <tbody>
        {dataFiles.map(dataFile => (
          <DataFileReplacementTableRow // These are rows for data sets that have passed the screener and been/being imported
            dataFile={dataFile}
            key={dataFile.title}
            publicationId={publicationId}
            releaseVersionId={releaseVersionId}
            onConfirmAction={onConfirmReplacement}
            onReplacementStatusChange={onReplacementStatusChange}
          />
        ))}
        {dataSetUploads.map(upload => (
          <DataFilesTableUploadRow // These are rows for data sets that have been put through the screener
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
