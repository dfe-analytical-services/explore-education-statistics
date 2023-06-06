import Link from '@admin/components/Link';
import AncillaryFileForm, {
  AncillaryFileFormValues,
} from '@admin/pages/release/data/components/AncillaryFileForm';
import releaseAncillaryFileQueries from '@admin/queries/releaseAncillaryFileQueries';
import {
  ReleaseAncillaryFileRouteParams,
  releaseAncillaryFilesRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseAncillaryFileService from '@admin/services/releaseAncillaryFileService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

export default function ReleaseAncillaryFilePage({
  history,
  match: {
    params: { publicationId, releaseId, fileId },
  },
}: RouteComponentProps<ReleaseAncillaryFileRouteParams>) {
  const { data: file, isLoading: isLoadingFile } = useQuery(
    releaseAncillaryFileQueries.get(releaseId, fileId),
  );

  const { data: allFiles = [], isLoading: isLoadingAllFiles } = useQuery(
    releaseAncillaryFileQueries.list(releaseId),
  );

  const navigateBack = useCallback(
    () =>
      history.push(
        generatePath<ReleaseRouteParams>(releaseAncillaryFilesRoute.path, {
          publicationId,
          releaseId,
        }),
      ),
    [history, publicationId, releaseId],
  );

  const handleSubmit = useCallback(
    async ({ title, summary, file: newFile }: AncillaryFileFormValues) => {
      await Promise.all([
        releaseAncillaryFileService.updateFile(releaseId, fileId, {
          title,
          summary,
        }),
        newFile
          ? releaseAncillaryFileService.replaceFile(releaseId, fileId, newFile)
          : undefined,
      ]);

      navigateBack();
    },
    [fileId, navigateBack, releaseId],
  );

  return (
    <>
      <Link
        className="govuk-!-margin-bottom-6"
        back
        to={generatePath<ReleaseRouteParams>(releaseAncillaryFilesRoute.path, {
          publicationId,
          releaseId,
        })}
      >
        Back
      </Link>

      <LoadingSpinner loading={isLoadingFile || isLoadingAllFiles}>
        <section>
          <h2>Edit ancillary file</h2>

          {file ? (
            <AncillaryFileForm
              files={allFiles.filter(f => f.id !== file.id)}
              fileFieldLabel="Upload new file"
              initialValues={{
                title: file.title,
                summary: file.summary,
                file: null,
              }}
              onCancel={navigateBack}
              onSubmit={handleSubmit}
            />
          ) : (
            <WarningMessage>
              Could not load ancillary file details
            </WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
}
