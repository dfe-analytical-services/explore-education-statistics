import Link from '@admin/components/Link';
import {
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ReleasePreviewTableTool from '@admin/pages/release/content/components/ReleasePreviewTableTool';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import publicationService from '@admin/services/publicationService';
import { generatePath } from 'react-router-dom';
import React from 'react';
import { useParams } from 'react-router';

const ReleaseTableToolPage = () => {
  const { publicationId, releaseId } = useParams<ReleaseRouteParams>();

  const { value: publication, isLoading } = useAsyncHandledRetry(
    () => publicationService.getPublication(publicationId),
    [releaseId],
  );

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
        {publication && (
          <ReleasePreviewTableTool
            releaseId={releaseId}
            publication={publication}
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseTableToolPage;
