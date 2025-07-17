import Link from '@admin/components/Link';
import DataFileSummaryList from '@admin/pages/release/data/components/DataFileSummaryList';
import DataUploadCancelButton from '@admin/pages/release/data/components/DataUploadCancelButton';
import { useConfig } from '@admin/contexts/ConfigContext';
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
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
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
  const {
    enableReplacementOfPublicApiDataSets: isNewReplaceDsvFeatureEnabled,
  } = useConfig();
  const { releaseVersion } = useReleaseVersionContext();
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
                    <ReplaceDataButtonOrModal
                      allowReplacementOfDataFile={allowReplacementOfDataFile}
                      releaseIsNotAmendmentAndIsLinkedToApi={
                        !releaseVersion.amendment &&
                        dataFile.publicApiDataSetId != null
                      }
                      publicationId={publicationId}
                      dataFileId={dataFile.id}
                      publicApiDataSetId={dataFile.publicApiDataSetId}
                      releaseVersionId={releaseVersionId}
                    />
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

function CannotReplaceDataModal({ children }: { children: React.ReactNode }) {
  return (
    <Modal
      showClose
      title="Cannot replace data"
      triggerButton={<ButtonText>Replace data</ButtonText>}
    >
      {children}
    </Modal>
  );
}

function ReplaceDataButtonOrModal({
  allowReplacementOfDataFile,
  releaseIsNotAmendmentAndIsLinkedToApi,
  dataFileId,
  publicApiDataSetId,
  publicationId,
  releaseVersionId,
}: {
  allowReplacementOfDataFile: boolean;
  releaseIsNotAmendmentAndIsLinkedToApi: boolean;
  dataFileId: string;
  publicApiDataSetId: string | undefined;
  publicationId: string;
  releaseVersionId: string;
}) {
  if (!allowReplacementOfDataFile) {
    return (
      <CannotReplaceDataModal>
        <p>
          This data file has an API data set linked to it. Please remove the API
          data set before replacing the data.
        </p>
        <p>
          <Link
            to={
              publicApiDataSetId
                ? generatePath<ReleaseDataSetRouteParams>(
                    releaseApiDataSetDetailsRoute.path,
                    {
                      publicationId,
                      releaseVersionId,
                      dataSetId: publicApiDataSetId,
                    },
                  )
                : {}
            }
          >
            Go to API data set
          </Link>
        </p>
      </CannotReplaceDataModal>
    );
  }

  if (releaseIsNotAmendmentAndIsLinkedToApi) {
    return (
      <CannotReplaceDataModal>
        <p>
          This data replacement can not be completed as it is targeting an
          existing draft API data set.
        </p>
        <p>
          Please contact the explore statistics team at{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          for support on completing this replacement.
        </p>
      </CannotReplaceDataModal>
    );
  }

  return (
    <Link
      to={generatePath<ReleaseDataFileReplaceRouteParams>(
        releaseDataFileReplaceRoute.path,
        {
          publicationId,
          releaseVersionId,
          fileId: dataFileId,
        },
      )}
    >
      Replace data
    </Link>
  );
}
