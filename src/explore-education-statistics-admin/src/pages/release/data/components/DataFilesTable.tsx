import Link from '@admin/components/Link';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
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
} from '@admin/services/releaseDataFileService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import ReorderableList from '@common/components/ReorderableList';
import reorder from '@common/utils/reorder';
import React, { useEffect, useState } from 'react';
import { generatePath } from 'react-router';
import styles from './DataFilesTable.module.scss';

interface Props {
  dataFiles: DataFile[];
  publicationId: string;
  releaseId: string;
  canUpdateRelease?: boolean;
  isReordering: boolean;
  onCancelReordering: () => void;
  onConfirmReordering: (nextSeries: DataFile[]) => void;
  onDeleteFile: (dataFile: DataFile) => Promise<void>;
  onStatusChange: (
    dataFile: DataFile,
    { totalRows, status }: DataFileImportStatus,
  ) => Promise<void>;
}

export default function DataFilesTable({
  dataFiles: initialDataFiles,
  publicationId,
  releaseId,
  canUpdateRelease,
  isReordering,
  onCancelReordering,
  onConfirmReordering,
  onDeleteFile,
  onStatusChange,
}: Props) {
  const [dataFiles, setDataFiles] = useState(initialDataFiles);

  useEffect(() => {
    setDataFiles(initialDataFiles);
  }, [initialDataFiles]);

  if (isReordering) {
    return (
      <ReorderableList
        heading="Reorder data files"
        id="dataFiles"
        list={dataFiles.map(({ id, title }) => ({
          id,
          label: title,
        }))}
        onCancel={() => {
          setDataFiles(initialDataFiles);
          onCancelReordering();
        }}
        onConfirm={() => onConfirmReordering(dataFiles)}
        onMoveItem={({ prevIndex, nextIndex }) => {
          const reordered = reorder(dataFiles, prevIndex, nextIndex);
          setDataFiles(reordered);
        }}
        onReverse={() => {
          setDataFiles(dataFiles.toReversed());
        }}
      />
    );
  }

  return (
    <table className={styles.table} data-testid="Data files table">
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
          <tr key={dataFile.title}>
            <td data-testid="Title" className={styles.title}>
              {dataFile.title}
            </td>
            <td data-testid="Data file size" className={styles.fileSize}>
              {dataFile.fileSize.size.toLocaleString()} {dataFile.fileSize.unit}
            </td>
            <td data-testid="Status">
              <ImporterStatus
                className={styles.fileStatus}
                releaseId={releaseId}
                dataFile={dataFile}
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
                  <DataFileDetailsTable
                    dataFile={dataFile}
                    releaseId={releaseId}
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
                                releaseId,
                                fileId: dataFile.id,
                              },
                            )}
                          >
                            Edit title
                          </Link>
                          {dataFile.publicApiDataSetId ? (
                            <Modal
                              showClose
                              title="Cannot replace data"
                              triggerButton={
                                <ButtonText>Replace data</ButtonText>
                              }
                            >
                              <p>
                                This data file has an API data set linked to it.
                                Please remove the API data set before replacing
                                the data.
                              </p>
                              <p>
                                <Link
                                  to={generatePath<ReleaseDataSetRouteParams>(
                                    releaseApiDataSetDetailsRoute.path,
                                    {
                                      publicationId,
                                      releaseId,
                                      dataSetId: dataFile.publicApiDataSetId,
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
                                  releaseId,
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
                            <ButtonText className="govuk-!-margin-left-3">
                              Delete files
                            </ButtonText>
                          }
                        >
                          <p>
                            This data file has an API data set linked to it.
                            Please remove the API data set before deleting.
                          </p>
                          <p>
                            <Link
                              to={generatePath<ReleaseDataSetRouteParams>(
                                releaseApiDataSetDetailsRoute.path,
                                {
                                  publicationId,
                                  releaseId,
                                  dataSetId: dataFile.publicApiDataSetId,
                                },
                              )}
                            >
                              Go to API data set
                            </Link>
                          </p>
                        </Modal>
                      ) : (
                        <ButtonText
                          onClick={() => onDeleteFile(dataFile)}
                          variant="warning"
                        >
                          Delete files
                        </ButtonText>
                      )}
                    </>
                  )}
                {dataFile.permissions.canCancelImport && (
                  <DataUploadCancelButton
                    releaseId={releaseId}
                    fileId={dataFile.id}
                  />
                )}
              </ButtonGroup>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
