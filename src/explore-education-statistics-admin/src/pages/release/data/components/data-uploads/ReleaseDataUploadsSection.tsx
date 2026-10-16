import DataUploadsPermissions from '@admin/pages/release/data/types/dataUploadsPermissions';
import { useAuthContext } from '@admin/contexts/AuthContext';
import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import dataReplacementService from '@admin/services/dataReplacementService';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import DataFileReplacementTable from '@admin/pages/release/data/components/data-uploads/data-file-replacements/DataFileReplacementTable';
import Button from '@common/components/Button';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { useQuery, useQueryClient, Updater } from '@tanstack/react-query';
import React, { useCallback, useMemo } from 'react';
import DataFilesTable from './DataFilesTable';
import DataUploadsGuidance from './DataUploadsGuidance';
import DataFileUploadForm from './DataFileUploadForm';
import DataFilesReorderableList from './DataFilesReorderableList';

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

  const { user } = useAuthContext();

  const permissions = useMemo<DataUploadsPermissions>(
    () => ({
      canUpdateRelease,
      canOverrideScreenerResult: user?.permissions.isBauUser ?? false,
      canManagePublicApiDataSets:
        user?.permissions.canManagePublicApiDataSets ?? false,
    }),
    [canUpdateRelease, user],
  );

  // Pre-import uploads include screening and awaiting-import states.
  // Import-stage files include running, completed and unsuccessful imports.
  const queryClient = useQueryClient();

  const {
    data: importStageDataFiles = [],
    isError: dataFilesError,
    isLoading,
    refetch: refetchDataFiles,
  } = useQuery(releaseDataFileQueries.list(releaseVersionId));
  const {
    data: preImportDataSetUploads = [],
    isError: dataSetUploadsError,
    isLoading: isLoadingUploads,
    refetch: refetchDataSetUploads,
  } = useQuery(releaseDataFileQueries.listUploads(releaseVersionId));

  const newDataSetUploads = useMemo(
    () => preImportDataSetUploads.filter(upload => !upload.replacingFileId),
    [preImportDataSetUploads],
  );

  const replacementDataSetUploads = useMemo(
    () => preImportDataSetUploads.filter(upload => upload.replacingFileId),
    [preImportDataSetUploads],
  );

  const dataFilesWithoutImportedReplacements = useMemo(
    () => importStageDataFiles.filter(dataFile => !dataFile.replacedByDataFile),
    [importStageDataFiles],
  );

  const dataFilesWithImportedReplacements = useMemo(
    () => importStageDataFiles.filter(dataFile => dataFile.replacedByDataFile),
    [importStageDataFiles],
  );

  const dataFilesWithValidReplacements =
    dataFilesWithImportedReplacements.filter(
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
        const dataFilePermissions =
          await permissionService.getDataFilePermissions(
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
                  permissions: dataFilePermissions,
                },
          ),
        );
      } catch {
        refetchDataFiles();
      }
    },
    [releaseVersionId, setAllDataFiles, refetchDataFiles],
  );

  const handleImportDataSets = useCallback(
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

  const handleDeleteFile = useCallback(
    async (deletedFileId: string) => {
      setAllDataFiles(files =>
        files?.filter(dataFile => dataFile.id !== deletedFileId),
      );
    },
    [setAllDataFiles],
  );

  const handleDeleteUpload = refreshDataFileLists;

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
      dataFilesWithValidReplacements.map(file => file.id),
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
          dataSetFileTitles={dataFilesWithoutImportedReplacements.map(
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
        {(importStageDataFiles.length > 0 ||
          preImportDataSetUploads.length > 0) &&
        !errorFetchingData ? (
          <>
            <h2>Uploaded data files</h2>

            {!isReordering && importStageDataFiles.length > 1 && (
              <div className="dfe-flex dfe-justify-content--space-between">
                <Button onClick={toggleReordering.on} variant="secondary">
                  Reorder data files
                </Button>
                {dataFilesWithValidReplacements.length > 1 && (
                  <Button onClick={handleConfirmAllReplacements}>
                    Confirm all valid replacements
                  </Button>
                )}
              </div>
            )}

            {isReordering ? (
              <DataFilesReorderableList
                dataFiles={importStageDataFiles}
                onCancelReordering={toggleReordering.off}
                onConfirmReordering={handleConfirmReordering}
              />
            ) : (
              <>
                {(dataFilesWithImportedReplacements.length > 0 ||
                  replacementDataSetUploads.length > 0) && (
                  <DataFileReplacementTable
                    permissions={permissions}
                    caption="Data file replacements"
                    dataFiles={dataFilesWithImportedReplacements}
                    dataSetUploads={replacementDataSetUploads}
                    publicationId={publicationId}
                    releaseVersionId={releaseVersionId}
                    testId="Data file replacements table"
                    onCancelReplacement={refetchDataFiles}
                    onConfirmReplacement={refetchDataFiles}
                    onRefreshUploads={refetchDataSetUploads}
                    onDeleteUpload={handleDeleteUpload}
                    onImportDataSets={handleImportDataSets}
                  />
                )}

                {(dataFilesWithoutImportedReplacements.length > 0 ||
                  newDataSetUploads.length > 0) && (
                  <DataFilesTable
                    permissions={permissions}
                    caption="Data files"
                    dataFiles={dataFilesWithoutImportedReplacements}
                    dataSetUploads={newDataSetUploads}
                    publicationId={publicationId}
                    releaseVersionId={releaseVersionId}
                    testId="Data files table"
                    onDeleteFile={handleDeleteFile}
                    onDeleteUpload={handleDeleteUpload}
                    onImportDataSets={handleImportDataSets}
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
