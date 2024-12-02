import Link from '@admin/components/Link';
import ReorderableAccordion from '@admin/components/editable/ReorderableAccordion';
import ReorderableAccordionSection from '@admin/components/editable/ReorderableAccordionSection';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
import DataFileUploadForm, {
  DataFileUploadFormValues,
} from '@admin/pages/release/data/components/DataFileUploadForm';
import { terminalImportStatuses } from '@admin/pages/release/data/components/ImporterStatus';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import {
  releaseApiDataSetDetailsRoute,
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  ArchiveDataSetFile,
  DataFile,
  DataFileImportStatus,
  DeleteDataFilePlan,
} from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import logger from '@common/services/logger';
import DataUploadCancelButton from '@admin/pages/release/data/components/DataUploadCancelButton';
import React, { useCallback, useEffect, useState } from 'react';
import { generatePath } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import BulkZipUploadModalConfirm from './BulkZipUploadModalConfirm';

interface Props {
  publicationId: string;
  releaseId: string;
  canUpdateRelease: boolean;
  onDataFilesChange?: (dataFiles: DataFile[]) => void;
}

interface DeleteDataFile {
  plan: DeleteDataFilePlan;
  file: DataFile;
}

const ReleaseDataUploadsSection = ({
  publicationId,
  releaseId,
  canUpdateRelease,
  onDataFilesChange,
}: Props) => {
  const [deleteDataFile, setDeleteDataFile] = useState<DeleteDataFile>();
  const [activeFileIds, setActiveFileIds] = useState<string[]>();
  const [dataFiles, setDataFiles] = useState<DataFile[]>([]);
  const [bulkUploadPlan, setBulkUploadPlan] = useState<ArchiveDataSetFile[]>();

  const {
    data: initialDataFiles = [],
    isLoading,
    refetch: refetchDataFiles,
  } = useQuery(releaseDataFileQueries.list(releaseId));

  // Store the data files on state so we can reliably update them
  // when the permissions/status change.
  useEffect(() => {
    setDataFiles(initialDataFiles);
  }, [initialDataFiles, isLoading, setDataFiles]);

  useEffect(() => {
    onDataFilesChange?.(dataFiles);
  }, [dataFiles, onDataFilesChange]);

  const setFileDeleting = (dataFile: DeleteDataFile, deleting: boolean) => {
    setDataFiles(currentDataFiles =>
      currentDataFiles.map(file =>
        file.fileName !== dataFile.file.fileName
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    );
  };

  const confirmBulkUploadPlan = useCallback(
    async (archiveDataSetFiles: ArchiveDataSetFile[]) => {
      const newFiles = await releaseDataFileService.importBulkZipDataFile(
        releaseId,
        archiveDataSetFiles,
      );

      setBulkUploadPlan(undefined);
      setActiveFileIds([...dataFiles, ...newFiles].map(file => file.id));
      refetchDataFiles();
    },
    [
      releaseId,
      setBulkUploadPlan,
      setActiveFileIds,
      dataFiles,
      refetchDataFiles,
    ],
  );

  const handleStatusChange = async (
    dataFile: DataFile,
    { totalRows, status }: DataFileImportStatus,
  ) => {
    const permissions = await permissionService.getDataFilePermissions(
      releaseId,
      dataFile.id,
    );

    setActiveFileIds([]);

    setDataFiles(currentDataFiles =>
      currentDataFiles.map(file =>
        file.fileName !== dataFile.fileName
          ? file
          : {
              ...dataFile,
              rows: totalRows,
              status,
              permissions,
            },
      ),
    );
  };

  const handleSubmit = useCallback(
    async (values: DataFileUploadFormValues) => {
      const newFiles: DataFile[] = [];

      switch (values.uploadType) {
        case 'csv': {
          if (!values.subjectTitle) {
            return;
          }
          newFiles.push(
            await releaseDataFileService.uploadDataFiles(releaseId, {
              title: values.subjectTitle,
              dataFile: values.dataFile as File,
              metadataFile: values.metadataFile as File,
            }),
          );
          refetchDataFiles();
          break;
        }
        case 'zip': {
          if (!values.subjectTitle) {
            return;
          }
          newFiles.push(
            await releaseDataFileService.uploadZipDataFile(releaseId, {
              title: values.subjectTitle,
              zipFile: values.zipFile as File,
            }),
          );
          refetchDataFiles();
          break;
        }
        case 'bulkZip': {
          const uploadPlan =
            await releaseDataFileService.getUploadBulkZipDataFilePlan(
              releaseId,
              values.bulkZipFile!,
            );

          setBulkUploadPlan(uploadPlan);
          break;
        }
        default:
          break;
      }
      setActiveFileIds(newFiles.map(file => file.id));
    },
    [releaseId, refetchDataFiles],
  );

  return (
    <>
      <h2>Add data file to release</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Data files will be displayed in the table tool and can be used to
          create data blocks. They will also be attached to the release for
          users to download. Please ensure:
        </p>
        <ul>
          <li>
            your data files have passed the checks in our{' '}
            <a href="https://rsconnect/rsc/dfe-published-data-qa/">
              screening app
            </a>
          </li>
          <li>
            your data files meets these standards - if not you won't be able to
            upload it to your release
          </li>
          <li>
            if you have any issues uploading data files, or questions about data
            standards contact:{' '}
            <a href="mailto:explore.statistics@education.gov.uk">
              explore.statistics@education.gov.uk
            </a>
          </li>
        </ul>
      </InsetText>
      {canUpdateRelease ? (
        <>
          <DataFileUploadForm dataFiles={dataFiles} onSubmit={handleSubmit} />
          {bulkUploadPlan === undefined ||
          bulkUploadPlan.length === 0 ? null : (
            <BulkZipUploadModalConfirm
              bulkUploadPlan={bulkUploadPlan}
              onConfirm={confirmBulkUploadPlan}
              onCancel={() => setBulkUploadPlan(undefined)}
            />
          )}
        </>
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <LoadingSpinner loading={isLoading}>
        {dataFiles.length > 0 ? (
          <ReorderableAccordion
            canReorder={canUpdateRelease}
            id="uploadedDataFiles"
            heading="Uploaded data files"
            reorderHiddenText="files"
            onReorder={async (fileIds: string[]) => {
              setDataFiles(
                await releaseDataFileService.updateDataFilesOrder(
                  releaseId,
                  fileIds,
                ),
              );
            }}
          >
            {dataFiles.map(dataFile => (
              <ReorderableAccordionSection
                id={dataFile.id}
                key={dataFile.title}
                heading={dataFile.title}
                headingTag="h3"
                open={activeFileIds?.includes(dataFile.id)}
              >
                <div style={{ position: 'relative' }}>
                  {dataFile.isDeleting && (
                    <LoadingSpinner text="Deleting files" overlay />
                  )}
                  <DataFileDetailsTable
                    dataFile={dataFile}
                    releaseId={releaseId}
                    onStatusChange={handleStatusChange}
                  >
                    {canUpdateRelease &&
                      terminalImportStatuses.includes(dataFile.status) && (
                        <>
                          {dataFile.status === 'COMPLETE' && (
                            <>
                              <Link
                                className="govuk-!-margin-right-4"
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
                                          dataSetId:
                                            dataFile.publicApiDataSetId,
                                        },
                                      )}
                                    >
                                      Go to API data set
                                    </Link>
                                  </p>
                                </Modal>
                              ) : (
                                <Link
                                  className="govuk-!-margin-right-4"
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
                              onClick={async () =>
                                releaseDataFileService
                                  .getDeleteDataFilePlan(releaseId, dataFile)
                                  .then(plan => {
                                    setDeleteDataFile({
                                      plan,
                                      file: dataFile,
                                    });
                                  })
                              }
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
                  </DataFileDetailsTable>
                </div>
              </ReorderableAccordionSection>
            ))}
          </ReorderableAccordion>
        ) : (
          <InsetText>No data files have been uploaded.</InsetText>
        )}
      </LoadingSpinner>
      {deleteDataFile && (
        <ModalConfirm
          open
          title="Confirm deletion of selected data files"
          onExit={() => setDeleteDataFile(undefined)}
          onCancel={() => setDeleteDataFile(undefined)}
          onConfirm={async () => {
            const { file } = deleteDataFile;

            setDeleteDataFile(undefined);
            setFileDeleting(deleteDataFile, true);

            try {
              await releaseDataFileService.deleteDataFiles(releaseId, file.id);

              setDataFiles(currentDataFiles =>
                currentDataFiles.filter(dataFile => dataFile.id !== file.id),
              );
            } catch (err) {
              logger.error(err);
              setFileDeleting(deleteDataFile, false);
            }
          }}
        >
          <p>Are you sure you want to delete {deleteDataFile.file.title}?</p>
          <p>This data will no longer be available for use in this release.</p>

          {deleteDataFile.plan.deleteDataBlockPlan.dependentDataBlocks.length >
            0 && (
            <>
              <p>The following data blocks will also be deleted:</p>

              <ul>
                {deleteDataFile.plan.deleteDataBlockPlan.dependentDataBlocks.map(
                  block => (
                    <li key={block.name}>
                      <p>{block.name}</p>
                      {block.contentSectionHeading && (
                        <p>
                          {`It will also be removed from the "${block.contentSectionHeading}" content section.`}
                        </p>
                      )}
                      {block.infographicFilesInfo.length > 0 && (
                        <p>
                          The following infographic files will also be removed:
                          <ul>
                            {block.infographicFilesInfo.map(fileInfo => (
                              <li key={fileInfo.filename}>
                                <p>{fileInfo.filename}</p>
                              </li>
                            ))}
                          </ul>
                        </p>
                      )}
                      {block.isKeyStatistic && (
                        <p>
                          A key statistic associated with this data block will
                          be removed.
                        </p>
                      )}
                      {block.featuredTable && (
                        <p>
                          The featured table "{`${block.featuredTable?.name}`}"
                          using this data block will be removed.
                        </p>
                      )}
                    </li>
                  ),
                )}
              </ul>
            </>
          )}
          {deleteDataFile.plan.footnoteIds.length > 0 && (
            <p>
              {`${deleteDataFile.plan.footnoteIds.length} ${
                deleteDataFile.plan.footnoteIds.length > 1
                  ? 'footnotes'
                  : 'footnote'
              } will be removed or updated.`}
            </p>
          )}
        </ModalConfirm>
      )}
    </>
  );
};

export default ReleaseDataUploadsSection;
