import DataUploadsPermissions from '@admin/pages/release/data/types/dataUploadsPermissions';
import {
  DataFile,
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import React from 'react';
import styles from '@admin/pages/release/data/components/data-uploads/DataFilesTable.module.scss';
import DataSetUploadTableRow from '@admin/pages/release/data/components/data-uploads/data-set-uploads/DataSetUploadTableRow';
import DataFileReplacementTableRow from './DataFileReplacementTableRow';

interface Props {
  permissions: DataUploadsPermissions;
  caption: string;
  dataFiles: DataFile[];
  dataSetUploads: DataSetUpload[];
  publicationId: string;
  releaseVersionId: string;
  testId?: string;
  onCancelReplacement: () => void;
  onConfirmReplacement: () => void;
  onDeleteUpload: (deletedUploadId: string) => void;
  onImportDataSets: (dataSetUploadIds: string[]) => void;
  onRefreshUploads: () => void;
}

export default function DataFileReplacementTable({
  permissions,
  caption,
  dataFiles,
  dataSetUploads,
  publicationId,
  releaseVersionId,
  testId,
  onCancelReplacement,
  onConfirmReplacement,
  onRefreshUploads,
  onDeleteUpload,
  onImportDataSets,
}: Props) {
  return (
    <div className="table-container">
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
              onCancelReplacement={onCancelReplacement}
              onConfirmReplacement={onConfirmReplacement}
            />
          ))}
          {dataSetUploads.map((upload, index) => (
            <DataSetUploadTableRow // These are rows for data sets that have been put through the screener
              permissions={permissions}
              dataSetUpload={upload}
              key={upload.id}
              releaseVersionId={releaseVersionId}
              isReplacement
              onDeleteUpload={onDeleteUpload}
              onImportDataSets={onImportDataSets}
              onRefreshUploads={onRefreshUploads}
              testId={`data-set-upload-row-${index + 1}`}
            />
          ))}
        </tbody>
      </table>
    </div>
  );
}
