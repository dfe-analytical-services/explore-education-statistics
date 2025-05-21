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
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseDataFileReplacePage = ({
  history,
  match: {
    params: { publicationId, releaseVersionId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileReplaceRouteParams>) => {
  const {
    value: dataFile,
    isLoading: dataFileLoading,
    setState: setDataFile,
    retry: fetchDataFile,
  } = useAsyncHandledRetry(
    () => releaseDataFileService.getDataFile(releaseVersionId, fileId),
    [releaseVersionId, fileId],
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
    return releaseDataFileService.getDataFile(
      releaseVersionId,
      dataFile.replacedBy,
    );
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
          releaseVersionId,
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
      releaseVersionId,
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
      file = await releaseDataFileService.uploadDataFiles(releaseVersionId, {
        replacingFileId: currentFile.id,
        dataFile: values.dataFile as File,
        metadataFile: values.metadataFile as File,
      });
    } else {
      file = await releaseDataFileService.uploadZipDataFile(releaseVersionId, {
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
          releaseVersionId,
          file.id,
        ),
      },
    });
  };
  const cancelText =
    dataFile?.publicApiDataSetId === undefined
      ? 'Cancel data replacement'
      : 'Cancel data replacement & delete attached draft API version';
  const cancelBodyText =
    dataFile?.publicApiDataSetId === undefined ? (
      <>
        Are you sure you want to cancel this data replacement? The pending +
        replacement data file will be deleted.?
      </>
    ) : (
      <>
        <p>
          Are you sure you want to cancel this data replacement and remove the
          attached draft API version?
        </p>
        <p>
          Please note this doesn't affect the current live API data set in any
          way. You can reassign a data set version at any time prior to this
          release being published.
        </p>
      </>
    );
  const replacementCancelButton = (
    <ModalConfirm
      title={cancelText}
      triggerButton={<Button variant="secondary">{cancelText}</Button>}
      onConfirm={async () => {
        if (replacementDataFile?.id) {
          await releaseDataFileService.deleteDataFiles(
            releaseVersionId,
            replacementDataFile.id,
          );
        }

        fetchDataFile();
      }}
    >
      <p>{cancelBodyText}</p>
    </ModalConfirm>
  );

  const getReplacementPlanMessage = () => {
    if (replacementDataFile?.status === 'COMPLETE') {
      return null;
    }

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
          releaseVersionId,
        })}
      >
        Back
      </Link>

      <LoadingSpinner loading={dataFileLoading || replacementDataFileLoading}>
        {dataFile && (
          <>
            <section className="govuk-!-margin-bottom-8">
              <h2>Data file details</h2>

              {!!replacementDataFileError && (
                <WarningMessage>
                  There was a problem loading the replacement file details.
                </WarningMessage>
              )}

              <DataFileDetailsTable
                dataFile={dataFile}
                replacementDataFile={replacementDataFile}
                releaseVersionId={releaseVersionId}
                onStatusChange={handleStatusChange}
                onReplacementStatusChange={handleReplacementStatusChange}
              />
            </section>

            {!dataFile.replacedBy ? (
              <section>
                <h2>Upload replacement data</h2>

                <DataFileUploadForm
                  isDataReplacement
                  onSubmit={values => handleSubmit(dataFile, values)}
                />
              </section>
            ) : (
              <section>
                <h2>Pending data replacement</h2>

                {getReplacementPlanMessage()}

                {replacementDataFile?.status === 'COMPLETE' && (
                  <DataFileReplacementPlan
                    cancelButton={replacementCancelButton}
                    publicationId={publicationId}
                    releaseVersionId={releaseVersionId}
                    fileId={dataFile.id}
                    replacementFileId={replacementDataFile.id}
                    onReplacement={() => {
                      history.push(
                        generatePath<ReleaseDataFileReplaceRouteParams>(
                          releaseDataFileReplacementCompleteRoute.path,
                          {
                            publicationId,
                            releaseVersionId,
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
    </>
  );
};

export default ReleaseDataFileReplacePage;
