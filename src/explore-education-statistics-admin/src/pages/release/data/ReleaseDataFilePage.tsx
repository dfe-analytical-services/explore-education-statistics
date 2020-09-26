import Link from '@admin/components/Link';
import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
import DataFileUploadForm from '@admin/pages/release/data/components/DataFileUploadForm';
import {
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
  releaseDataRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseDataFilePage = ({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileRouteParams>) => {
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
  } = useAsyncRetry(async () => {
    if (!dataFile?.replacedBy) {
      return undefined;
    }

    return releaseDataFileService.getDataFile(releaseId, dataFile.replacedBy);
  }, [dataFile]);

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

              <DataFileDetailsTable
                dataFile={dataFile}
                replacementDataFile={replacementDataFile}
                releaseId={releaseId}
                onStatusChange={(file, { status }) => {
                  setDataFile({
                    value: {
                      ...file,
                      status,
                    },
                  });
                }}
                onReplacementStatusChange={(file, { status }) => {
                  setReplacementDataFile({
                    value: {
                      ...file,
                      status,
                    },
                  });
                }}
              />
            </section>

            {!dataFile.replacedBy ? (
              <section>
                <h2>Upload replacement data</h2>

                <DataFileUploadForm
                  submitText="Upload replacement data"
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
                      value: file,
                    });
                  }}
                />
              </section>
            ) : (
              <section>
                <h2>Pending data replacement</h2>

                {replacementDataFile?.status === 'COMPLETE' && (
                  <DataFileReplacementPlan
                    publicationId={publicationId}
                    releaseId={releaseId}
                    fileId={dataFile.id}
                    replacementFileId={replacementDataFile.id}
                    onCancel={fetchDataFile}
                    onReplacement={() => {
                      history.push(
                        generatePath<ReleaseDataFileRouteParams>(
                          releaseDataFileRoute.path,
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
    </>
  );
};

export default ReleaseDataFilePage;
