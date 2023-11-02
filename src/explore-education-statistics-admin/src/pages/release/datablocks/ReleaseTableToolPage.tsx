import Link from '@admin/components/Link';
import {
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ReleasePreviewTableTool from '@admin/pages/release/content/components/ReleasePreviewTableTool';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import publicationService, {
  Publication,
} from '@admin/services/publicationService';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router-dom';
import releaseService, { Release } from '@admin/services/releaseService';

interface PublicationAndRelease {
  publication: Publication;
  release: Release;
}

const ReleaseTableToolPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { releaseId, publicationId } = match.params;

  const { value: publicationAndRelease, isLoading } = useAsyncHandledRetry<
    PublicationAndRelease | undefined
  >(async () => {
    const [publication, release] = await Promise.all([
      publicationService.getPublication(publicationId),
      releaseService.getRelease(releaseId),
    ]);

    return {
      publication,
      release,
    };
  }, [releaseId]);

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseRouteParams>(releaseDataBlocksRoute.path, {
          publicationId,
          releaseId,
        })}
      >
        Back
      </Link>
      <LoadingSpinner loading={isLoading}>
        {publicationAndRelease && (
          <ReleasePreviewTableTool
            releaseId={releaseId}
            publication={publicationAndRelease.publication}
            releaseType={publicationAndRelease.release.type}
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseTableToolPage;
