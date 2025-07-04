import { DataFile } from '@admin/services/releaseDataFileService';
import styles from '@admin/pages/release/data/components/DataFilesTable.module.scss';
import DataFileReplacementTableRow from '@admin/pages/release/data/components/DataFilesReplacementTableRow';
import React from 'react';

interface Props {
  caption: string;
  dataFiles: DataFile[];
  publicationId: string;
  releaseVersionId: string;
  testId?: string;
  onConfirmAction?: () => void;
}

export default function DataFilesReplacementTable({
  caption,
  dataFiles,
  publicationId,
  releaseVersionId,
  testId,
  onConfirmAction,
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
          <DataFileReplacementTableRow
            dataFile={dataFile}
            key={dataFile.title}
            publicationId={publicationId}
            releaseVersionId={releaseVersionId}
            onConfirmAction={onConfirmAction}
          />
        ))}
      </tbody>
    </table>
  );
}
