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
import { generatePath } from 'react-router';
import React from 'react';
import styles from './DataFilesTable.module.scss';

interface Props {
  dataFiles: DataFile[];
  publicationId: string;
  releaseId: string;
  canUpdateRelease?: boolean;
  handleDeleteFile: (dataFile: DataFile) => Promise<void>;
  handleStatusChange: (
    dataFile: DataFile,
    { totalRows, status }: DataFileImportStatus,
  ) => Promise<void>;
}

const DataFilesTable = ({
  dataFiles,
  publicationId,
  releaseId,
  canUpdateRelease,
  handleDeleteFile,
  handleStatusChange,
}: Props) => {
  return (
    <>
      <h2 className="govuk-heading-l">Uploaded data files</h2>
      <table className={styles.table}>
        <thead>
          <tr>
            <th scope="col">Subject title</th>
            <th scope="col">Size</th>
            {/* <th scope="col">Rows</th> */}
            <th scope="col">Status</th>
            {/* <th scope="col">Uploaded by</th> */}
            {/* 
                  <th scope="col">Date uploaded</th> */}
            <th scope="col">Actions</th>
          </tr>
        </thead>

        <tbody>
          {dataFiles.map(dataFile => (
            <tr key={dataFile.title}>
              <td>{dataFile.title}</td>
              <td className={styles.fileSize}>
                {dataFile.fileSize.size.toLocaleString()}{' '}
                {dataFile.fileSize.unit}
              </td>
              {/* <td>{dataFile.rows}</td> */}
              <td>
                <ImporterStatus
                  className={styles.fileStatus}
                  releaseId={releaseId}
                  dataFile={dataFile}
                  onStatusChange={handleStatusChange}
                />
              </td>
              {/* <td>
                <a href={`mailto:${dataFile.userName}`}>{dataFile.userName}</a>
              </td> */}
              {/* <td>
                      {!dataFile.created ? undefined : (
                        <FormattedDate format="d MMMM yyyy HH:mm">
                          {dataFile.created}
                        </FormattedDate>
                      )}
                    </td> */}
              <td>
                <ButtonGroup className={styles.actions}>
                  <Modal
                    showClose
                    title="Data file details"
                    triggerButton={<ButtonText>View details</ButtonText>}
                  >
                    <DataFileDetailsTable
                      dataFile={dataFile}
                      releaseId={releaseId}
                      onStatusChange={handleStatusChange}
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
                                  This data file has an API data set linked to
                                  it. Please remove the API data set before
                                  replacing the data.
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
                            onClick={() => handleDeleteFile(dataFile)}
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
    </>
  );
};

export default DataFilesTable;
