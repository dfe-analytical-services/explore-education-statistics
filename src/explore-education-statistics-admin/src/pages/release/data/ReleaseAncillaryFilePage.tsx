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
    params: { publicationId, releaseVersionId, fileId },
  },
}: RouteComponentProps<ReleaseAncillaryFileRouteParams>) {
  const { data: file, isLoading: isLoadingFile } = useQuery(
    releaseAncillaryFileQueries.get(releaseVersionId, fileId),
  );

  const { data: allFiles = [], isLoading: isLoadingAllFiles } = useQuery(
    releaseAncillaryFileQueries.list(releaseVersionId),
  );

  const navigateBack = useCallback(
    () =>
      history.push(
        generatePath<ReleaseRouteParams>(releaseAncillaryFilesRoute.path, {
          publicationId,
          releaseVersionId,
        }),
      ),
    [history, publicationId, releaseVersionId],
  );

  const handleSubmit = useCallback(
    async ({ title, summary, file: newFile }: AncillaryFileFormValues) => {
      await releaseAncillaryFileService.updateFile(releaseVersionId, fileId, {
        title,
        summary,
        file: newFile,
      });

      navigateBack();
    },
    [fileId, navigateBack, releaseVersionId],
  );

  return (
    <>
      <Link
        className="govuk-!-margin-bottom-6"
        back
        to={generatePath<ReleaseRouteParams>(releaseAncillaryFilesRoute.path, {
          publicationId,
          releaseVersionId,
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
              initialValues={{
                title: file.title,
                summary: file.summary,
                file: null,
              }}
              isEditing
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
