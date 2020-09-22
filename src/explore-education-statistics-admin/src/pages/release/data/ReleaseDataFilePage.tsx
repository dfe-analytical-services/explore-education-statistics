import Link from '@admin/components/Link';
import useQueryParams from '@admin/hooks/useQueryParams';
import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import DataFileSummaryList from '@admin/pages/release/data/components/DataFileSummaryList';
import {
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
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseDataFileRouteParams>) => {
  // Temporarily use query params to set the replacement file id
  // TODO: Remove this when upload APIs are in place.
  const { replacementFileId } = useQueryParams<{ replacementFileId: string }>();

  const { value: dataFile, isLoading } = useAsyncHandledRetry(
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
                  fileId={dataFile.id}
                  replacementFileId={replacementFileId}
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
