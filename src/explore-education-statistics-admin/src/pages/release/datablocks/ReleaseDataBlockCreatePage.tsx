import Link from '@admin/components/Link';
import PageMetaTitle from '@admin/components/PageMetaTitle';
import DataBlockPageTabs from '@admin/pages/release/datablocks/components/DataBlockPageTabs';
import {
  releaseDataBlockEditRoute,
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import permissionService from '@admin/services/permissionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useCallback } from 'react';
import { generatePath, useNavigate } from 'react-router';
import { useParams } from 'react-router-dom';

const ReleaseDataBlockCreatePage = () => {
  const navigate = useNavigate();
  const { publicationId, releaseVersionId } =
    useParams<ReleaseRouteParams>() as ReleaseRouteParams;

  const { value: canUpdateRelease, isLoading } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseVersionId),
    [releaseVersionId],
  );

  const handleDataBlockSave = useCallback(
    async (dataBlock: ReleaseDataBlock) => {
      navigate(
        generatePath(releaseDataBlockEditRoute.fullPath, {
          publicationId,
          releaseVersionId,
          dataBlockVersionId: dataBlock.id,
        }),
      );
    },
    [navigate, publicationId, releaseVersionId],
  );

  return (
    <>
      <PageMetaTitle title="Create data block" />
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath(releaseDataBlocksRoute.fullPath, {
          publicationId,
          releaseVersionId,
        })}
      >
        Back
      </Link>

      <LoadingSpinner loading={isLoading}>
        <h2>Create data block</h2>

        <section>
          {canUpdateRelease ? (
            <DataBlockPageTabs
              releaseVersionId={releaseVersionId}
              onDataBlockSave={handleDataBlockSave}
            />
          ) : (
            <WarningMessage>
              This release has been approved, and can no longer be updated.
            </WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataBlockCreatePage;
