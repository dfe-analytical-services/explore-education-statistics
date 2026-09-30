import DataUploadsPermissions from '@admin/pages/release/data/types/dataUploadsPermissions';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import { DataSetUpload } from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import React from 'react';
import { useQuery } from '@tanstack/react-query';
import styles from '@admin/pages/release/data/components/data-uploads/DataFilesTable.module.scss';
import DataSetUploadDeleteModal from './DataSetUploadDeleteModal';
import DataSetUploadDetailsModal from './DataSetUploadDetailsModal';
import ScreenerStatus, { terminalScreeningStatuses } from './ScreenerStatus';

interface Props {
  permissions: DataUploadsPermissions;
  dataSetUpload: DataSetUpload;
  /**
   * Whether this upload replaces an existing data file. Set explicitly by the
   * calling table rather than inferred, so a mis-filtered list cannot silently
   * render the wrong labels and actions.
   */
  isReplacement: boolean;
  releaseVersionId: string;
  onDeleteUpload: (deletedUploadId: string) => void;
  onImportDataSets: (uploadIds: string[]) => void;
  onRefreshUploads: () => void;
  testId?: string;
}

export default function DataSetUploadTableRow({
  permissions,
  dataSetUpload,
  isReplacement,
  releaseVersionId,
  onDeleteUpload,
  onImportDataSets,
  onRefreshUploads,
  testId,
}: Props) {
  // The screener updates this upload's status out-of-band, so poll for it
  // rather than relying on the status the list was fetched with.
  const { data: screenerProgress } = useQuery({
    ...releaseDataFileQueries.screeningStatus(
      releaseVersionId,
      dataSetUpload.id,
    ),
    // Don't poll if the upload has finished screening.
    enabled: !terminalScreeningStatuses.includes(dataSetUpload.screeningStatus),
    // Poll every 5 seconds until screening finishes.
    refetchInterval: progress =>
      progress && terminalScreeningStatuses.includes(progress.status)
        ? false
        : 5000,
    onSuccess: progress => {
      // Refresh the list so the row picks up the screener results that come
      // with the finished upload.
      if (
        progress.status !== dataSetUpload.screeningStatus &&
        terminalScreeningStatuses.includes(progress.status)
      ) {
        onRefreshUploads();
      }
    },
  });

  const screeningStatus =
    screenerProgress?.status ?? dataSetUpload.screeningStatus;

  return (
    <tr key={dataSetUpload.dataSetTitle}>
      <td
        data-testid={`${dataSetUpload.dataSetTitle}-title`}
        className={styles.title}
      >
        {dataSetUpload.dataSetTitle}
      </td>
      <td
        data-testid={`${dataSetUpload.dataSetTitle}-size`}
        className={styles.fileSize}
      >
        {dataSetUpload.dataFileSize}
      </td>
      <td data-testid={`${dataSetUpload.dataSetTitle}-status`}>
        <ScreenerStatus
          dataSetTitle={dataSetUpload.dataSetTitle}
          percentageComplete={screenerProgress?.percentageComplete ?? 0}
          status={screeningStatus}
        />
      </td>
      <td data-testid={`${dataSetUpload.dataSetTitle}-actions`}>
        <ButtonGroup className={styles.actions}>
          <DataSetUploadDetailsModal
            permissions={permissions}
            dataSetUpload={dataSetUpload}
            releaseVersionId={releaseVersionId}
            screeningStatus={screeningStatus}
            testId={testId}
            onImportDataSets={onImportDataSets}
          />
          {screeningStatus !== 'Screening' && permissions.canUpdateRelease && (
            <DataSetUploadDeleteModal
              dataSetTitle={dataSetUpload.dataSetTitle}
              dataSetUploadId={dataSetUpload.id}
              isReplacement={isReplacement}
              releaseVersionId={releaseVersionId}
              onDeleteUpload={onDeleteUpload}
            />
          )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
