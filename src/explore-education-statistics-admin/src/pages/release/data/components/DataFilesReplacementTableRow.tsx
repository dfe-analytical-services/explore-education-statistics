import Link from '@admin/components/Link';
import ImporterStatus from '@admin/pages/release/data/components/ImporterStatus';
import {
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
} from '@admin/routes/releaseRoutes';
import dataFileReplacementQueries from '@admin/queries/dataFileReplacementQueries';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  ReplacementDataFile,
} from '@admin/services/releaseDataFileService';
import dataReplacementService from '@admin/services/dataReplacementService';
import useToggle from '@common/hooks/useToggle';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import React, { useEffect } from 'react';
import { generatePath } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import styles from './DataFilesTable.module.scss';

interface Props {
  dataFile: DataFile;
  publicationId: string;
  releaseVersionId: string;
  onConfirmAction?: () => void;
  onReplacementStatusUpdate?: (updatedDataFile: DataFile) => void;
}

export default function DataFilesReplacementTableRow({
  dataFile,
  publicationId,
  releaseVersionId,
  onConfirmAction,
  onReplacementStatusUpdate,
}: Props) {
  const [fetchPlan, toggleFetchPlan] = useToggle(false);
  const [canCancel, toggleCanCancel] = useToggle(false);

  const { data: replacementDataFile, isLoading } = useQuery({
    ...releaseDataFileQueries.getDataFile(
      releaseVersionId,
      dataFile.replacedBy ?? '',
    ),
    initialData: dataFile.replacedByDataFile,
  });

  const { data: plan } = useQuery({
    ...dataFileReplacementQueries.getReplacementPlan(
      releaseVersionId,
      dataFile.id,
      dataFile.replacedBy ?? '',
    ),
    enabled: fetchPlan,
  });

  useEffect(() => {
    if (replacementDataFile?.status === 'COMPLETE') {
      toggleFetchPlan.on();
    }
  }, [replacementDataFile?.status, toggleFetchPlan, toggleCanCancel]);

  useEffect(() => {
    onReplacementStatusUpdate?.({
      ...dataFile,
      replacedByDataFile: {
        ...dataFile.replacedByDataFile,
        hasValidReplacementPlan: plan?.valid ?? false,
      } as ReplacementDataFile,
    });
  }, [plan, dataFile, onReplacementStatusUpdate]);

  const handleStatusChange = async (
    _: DataFile,
    replacementImportStatus: DataFileImportStatus,
  ) => {
    if (replacementImportStatus.status === 'COMPLETE') {
      toggleFetchPlan.on();
      toggleCanCancel.on();

      onReplacementStatusUpdate?.({
        ...dataFile,
        replacedByDataFile: {
          ...dataFile.replacedByDataFile,
          status: replacementImportStatus.status,
        } as ReplacementDataFile,
      });
    }
  };

  if (!replacementDataFile) {
    return (
      <tr>
        <td colSpan={4}>
          <LoadingSpinner loading={isLoading} />
        </td>
      </tr>
    );
  }

  return (
    <tr key={dataFile.title}>
      <td data-testid="Title" className={styles.title}>
        {dataFile.title}
      </td>
      <td data-testid="Size" className={styles.fileSize}>
        {dataFile.fileSize.size.toLocaleString()} {dataFile.fileSize.unit}
      </td>
      <td data-testid="Status">
        {plan ? (
          <>
            {plan?.valid ? (
              <Tag colour="green">Ready</Tag>
            ) : (
              <Tag colour="red">Error</Tag>
            )}
          </>
        ) : (
          <ImporterStatus
            className={styles.fileStatus}
            dataFile={replacementDataFile}
            hideErrors
            releaseVersionId={releaseVersionId}
            onStatusChange={handleStatusChange}
          />
        )}
      </td>
      <td data-testid="Actions">
        <ButtonGroup className={styles.actions}>
          <Link
            to={generatePath<ReleaseDataFileReplaceRouteParams>(
              releaseDataFileReplaceRoute.path,
              {
                publicationId,
                releaseVersionId,
                fileId: dataFile.id,
              },
            )}
          >
            View details
          </Link>
          <>
            {(canCancel || replacementDataFile.status === 'COMPLETE') && (
              <ModalConfirm
                title="Cancel data replacement"
                triggerButton={
                  <ButtonText variant="secondary">
                    Cancel replacement
                  </ButtonText>
                }
                onConfirm={async () => {
                  await releaseDataFileService.deleteDataFiles(
                    releaseVersionId,
                    replacementDataFile.id,
                  );
                  onConfirmAction?.();
                }}
              >
                <p>
                  Are you sure you want to cancel this data replacement? The
                  pending replacement data file will be deleted.
                </p>
              </ModalConfirm>
            )}
            {plan?.valid && (
              <ButtonText
                onClick={async () => {
                  await dataReplacementService.replaceData(releaseVersionId, [
                    dataFile.id,
                  ]);

                  onConfirmAction?.();
                }}
              >
                Confirm replacement
              </ButtonText>
            )}
          </>
        </ButtonGroup>
      </td>
    </tr>
  );
}
