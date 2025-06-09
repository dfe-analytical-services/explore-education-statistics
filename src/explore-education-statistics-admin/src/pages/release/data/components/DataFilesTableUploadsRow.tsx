import Link from '@admin/components/Link';
import {
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import { DataSetUpload } from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import React from 'react';
import { generatePath } from 'react-router';
import styles from './DataFilesTable.module.scss';
import { terminalImportStatuses } from './ImporterStatus';
import DataSetUploadSummaryList from './DataSetUploadSummaryList';

interface Props {
  canUpdateRelease?: boolean;
  dataSetUpload: DataSetUpload;
  publicationId: string;
  releaseVersionId: string;
}

export default function DataFilesTableUploadRow({
  canUpdateRelease,
  dataSetUpload,
  publicationId,
  releaseVersionId,
}: Props) {
  return (
    <tr key={dataSetUpload.dataSetTitle}>
      <td data-testid="Title" className={styles.title}>
        {dataSetUpload.dataSetTitle}
      </td>
      <td data-testid="Size" className={styles.fileSize}>
        999 Kb
      </td>
      <td data-testid="Status">{dataSetUpload.status}</td>
      <td data-testid="Actions">
        <ButtonGroup className={styles.actions}>
          <Modal
            showClose
            title="Data file details"
            triggerButton={<ButtonText>View details</ButtonText>}
          >
            <DataSetUploadSummaryList dataSetUpload={dataSetUpload} />
          </Modal>
          {canUpdateRelease &&
            terminalImportStatuses.includes(dataSetUpload.status) && (
              <Link
                to={generatePath<ReleaseDataFileRouteParams>(
                  releaseDataFileRoute.path,
                  {
                    publicationId,
                    releaseVersionId,
                    fileId: dataSetUpload.id,
                  },
                )}
              >
                Edit title
              </Link>
            )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
