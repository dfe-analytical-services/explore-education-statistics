import Link from '@admin/components/Link';
import DataFileSummaryList from '@admin/pages/release/data/components/DataFileSummaryList';
import DataUploadCancelButton from '@admin/pages/release/data/components/DataUploadCancelButton';
import ImporterStatus, {
  terminalImportStatuses,
} from '@admin/pages/release/data/components/ImporterStatus';
import {
  releaseApiDataSetDetailsRoute,
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import {
  DataFile,
  DataFileImportStatus,
  DeleteDataFilePlan,
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import React from 'react';
import { generatePath } from 'react-router';
import styles from './DataFilesTable.module.scss';
import DataFilesTableRow from './DataFilesTableRow';

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
