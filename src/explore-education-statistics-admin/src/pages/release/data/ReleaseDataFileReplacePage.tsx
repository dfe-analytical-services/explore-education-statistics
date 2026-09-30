import Link from '@admin/components/Link';
import DataFileDetailsTable from '@admin/pages/release/data/components/DataFileDetailsTable';
import {
  ReleaseDataFileReplaceRouteParams,
  releaseDataRoute,
} from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseDataFileService, {
  DataFile,
  DataFileImportStatus,
} from '@admin/services/releaseDataFileService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import { generatePath } from 'react-router';
import PendingDataReplacementSection from '@admin/pages/release/data/components/PendingDataReplacementSection';
import { useParams } from 'react-router-dom';

const ReleaseDataFileReplacePage = () => {
  const { publicationId, releaseVersionId, fileId } =
    useParams<ReleaseDataFileReplaceRouteParams>() as ReleaseDataFileReplaceRouteParams;

  const {
    value: dataFile,
    isLoading: dataFileLoading,
    setState: setDataFile,
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
    if (!dataFile?.replacedByDataFileId) {
      return undefined;
    }
    return releaseDataFileService.getDataFile(
      releaseVersionId,
      dataFile.replacedByDataFileId,
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

  return (
    <>
      <Link
        className="govuk-!-margin-bottom-6"
        back
        to={generatePath(releaseDataRoute.fullPath, {
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
            <PendingDataReplacementSection
              dataFileId={dataFile.id}
              replacementDataFileError={replacementDataFileError}
              replacementDataFile={replacementDataFile}
              publicationId={publicationId}
              releaseVersionId={releaseVersionId}
              publicApiDataSetId={dataFile.publicApiDataSetId}
            />
          </>
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataFileReplacePage;
