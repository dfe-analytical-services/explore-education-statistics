import Link from '@admin/components/Link';
import {
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseContentService from '@admin/services/releaseContentService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ReleasePreviewTableTool from '@admin/pages/release/content/components/ReleasePreviewTableTool';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router-dom';

const ReleaseTableToolPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { releaseId, publicationId } = match.params;

  const { value, isLoading } = useAsyncHandledRetry(
    () => releaseContentService.getContent(releaseId),
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
        {value && (
          <ReleasePreviewTableTool
            releaseId={releaseId}
            publication={value.release.publication}
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseTableToolPage;
