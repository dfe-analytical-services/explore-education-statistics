import Link from '@admin/components/Link';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import {
  releaseDataFileReplacementCompleteRoute,
  ReleaseDataFileRouteParams,
  releaseDataRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  DataFile,
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

const ReleaseDataFilePage = ({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileRouteParams>) => {
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
                onStatusChange={async (file, { status }) => {
                  setDataFile({
                    value: {
                      ...file,
                      status,
                      permissions: await permissionService.getDataFilePermissions(
                        releaseId,
                        file.fileName,
                      ),
                    },
                  });
                }}
                onReplacementStatusChange={async (file, { status }) => {
                  setReplacementDataFile({
                    value: {
                      ...file,
                      status,
                      permissions: await permissionService.getDataFilePermissions(
                        releaseId,
                        file.fileName,
                      ),
                    },
                  });
                }}
              />
            </section>

            {!dataFile.replacedBy ? (
              <section>
                <h2>Upload replacement data</h2>

                <DataFileUploadForm
                  onSubmit={async values => {
                    let file: DataFile;

                    if (values.uploadType === 'csv') {
                      file = await releaseDataFileService.uploadDataFiles(
                        releaseId,
                        {
                          replacingFileId: dataFile.id,
                          dataFile: values.dataFile as File,
                          metadataFile: values.metadataFile as File,
                        },
                      );
                    } else {
                      file = await releaseDataFileService.uploadZipDataFile(
                        releaseId,
                        {
                          replacingFileId: dataFile.id,
                          zipFile: values.zipFile as File,
                        },
                      );
                    }

                    setDataFile({
                      value: {
                        ...dataFile,
                        replacedBy: file.id,
                      },
                    });
                    setReplacementDataFile({
                      value: {
                        ...file,
                        permissions: await permissionService.getDataFilePermissions(
                          releaseId,
                          file.fileName,
                        ),
                      },
                    });
                  }}
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
                        generatePath<ReleaseDataFileRouteParams>(
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
        mounted={isCancelling}
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

export default ReleaseDataFilePage;
