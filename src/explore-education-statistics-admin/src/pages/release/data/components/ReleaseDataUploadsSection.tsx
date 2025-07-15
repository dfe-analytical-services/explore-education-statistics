import DataFilesReorderableList from '@admin/pages/release/data/components/DataFilesReorderableList';
import DataFileUploadForm, {
  DataFileUploadFormValues,
} from '@admin/pages/release/data/components/DataFileUploadForm';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import DataFilesTable from '@admin/pages/release/data/components/DataFilesTable';
import DataFilesReplacementTable from '@admin/pages/release/data/components/DataFilesReplacementTable';
import Button from '@common/components/Button';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback, useEffect, useMemo, useState } from 'react';

interface Props {
  publicationId: string;
  releaseVersionId: string;
  canUpdateRelease: boolean;
  onDataFilesChange?: (dataFiles: DataFile[]) => void;
}

export default function ReleaseDataUploadsSection({
  publicationId,
  releaseVersionId,
  canUpdateRelease,
  onDataFilesChange,
}: Props) {
  const [allDataFiles, setAllDataFiles] = useState<DataFile[]>([]);
  const [allDataSetUploads, setAllDataSetUploads] = useState<DataSetUpload[]>(
    [],
  );
  const [isReordering, toggleReordering] = useToggle(false);

  const {
    data: initialDataFiles,
    isLoading,
    refetch: refetchDataFiles,
  } = useQuery(releaseDataFileQueries.list(releaseVersionId));

  const {
    data: initialDataSetUploads,
    isLoading: isLoadingUploads,
    refetch: refetchDataSetUploads,
  } = useQuery(releaseDataFileQueries.listUploads(releaseVersionId));

  const uploadsWithoutReplacements = allDataSetUploads.filter(
    upload => !upload.replacingFileId,
  );

  const uploadsWithReplacements = allDataSetUploads.filter(
    upload => upload.replacingFileId,
  );

  // Store the data files on state so we can reliably update them
  // when the permissions/status change.
  useEffect(() => {
    setAllDataFiles(initialDataFiles ?? []);
    setAllDataSetUploads(initialDataSetUploads ?? []);
  }, [initialDataFiles, initialDataSetUploads]);

  useEffect(() => {
    onDataFilesChange?.(allDataFiles);
  }, [allDataFiles, onDataFilesChange]);

  const replacedDataFiles = useMemo(
    () => allDataFiles.filter(dataFile => dataFile.replacedBy),
    [allDataFiles],
  );

  // TODO - EES-6244 bulk confirmation of replacements
  //  const validReplacedDataFiles = replacedDataFiles.filter(
  //   file => file.status === 'COMPLETE', // this checks the original file status, not the replacement!
  // );
  // const allowBulkConfirm = validReplacedDataFiles.length > 1;

  const dataFiles = useMemo(
    () => allDataFiles.filter(dataFile => !dataFile.replacedBy),
    [allDataFiles],
  );

  const handleStatusChange = useCallback(
    async (dataFile: DataFile, { totalRows, status }: DataFileImportStatus) => {
      // EES-5732 UI tests related to data replacement sometimes fail
      // because of a permission call for the replaced file being called,
      // probably caused by the speed of the tests.
      // This prevents this happening.
      if (status === 'NOT_FOUND') {
        return;
      }

      const permissions = await permissionService.getDataFilePermissions(
        releaseVersionId,
        dataFile.id,
      );

      setAllDataFiles(currentDataFiles =>
        currentDataFiles.map(file =>
          file.fileName !== dataFile.fileName
            ? file
            : {
                ...dataFile,
                rows: totalRows,
                status,
                permissions,
                replacedBy: file.replacedBy,
              },
        ),
      );
    },
    [releaseVersionId],
  );

  const handleDataSetImport = useCallback(
    async (dataSetUploadIds: string[]) => {
      await releaseDataFileService.importDataSets(
        releaseVersionId,
        dataSetUploadIds,
      );

      setAllDataSetUploads(uploads =>
        uploads.filter(upload => !dataSetUploadIds.includes(upload.id)),
      );

      await refetchDataFiles();
      await refetchDataSetUploads();
    },
    [releaseVersionId, refetchDataFiles, refetchDataSetUploads],
  );

  const handleDeleteUploadConfirm = useCallback(
    async (deletedUploadId: string) => {
      setAllDataSetUploads(uploads =>
        uploads.filter(upload => upload.id !== deletedUploadId),
      );
    },
    [],
  );

  const handleDeleteConfirm = useCallback(async (deletedFileId: string) => {
    setAllDataFiles(files =>
      files.filter(dataFile => dataFile.id !== deletedFileId),
    );
  }, []);

  const handleSubmit = useCallback(
    async (values: DataFileUploadFormValues) => {
      switch (values.uploadType) {
        case 'csv': {
          if (!values.title) {
            return;
          }

          await releaseDataFileService.uploadDataSetFilePair(releaseVersionId, {
            title: values.title,
            dataFile: values.dataFile as File,
            metadataFile: values.metadataFile as File,
          });
          break;
        }
        case 'zip': {
          if (!values.title) {
            return;
          }
          await releaseDataFileService.uploadZippedDataSetFilePair(
            releaseVersionId,
            {
              title: values.title,
              zipFile: values.zipFile as File,
            },
          );
          break;
        }
        case 'bulkZip': {
          await releaseDataFileService.uploadBulkZipDataSetFile(
            releaseVersionId,
            values.bulkZipFile!,
          );
          break;
        }
        default:
          break;
      }

      await refetchDataFiles();
      await refetchDataSetUploads();
    },
    [releaseVersionId, refetchDataFiles, refetchDataSetUploads],
  );

  const handleConfirmReordering = useCallback(
    async (nextDataFiles: DataFile[]) => {
      setAllDataFiles(
        await releaseDataFileService.updateDataFilesOrder(
          releaseVersionId,
          nextDataFiles.map(file => file.id),
        ),
      );
      toggleReordering.off();
    },
    [releaseVersionId, toggleReordering],
  );

  // TODO - EES-6244 bulk confirmation of replacements
  // const handleConfirmAllReplacements = () => {};

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
        <DataFileUploadForm dataFiles={allDataFiles} onSubmit={handleSubmit} />
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <LoadingSpinner loading={isLoading || isLoadingUploads}>
        {allDataFiles.length > 0 || allDataSetUploads.length > 0 ? (
          <>
            <h2>Uploaded data files</h2>

            {!isReordering && allDataFiles.length > 1 && (
              <div className="dfe-flex dfe-justify-content--space-between">
                <Button onClick={toggleReordering.on} variant="secondary">
                  Reorder data files
                </Button>
                {/* TODO - EES-6244 bulk confirmation of replacements */}
                {/* {allowBulkConfirm && (
                  <Button onClick={handleConfirmAllReplacements}>
                    Confirm all valid replacements
                  </Button>
                )} */}
              </div>
            )}

            {isReordering ? (
              <DataFilesReorderableList
                dataFiles={allDataFiles}
                onCancelReordering={toggleReordering.off}
                onConfirmReordering={handleConfirmReordering}
              />
            ) : (
              <>
                {(replacedDataFiles.length > 0 ||
                  uploadsWithReplacements.length > 0) && (
                  <DataFilesReplacementTable
                    canUpdateRelease={canUpdateRelease}
                    caption="Data file replacements"
                    dataFiles={replacedDataFiles}
                    dataSetUploads={uploadsWithReplacements}
                    publicationId={publicationId}
                    releaseVersionId={releaseVersionId}
                    testId="Data file replacements table"
                    onConfirmReplacement={refetchDataFiles}
                    onDeleteUpload={handleDeleteUploadConfirm}
                    onDataSetImport={handleDataSetImport}
                  />
                )}

                {(dataFiles.length > 0 ||
                  uploadsWithoutReplacements.length > 0) && (
                  <DataFilesTable
                    canUpdateRelease={canUpdateRelease}
                    caption="Data files"
                    dataFiles={dataFiles}
                    dataSetUploads={uploadsWithoutReplacements}
                    publicationId={publicationId}
                    releaseVersionId={releaseVersionId}
                    testId="Data files table"
                    onDeleteFile={handleDeleteConfirm}
                    onDeleteUpload={handleDeleteUploadConfirm}
                    onDataSetImport={handleDataSetImport}
                    onStatusChange={handleStatusChange}
                  />
                )}
              </>
            )}
          </>
        ) : (
          <InsetText>No data files have been uploaded.</InsetText>
        )}
      </LoadingSpinner>
    </>
  );
}
