import Link from '@admin/components/Link';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import DataFileUploadForm, {
  DataFileUploadFormValues,
} from '@admin/pages/release/data/components/DataFileUploadForm';
import {
  releaseDataFileReplacementCompleteRoute,
  ReleaseDataFileReplaceRouteParams,
  releaseDataRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
} from '@admin/services/releaseDataFileService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseDataFileReplacePage = ({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileReplaceRouteParams>) => {
  const [isCancelling, toggleCancelling] = useToggle(false);
  const {
    value: dataFile,
    isLoading: dataFileLoading,
    setState: setDataFile,
    retry: fetchDataFile,
  } = useAsyncHandledRetry(
    () => releaseDataFileService.getDataFile(releaseId, fileId),
    [releaseId, fileId],
  );

  const {
    value: replacementDataFile,
    isLoading: replacementDataFileLoading,
    setState: setReplacementDataFile,
    error: replacementDataFileError,
  } = useAsyncRetry(async () => {
    if (!dataFile?.replacedBy) {
      return undefined;
    }
    return releaseDataFileService.getDataFile(releaseId, dataFile.replacedBy);
  }, [dataFile]);

  const handleStatusChange = async (
    file: DataFile,
    { totalRows, status }: DataFileImportStatus,
  ) => {
    setDataFile({
      value: {
        ...file,
        rows: totalRows,
        status,
        permissions: await permissionService.getDataFilePermissions(
          releaseId,
          file.id,
        ),
      },
    });
  };

  const handleReplacementStatusChange = async (
    file: DataFile,
    { totalRows, status }: DataFileImportStatus,
  ) => {
    const permissions = await permissionService.getDataFilePermissions(
      releaseId,
      file.id,
    );
    setReplacementDataFile({
      value: {
        ...file,
        rows: totalRows,
        status,
        permissions,
      },
    });
  };

  const handleSubmit = async (
    currentFile: DataFile,
    values: DataFileUploadFormValues,
  ) => {
    let file: DataFile;

    if (values.uploadType === 'csv') {
      file = await releaseDataFileService.uploadDataFiles(releaseId, {
        replacingFileId: currentFile.id,
        dataFile: values.dataFile as File,
        metadataFile: values.metadataFile as File,
      });
    } else {
      file = await releaseDataFileService.uploadZipDataFile(releaseId, {
        replacingFileId: currentFile.id,
        zipFile: values.zipFile as File,
      });
    }

    setDataFile({
      value: {
        ...currentFile,
        replacedBy: file.id,
      },
    });
    setReplacementDataFile({
      value: {
        ...file,
        permissions: await permissionService.getDataFilePermissions(
          releaseId,
          file.id,
        ),
      },
    });
  };

  const getReplacementPlanMessage = () => {
    if (replacementDataFile?.status === 'COMPLETE') {
      return null;
    }

    const replacementCancelButton = (
      <Button onClick={toggleCancelling.on}>Cancel data replacement</Button>
    );

    if (replacementDataFileError) {
      return (
        <>
          <WarningMessage>
            There was a problem loading the data replacement information.
          </WarningMessage>

          {replacementCancelButton}
        </>
      );
    }

    if (replacementDataFile?.status === 'FAILED') {
      return (
        <>
          <WarningMessage>
            Replacement data file import failed. Please cancel and try again.
          </WarningMessage>

          {replacementCancelButton}
        </>
      );
    }

    return (
      <WarningMessage>
        The replacement data file is still being processed. Data replacement
        cannot continue until it has completed.
      </WarningMessage>
    );
  };

  return (
    <>
      <Link
        className="govuk-!-margin-bottom-6"
        back
        to={generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
          publicationId,
          releaseId,
        })}
      >
        Back
      </Link>

      <LoadingSpinner loading={dataFileLoading || replacementDataFileLoading}>
        {dataFile && (
          <>
            <section className="govuk-!-margin-bottom-8">
              <h2>Data file details</h2>

              {replacementDataFileError && (
                <WarningMessage>
                  There was a problem loading the replacement file details.
                </WarningMessage>
              )}

              <DataFileDetailsTable
                dataFile={dataFile}
                replacementDataFile={replacementDataFile}
                releaseId={releaseId}
                onStatusChange={handleStatusChange}
                onReplacementStatusChange={handleReplacementStatusChange}
              />
            </section>

            {!dataFile.replacedBy ? (
              <section>
                <h2>Upload replacement data</h2>

                <DataFileUploadForm
                  onSubmit={values => handleSubmit(dataFile, values)}
                />
              </section>
            ) : (
              <section>
                <h2>Pending data replacement</h2>

                {getReplacementPlanMessage()}

                {replacementDataFile?.status === 'COMPLETE' && (
                  <DataFileReplacementPlan
                    publicationId={publicationId}
                    releaseId={releaseId}
                    fileId={dataFile.id}
                    replacementFileId={replacementDataFile.id}
                    onCancel={toggleCancelling.on}
                    onReplacement={() => {
                      history.push(
                        generatePath<ReleaseDataFileReplaceRouteParams>(
                          releaseDataFileReplacementCompleteRoute.path,
                          {
                            publicationId,
                            releaseId,
                            fileId: replacementDataFile.id,
                          },
                        ),
                      );
                    }}
                  />
                )}
              </section>
            )}
          </>
        )}
      </LoadingSpinner>
      <ModalConfirm
        title="Cancel data replacement"
        open={isCancelling}
        onExit={toggleCancelling.off}
        onConfirm={async () => {
          toggleCancelling.off();

          if (replacementDataFile?.id) {
            await releaseDataFileService.deleteDataFiles(
              releaseId,
              replacementDataFile.id,
            );
          }

          fetchDataFile();
        }}
      >
        <p>
          Are you sure you want to cancel this data replacement? The pending
          replacement data file will be deleted.
        </p>
      </ModalConfirm>
    </>
  );
};

export default ReleaseDataFileReplacePage;
