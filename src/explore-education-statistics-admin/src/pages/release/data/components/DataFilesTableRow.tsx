import Link from '@admin/components/Link';
import DataFileSummaryList from '@admin/pages/release/data/components/DataFileSummaryList';
import DataUploadCancelButton from '@admin/pages/release/data/components/DataUploadCancelButton';
import { useFeatureFlag } from '@admin/contexts/FeatureFlagContext';
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
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import React from 'react';
import { generatePath } from 'react-router';
import styles from './DataFilesTable.module.scss';
import DeleteDataFileModal from './DeleteDataFileModal';

interface Props {
  canUpdateRelease?: boolean;
  dataFile: DataFile;
  publicationId: string;
  releaseVersionId: string;
  onConfirmDelete: (deletedFileId: string) => void;
  onStatusChange: (
    dataFile: DataFile,
    importStatus: DataFileImportStatus,
  ) => Promise<void>;
}

export default function DataFilesTableRow({
  canUpdateRelease,
  dataFile,
  publicationId,
  releaseVersionId,
  onConfirmDelete,
  onStatusChange,
}: Props) {
  const isNewReplaceDsvFeatureEnabled = useFeatureFlag(
    'enableReplacementOfPublicApiDataSets',
  );
  const allowReplacementOfDataFile = isNewReplaceDsvFeatureEnabled
    ? true
    : dataFile.publicApiDataSetId == null;

  return (
    <tr key={dataFile.title}>
      <td data-testid="Title" className={styles.title}>
        {dataFile.title}
      </td>
      <td data-testid="Size" className={styles.fileSize}>
        {dataFile.fileSize.size.toLocaleString()} {dataFile.fileSize.unit}
      </td>
      <td data-testid="Status">
        <ImporterStatus
          className={styles.fileStatus}
          dataFile={dataFile}
          hideErrors
          releaseVersionId={releaseVersionId}
          onStatusChange={onStatusChange}
        />
      </td>
      <td data-testid="Actions">
        <ButtonGroup className={styles.actions}>
          <Modal
            showClose
            title="Data file details"
            triggerButton={<ButtonText>View details</ButtonText>}
          >
            <DataFileSummaryList
              dataFile={dataFile}
              releaseVersionId={releaseVersionId}
              onStatusChange={onStatusChange}
            />
          </Modal>
          {canUpdateRelease &&
            terminalImportStatuses.includes(dataFile.status) && (
              <>
                {dataFile.status === 'COMPLETE' && (
                  <>
                    <Link
                      to={generatePath<ReleaseDataFileRouteParams>(
                        releaseDataFileRoute.path,
                        {
                          publicationId,
                          releaseVersionId,
                          fileId: dataFile.id,
                        },
                      )}
                    >
                      Edit title
                    </Link>
                    {!allowReplacementOfDataFile ? (
                      <Modal
                        showClose
                        title="Cannot replace data"
                        triggerButton={<ButtonText>Replace data</ButtonText>}
                      >
                        <p>
                          This data file has an API data set linked to it.
                          Please remove the API data set before replacing the
                          data.
                        </p>
                        <p>
                          <Link
                            to={generatePath<ReleaseDataSetRouteParams>(
                              releaseApiDataSetDetailsRoute.path,
                              {
                                publicationId,
                                releaseVersionId,
                                dataSetId: dataFile.publicApiDataSetId ?? '',
                              },
                            )}
                          >
                            Go to API data set
                          </Link>
                        </p>
                      </Modal>
                    ) : (
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
                        Replace data
                      </Link>
                    )}
                  </>
                )}
                {dataFile.publicApiDataSetId ? (
                  <Modal
                    showClose
                    title="Cannot delete files"
                    triggerButton={
                      <ButtonText variant="warning">Delete files</ButtonText>
                    }
                  >
                    <p>
                      This data file has an API data set linked to it. Please
                      remove the API data set before deleting.
                    </p>
                    <p>
                      <Link
                        to={generatePath<ReleaseDataSetRouteParams>(
                          releaseApiDataSetDetailsRoute.path,
                          {
                            publicationId,
                            releaseVersionId,
                            dataSetId: dataFile.publicApiDataSetId,
                          },
                        )}
                      >
                        Go to API data set
                      </Link>
                    </p>
                  </Modal>
                ) : (
                  <DeleteDataFileModal
                    dataFile={dataFile}
                    releaseVersionId={releaseVersionId}
                    onConfirm={() => onConfirmDelete(dataFile.id)}
                  />
                )}
              </>
            )}
          {dataFile.permissions.canCancelImport && (
            <DataUploadCancelButton
              releaseVersionId={releaseVersionId}
              fileId={dataFile.id}
            />
          )}
        </ButtonGroup>
      </td>
    </tr>
  );
}
