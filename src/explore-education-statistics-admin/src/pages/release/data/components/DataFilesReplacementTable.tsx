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
  onConfirmReplacement?: () => void;
}

export default function DataFilesReplacementTable({
  caption,
  dataFiles,
  publicationId,
  releaseVersionId,
  testId,
  onConfirmReplacement,
}: Props) {
  return (
    <table className={styles.table} data-testid={testId}>
      <caption className="govuk-table__caption--m">{caption}</caption>

      <thead>
        <tr>
          <th scope="col">Title</th>
          <th scope="col">Size</th>
          <th scope="col">Replacement status</th>
          <th scope="col">Actions</th>
        </tr>
      </thead>

      <tbody>
        {dataFiles.map(dataFile => (
          <DataFileReplacementTableRow
            dataFile={dataFile}
            key={dataFile.title}
            publicationId={publicationId}
            releaseVersionId={releaseVersionId}
            onConfirmReplacement={onConfirmReplacement}
          />
        ))}
      </tbody>
    </table>
  );
}
