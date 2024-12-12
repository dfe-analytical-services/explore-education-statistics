import DataFileUploadForm, {
  DataFileUploadFormValues,
} from '@admin/pages/release/data/components/DataFileUploadForm';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  ArchiveDataSetFile,
  DataFile,
  DataFileImportStatus,
  DeleteDataFilePlan,
} from '@admin/services/releaseDataFileService';
import Button from '@common/components/Button';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback, useEffect, useState } from 'react';
import BulkZipUploadModalConfirm from './BulkZipUploadModalConfirm';
import DataFilesTable from './DataFilesTable';

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
  const [dataFiles, setDataFiles] = useState<DataFile[]>([]);
  const [bulkUploadPlan, setBulkUploadPlan] = useState<ArchiveDataSetFile[]>();
  const [isReordering, toggleReordering] = useToggle(false);

  const {
    data: initialDataFiles,
    isLoading,
    refetch: refetchDataFiles,
  } = useQuery(releaseDataFileQueries.list(releaseId));

  // Store the data files on state so we can reliably update them
  // when the permissions/status change.
  useEffect(() => {
    setDataFiles(initialDataFiles ?? []);
  }, [initialDataFiles]);

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
      await releaseDataFileService.importBulkZipDataFile(
        releaseId,
        archiveDataSetFiles,
      );

      setBulkUploadPlan(undefined);
      refetchDataFiles();
    },
    [releaseId, setBulkUploadPlan, refetchDataFiles],
  );

  const handleStatusChange = async (
    dataFile: DataFile,
    { totalRows, status }: DataFileImportStatus,
  ) => {
    // EES-5732 UI tests related to data replacement sometimes fail
    // because of a permission call for the replaced file being called,
    // probably caused by the speed of the tests.
    // This prevents this happening.
    if (status === 'NOT_FOUND') {
      return;
    }

    const permissions = await permissionService.getDataFilePermissions(
      releaseId,
      dataFile.id,
    );

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

  const handleDeleteFile = async (dataFile: DataFile) => {
    releaseDataFileService
      .getDeleteDataFilePlan(releaseId, dataFile)
      .then(plan => {
        setDeleteDataFile({
          plan,
          file: dataFile,
        });
      });
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
          <>
            <h2 className="govuk-heading-l">Uploaded data files</h2>
            <DataFilesTable
              canUpdateRelease={canUpdateRelease}
              dataFiles={dataFiles}
              isReordering={isReordering}
              onCancelReordering={toggleReordering.off}
              onConfirmReordering={async nextDataFiles => {
                setDataFiles(
                  await releaseDataFileService.updateDataFilesOrder(
                    releaseId,
                    nextDataFiles.map(file => file.id),
                  ),
                );
                toggleReordering.off();
              }}
              onStatusChange={handleStatusChange}
              onDeleteFile={handleDeleteFile}
              publicationId={publicationId}
              releaseId={releaseId}
            />

            {isReordering ? undefined : (
              <Button onClick={toggleReordering.on} variant="secondary">
                Reorder data files
              </Button>
            )}
          </>
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
