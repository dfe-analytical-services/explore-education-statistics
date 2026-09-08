import DataFilesReorderableList from '@admin/pages/release/data/components/DataFilesReorderableList';
import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import DataUploadsGuidance from '@admin/pages/release/data/components/DataUploadsGuidance';
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
import { useQuery, useQueryClient, Updater } from '@tanstack/react-query';
import React, { useCallback, useMemo } from 'react';

interface Props {
  publicationId: string;
  releaseVersionId: string;
  canUpdateRelease: boolean;
}

export default function ReleaseDataUploadsSection({
  publicationId,
  releaseVersionId,
  canUpdateRelease,
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
    data: allDataFiles = [],
    isError: dataFilesError,
    isLoading,
    refetch: refetchDataFiles,
  } = useQuery(releaseDataFileQueries.list(releaseVersionId));
  const {
    data: allDataSetUploads = [],
    isError: dataSetUploadsError,
    isLoading: isLoadingUploads,
    refetch: refetchDataSetUploads,
  } = useQuery(releaseDataFileQueries.listUploads(releaseVersionId));

  const uploadsWithoutReplacements = useMemo(
    () => allDataSetUploads.filter(upload => !upload.replacingFileId),
    [allDataSetUploads],
  );

  const uploadsWithReplacements = useMemo(
    () => allDataSetUploads.filter(upload => upload.replacingFileId),
    [allDataSetUploads],
  );

  const dataFilesExcludingReplacements = useMemo(
    () => allDataFiles.filter(dataFile => !dataFile.replacedByDataFile),
    [allDataFiles],
  );

  const inProgressReplacementDataFiles = useMemo(
    () => allDataFiles.filter(dataFile => dataFile.replacedByDataFile),
    [allDataFiles],
  );

  const validReplacementDataFiles = inProgressReplacementDataFiles.filter(
    originalFile =>
      originalFile.replacedByDataFile?.status === 'COMPLETE' &&
      originalFile.replacedByDataFile?.hasValidReplacementPlan,
  );

  const setAllDataFiles = useCallback(
    (updater: Updater<DataFile[] | undefined, DataFile[] | undefined>) =>
      queryClient.setQueryData(
        releaseDataFileQueries.list(releaseVersionId).queryKey,
        updater,
      ),
    [releaseVersionId, queryClient],
  );
  const setAllDataUploads = useCallback(
    (
      updater: Updater<
        DataSetUpload[] | undefined,
        DataSetUpload[] | undefined
      >,
    ) =>
      queryClient.setQueryData(
        releaseDataFileQueries.listUploads(releaseVersionId).queryKey,
        updater,
      ),
    [releaseVersionId, queryClient],
  );

  const refreshDataFileLists = useCallback(async () => {
    await Promise.all([refetchDataFiles(), refetchDataSetUploads()]);
  }, [refetchDataFiles, refetchDataSetUploads]);

  const handleStatusChange = useCallback(
    async (dataFile: DataFile, importStatus: DataFileImportStatus) => {
      try {
        const permissions = await permissionService.getDataFilePermissions(
          releaseVersionId,
          dataFile.id,
        );
        setAllDataFiles(currentDataFiles =>
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
    [releaseVersionId, setAllDataFiles, refetchDataFiles],
  );

  const handleDataSetImport = useCallback(
    async (dataSetUploadIds: string[]) => {
      await releaseDataFileService.importDataSets(
        releaseVersionId,
        dataSetUploadIds,
      );

      setAllDataUploads(uploads =>
        uploads?.filter(upload => !dataSetUploadIds.includes(upload.id)),
      );

      await refreshDataFileLists();
    },
    [releaseVersionId, setAllDataUploads, refreshDataFileLists],
  );

  const handleDeleteConfirm = useCallback(
    async (deletedFileId: string) => {
      setAllDataFiles(files =>
        files?.filter(dataFile => dataFile.id !== deletedFileId),
      );
    },
    [setAllDataFiles],
  );

  const handleConfirmReordering = useCallback(
    async (nextDataFiles: DataFile[]) => {
      await releaseDataFileService.updateDataFilesOrder(
        releaseVersionId,
        nextDataFiles.map(file => file.id),
      );

      setAllDataFiles(() => nextDataFiles);
      toggleReordering.off();
    },
    [releaseVersionId, setAllDataFiles, toggleReordering],
  );

  const handleConfirmAllReplacements = async () => {
    await dataReplacementService.replaceData(
      releaseVersionId,
      validReplacementDataFiles.map(file => file.id),
    );
    await refetchDataFiles();
  };

  const errorFetchingData = dataFilesError || dataSetUploadsError;

  return (
    <>
      <h2>Add data file to release</h2>

      <DataUploadsGuidance />

      {canUpdateRelease ? (
        <DataFileUploadForm
          dataSetFileTitles={dataFilesExcludingReplacements.map(
            file => file.title,
          )}
          releaseVersionId={releaseVersionId}
          onSubmit={refreshDataFileLists}
        />
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

      <LoadingSpinner loading={isLoading || isLoadingUploads}>
        {(allDataFiles.length > 0 || allDataSetUploads.length > 0) &&
        !errorFetchingData ? (
          <>
            <h2>Uploaded data files</h2>

            {!isReordering && allDataFiles.length > 1 && (
              <div className="dfe-flex dfe-justify-content--space-between">
                <Button onClick={toggleReordering.on} variant="secondary">
                  Reorder data files
                </Button>
                {validReplacementDataFiles.length > 1 && (
                  <Button onClick={handleConfirmAllReplacements}>
                    Confirm all valid replacements
                  </Button>
                )}
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
                {(inProgressReplacementDataFiles.length > 0 ||
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
                    onRefreshUploads={refetchDataSetUploads}
                    onDeleteUpload={refreshDataFileLists}
                    onDataSetImport={handleDataSetImport}
                  />
                )}

                {(dataFilesExcludingReplacements.length > 0 ||
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
                    onDeleteUpload={refreshDataFileLists}
                    onDataSetImport={handleDataSetImport}
                    onEditFile={refreshDataFileLists}
                    onReplaceFile={refreshDataFileLists}
                    onRefreshUploads={refetchDataSetUploads}
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
