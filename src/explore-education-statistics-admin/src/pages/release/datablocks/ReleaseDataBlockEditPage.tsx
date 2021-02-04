import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import DataBlockDeletePlanModal from '@admin/pages/release/datablocks/components/DataBlockDeletePlanModal';
import DataBlockSelector from '@admin/pages/release/datablocks/components/DataBlockSelector';
import DataBlockPageTabs from '@admin/pages/release/datablocks/components/DataBlockPageTabs';
import {
  ReleaseDataBlockRouteParams,
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import dataBlocksService, {
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import UrlContainer from '@common/components/UrlContainer';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import React, { useCallback, useRef } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const ReleaseDataBlockEditPage = ({
  match,
  history,
}: RouteComponentProps<ReleaseDataBlockRouteParams>) => {
  const {
    params: { publicationId, releaseId, dataBlockId },
  } = match;

  const config = useConfig();
  const pageRef = useRef<HTMLDivElement>(null);

  const [isDeleting, toggleDeleting] = useToggle(false);

  const {
    value: dataBlock,
    isLoading,
    setState: setDataBlock,
  } = useAsyncHandledRetry(() => dataBlocksService.getDataBlock(dataBlockId), [
    dataBlockId,
  ]);

  const handleDataBlockSave = useCallback(
    async (nextDataBlock: ReleaseDataBlock) => {
      setDataBlock({
        value: nextDataBlock,
      });

      if (pageRef.current) {
        pageRef.current.scrollIntoView({
          behavior: 'smooth',
          block: 'start',
        });
      }
    },
    [setDataBlock],
  );

  const handleDataBlockDelete = useCallback(() => {
    history.push(
      generatePath<ReleaseRouteParams>(releaseDataBlocksRoute.path, {
        publicationId,
        releaseId,
      }),
    );
  }, [history, publicationId, releaseId]);

  return (
    <div ref={pageRef}>
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

      <h2>Edit data block</h2>

      <DataBlockSelector
        publicationId={publicationId}
        releaseId={releaseId}
        dataBlockId={dataBlockId}
      />

      <hr className="govuk-!-margin-bottom-6" />

      <LoadingSpinner loading={isLoading}>
        {dataBlock && (
          <>
            <h2 className="govuk-heading-m">{dataBlock.name}</h2>

            <section>
              <p className="govuk-!-margin-bottom-6">
                <strong>Fast track URL:</strong>

                <UrlContainer
                  className="govuk-!-margin-left-4"
                  data-testid="fastTrackUrl"
                  url={`${config.PublicAppUrl}/data-tables/fast-track/${dataBlockId}`}
                />
              </p>

              <Button
                type="button"
                variant="warning"
                onClick={toggleDeleting.on}
              >
                Delete this data block
              </Button>

              <DataBlockPageTabs
                key={dataBlockId}
                releaseId={releaseId}
                dataBlock={dataBlock}
                onDataBlockSave={handleDataBlockSave}
              />
            </section>

            {isDeleting && (
              <DataBlockDeletePlanModal
                releaseId={releaseId}
                dataBlockId={dataBlockId}
                onConfirm={handleDataBlockDelete}
                onCancel={toggleDeleting.off}
                onExit={toggleDeleting.off}
              />
            )}
          </>
        )}
      </LoadingSpinner>
    </div>
  );
};

export default ReleaseDataBlockEditPage;
