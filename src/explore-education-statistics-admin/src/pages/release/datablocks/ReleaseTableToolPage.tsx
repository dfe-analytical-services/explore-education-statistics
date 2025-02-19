import Link from '@admin/components/Link';
import {
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ReleasePreviewTableTool from '@admin/pages/release/content/components/ReleasePreviewTableTool';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router-dom';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import { useQuery } from '@tanstack/react-query';
import publicationQueries from '@admin/queries/publicationQueries';

const ReleaseTableToolPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { releaseVersionId, publicationId } = match.params;
  const { releaseVersion } = useReleaseVersionContext();

  const { data: publication, isLoading } = useQuery(
    publicationQueries.get(publicationId),
  );

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseRouteParams>(releaseDataBlocksRoute.path, {
          publicationId,
          releaseVersionId,
        })}
      >
        Back
      </Link>
      <LoadingSpinner loading={isLoading}>
        {publication && (
          <ReleasePreviewTableTool
            releaseVersionId={releaseVersionId}
            publication={publication}
            releaseType={releaseVersion.type}
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseTableToolPage;
