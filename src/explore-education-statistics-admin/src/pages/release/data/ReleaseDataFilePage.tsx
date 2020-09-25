import Link from '@admin/components/Link';
import useQueryParams from '@admin/hooks/useQueryParams';
import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import DataFileSummaryList from '@admin/pages/release/data/components/DataFileSummaryList';
import {
  releaseDataFileRoute,
  ReleaseDataFileRouteParams,
  releaseDataRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseDataFilePage = ({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileRouteParams>) => {
  // Temporarily use query params to set the replacement file id
  // TODO: Remove this when upload APIs are in place.
  const { replacementFileId } = useQueryParams<{ replacementFileId: string }>();

  const {
    value: dataFile,
    isLoading,
    retry: fetchDataFile,
  } = useAsyncHandledRetry(
    () => releaseDataFileService.getDataFile(releaseId, fileId),
    [releaseId, fileId],
  );

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

      <LoadingSpinner loading={isLoading}>
        {dataFile && (
          <>
            <h2>Current data file</h2>

            <DataFileSummaryList dataFile={dataFile} releaseId={releaseId} />

            {/*
            TODO: Add upload form when API is in place (EES-1399)
            <h2>Upload replacement data</h2>

            <DataFileUploadForm
              submitText="Upload replacement data"
              onSubmit={handleSubmit}
            />
            */}

            {replacementFileId && (
              <>
                <h2>Pending data replacement</h2>

                <DataFileReplacementPlan
                  publicationId={publicationId}
                  releaseId={releaseId}
                  fileId={dataFile.id}
                  replacementFileId={replacementFileId}
                  onCancel={fetchDataFile}
                  onReplacement={() => {
                    history.push(
                      generatePath<ReleaseDataFileRouteParams>(
                        releaseDataFileRoute.path,
                        {
                          publicationId,
                          releaseId,
                          fileId: replacementFileId,
                        },
                      ),
                    );
                  }}
                />
              </>
            )}
          </>
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataFilePage;
