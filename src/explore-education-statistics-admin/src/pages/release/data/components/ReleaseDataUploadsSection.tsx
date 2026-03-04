import DataFilesReorderableList from '@admin/pages/release/data/components/DataFilesReorderableList';
import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import dataReplacementService from '@admin/services/dataReplacementService';
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
import { useQuery, useQueryClient } from '@tanstack/react-query';
import React, { useCallback, useEffect, useMemo } from 'react';

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
  const [isReordering, toggleReordering] = useToggle(false);

  // NOTE: When a data set is initially imported, it is first sent to the data screener to check for screener errors and
  // warnings. At this stage, the data set will be returned from `listUploads`. If the file has no errors from the
  // screener tests and the user has pressed a button to continue the import, the data set then starts being imported
  // properly, and will then be returned from `list` instead.
  //
  // So "dataSetUploads" are data sets currently being screened via the R docker container, while "dataFiles" are data
  // sets that have moved beyond the screener and are now being imported by the Data.Processor
  const queryClient = useQueryClient();

  const {
    data: allDataFiles,
    isError: dataFilesError,
    isLoading,
    refetch: refetchDataFiles,
  } = useQuery(releaseDataFileQueries.list(releaseVersionId));
  const {
    data: allDataSetUploads,
    isError: dataSetUploadsError,
    isLoading: isLoadingUploads,
    refetch: refetchDataSetUploads,
  } = useQuery(releaseDataFileQueries.listUploads(releaseVersionId));

  useEffect(() => {
    if (allDataFiles) {
      onDataFilesChange?.(allDataFiles);
    }
  }, [allDataFiles, onDataFilesChange]);

  const uploadsWithoutReplacements = allDataSetUploads?.filter(
    upload => !upload.replacingFileId,
  );

  const uploadsWithReplacements = allDataSetUploads?.filter(
    upload => upload.replacingFileId,
  );

  const dataFilesExcludingReplacements = useMemo(
    () => allDataFiles?.filter(dataFile => !dataFile.replacedByDataFile),
    [allDataFiles],
  );

  const inProgressReplacementDataFiles = useMemo(
    () => allDataFiles?.filter(dataFile => dataFile.replacedByDataFile),
    [allDataFiles],
  );

  const validReplacementDataFiles = inProgressReplacementDataFiles?.filter(
    originalFile =>
      originalFile.replacedByDataFile?.status === 'COMPLETE' &&
      originalFile.replacedByDataFile?.hasValidReplacementPlan,
  );

  const handleStatusChange = useCallback(
    async (dataFile: DataFile, importStatus: DataFileImportStatus) => {
      try {
        const permissions = await permissionService.getDataFilePermissions(
          releaseVersionId,
          dataFile.id,
        );
        queryClient.setQueryData(
          releaseDataFileQueries.list(releaseVersionId).queryKey,
          (currentDataFiles: DataFile[] | undefined) =>
            currentDataFiles?.map(file =>
              file.id !== dataFile.id
                ? file
                : {
                    ...dataFile,
                    rows: importStatus.totalRows,
                    status: importStatus.status,
                    permissions,
                  },
            ),
        );
      } catch {
        refetchDataFiles();
      }
    },
    [releaseVersionId, queryClient, refetchDataFiles],
  );

  const handleReplacementStatusChange = useCallback(
    async (updatedDataFile: DataFile) => {
      try {
        const permissions = await permissionService.getDataFilePermissions(
          releaseVersionId,
          updatedDataFile.id,
        );

        queryClient.setQueryData(
          releaseDataFileQueries.list(releaseVersionId).queryKey,
          (currentDataFiles: DataFile[] | undefined) =>
            currentDataFiles?.map(file =>
              file.id !== updatedDataFile.id
                ? file
                : {
                    ...updatedDataFile,
                    permissions,
                  },
            ),
        );
      } catch {
        refetchDataFiles();
      }
    },
    [releaseVersionId, queryClient, refetchDataFiles],
  );

  const handleDataSetImport = useCallback(
    async (dataSetUploadIds: string[]) => {
      await releaseDataFileService.importDataSets(
        releaseVersionId,
        dataSetUploadIds,
      );

      queryClient.setQueryData(
        releaseDataFileQueries.listUploads(releaseVersionId).queryKey,
        (uploads: DataSetUpload[] | undefined) =>
          uploads?.filter(upload => !dataSetUploadIds.includes(upload.id)),
      );

      await refetchDataSetUploads();
      await refetchDataFiles();
    },
    [releaseVersionId, queryClient, refetchDataFiles, refetchDataSetUploads],
  );

  const handleDeleteUploadConfirm = useCallback(
    async (deletedUploadId: string) => {
      queryClient.setQueryData(
        releaseDataFileQueries.listUploads(releaseVersionId).queryKey,
        (uploads: DataSetUpload[] | undefined) =>
          uploads?.filter(upload => upload.id !== deletedUploadId),
      );
    },
    [queryClient, releaseVersionId],
  );

  const handleDeleteConfirm = useCallback(
    async (deletedFileId: string) => {
      queryClient.setQueryData(
        releaseDataFileQueries.list(releaseVersionId).queryKey,
        (files: DataFile[] | undefined) =>
          files?.filter(dataFile => dataFile.id !== deletedFileId),
      );
    },
    [queryClient, releaseVersionId],
  );

  const handleSubmit = async () => {
    await refetchDataFiles();
    await refetchDataSetUploads();
  };

  const handleConfirmReordering = useCallback(
    async (nextDataFiles: DataFile[]) => {
      queryClient.setQueryData(
        releaseDataFileQueries.list(releaseVersionId).queryKey,
        await releaseDataFileService.updateDataFilesOrder(
          releaseVersionId,
          nextDataFiles.map(file => file.id),
        ),
      );
      toggleReordering.off();
    },
    [releaseVersionId, queryClient, toggleReordering],
  );

  const handleConfirmAllReplacements = async () => {
    if (validReplacementDataFiles) {
      await dataReplacementService.replaceData(
        releaseVersionId,
        validReplacementDataFiles.map(file => file.id),
      );
      await refetchDataFiles();
    }
  };

  const errorFetchingData = dataFilesError || dataSetUploadsError;

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
            <a
              href="https://rsconnect/rsc/dfe-published-data-qa/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              screening app (opens in new tab)
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
        <h4>Data replacement</h4>
        <p>
          Files are expected to have a unique title, any files that are uploaded
          with a title that matches an existing file will start a data
          replacement instead of importing as a separate file.
        </p>
      </InsetText>
      <WarningMessage>
        The system runs some basic screening checks during data import. Analysts
        should still ensure that all data files undergo the full screening check
        suite prior to being uploaded, as provided by the external screener app.
      </WarningMessage>
      {canUpdateRelease ? (
        <DataFileUploadForm
          dataSetFileTitles={dataFilesExcludingReplacements?.map(
            file => file.title,
          )}
          releaseVersionId={releaseVersionId}
          onSubmit={handleSubmit}
        />
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <LoadingSpinner loading={isLoading || isLoadingUploads}>
        {(allDataFiles && allDataFiles.length > 0) ||
        (allDataSetUploads &&
          allDataSetUploads.length > 0 &&
          !errorFetchingData) ? (
          <>
            <h2>Uploaded data files</h2>

            {!isReordering && allDataFiles && allDataFiles.length > 1 && (
              <div className="dfe-flex dfe-justify-content--space-between">
                <Button onClick={toggleReordering.on} variant="secondary">
                  Reorder data files
                </Button>
                {validReplacementDataFiles &&
                  validReplacementDataFiles.length > 1 && (
                    <Button onClick={handleConfirmAllReplacements}>
                      Confirm all valid replacements
                    </Button>
                  )}
              </div>
            )}

            {allDataFiles && isReordering ? (
              <DataFilesReorderableList
                dataFiles={allDataFiles}
                onCancelReordering={toggleReordering.off}
                onConfirmReordering={handleConfirmReordering}
              />
            ) : (
              <>
                {inProgressReplacementDataFiles &&
                  uploadsWithReplacements &&
                  (inProgressReplacementDataFiles.length > 0 ||
                    uploadsWithReplacements.length > 0) && (
                    <DataFilesReplacementTable
                      canUpdateRelease={canUpdateRelease}
                      caption="Data file replacements"
                      dataFiles={inProgressReplacementDataFiles}
                      dataSetUploads={uploadsWithReplacements}
                      publicationId={publicationId}
                      releaseVersionId={releaseVersionId}
                      testId="Data file replacements table"
                      onConfirmReplacement={refetchDataFiles}
                      onDeleteUpload={handleDeleteUploadConfirm}
                      onDataSetImport={handleDataSetImport}
                      onReplacementStatusChange={handleReplacementStatusChange}
                    />
                  )}

                {dataFilesExcludingReplacements &&
                  uploadsWithoutReplacements &&
                  (dataFilesExcludingReplacements.length > 0 ||
                    uploadsWithoutReplacements.length > 0) && (
                    <DataFilesTable
                      canUpdateRelease={canUpdateRelease}
                      caption="Data files"
                      dataFiles={dataFilesExcludingReplacements}
                      dataSetUploads={uploadsWithoutReplacements}
                      publicationId={publicationId}
                      releaseVersionId={releaseVersionId}
                      testId="Data files table"
                      onDeleteFile={handleDeleteConfirm}
                      onDeleteUpload={handleDeleteUploadConfirm}
                      onDataSetImport={handleDataSetImport}
                      onReplaceFile={handleSubmit}
                      onStatusChange={handleStatusChange}
                    />
                  )}
              </>
            )}
          </>
        ) : (
          <>
            {errorFetchingData ? (
              <WarningMessage>Failed to fetch data files.</WarningMessage>
            ) : (
              <InsetText>No data files have been uploaded.</InsetText>
            )}
          </>
        )}
      </LoadingSpinner>
    </>
  );
}
