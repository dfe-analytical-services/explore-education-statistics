import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import DataBlockDeletePlanModal from '@admin/pages/release/datablocks/components/DataBlockDeletePlanModal';
import DataBlockPageReadOnlyTabs from '@admin/pages/release/datablocks/components/DataBlockPageReadOnlyTabs';
import DataBlockPageTabs from '@admin/pages/release/datablocks/components/DataBlockPageTabs';
import DataBlockSelector from '@admin/pages/release/datablocks/components/DataBlockSelector';
import {
  ReleaseDataBlockRouteParams,
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import dataBlocksService, {
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import permissionService from '@admin/services/permissionService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
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

  const { value: canUpdate = false } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseId),
    [releaseId],
  );

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

      <h2>{canUpdate ? 'Edit data block' : 'View data block'}</h2>

      <DataBlockSelector
        canUpdate={canUpdate}
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
              <SummaryList smallKey noBorder>
                {!canUpdate && (
                  <SummaryListItem term="Highlight name">
                    {dataBlock.highlightName}
                  </SummaryListItem>
                )}

                <SummaryListItem term="Fast track URL">
                  <UrlContainer
                    data-testid="fastTrackUrl"
                    url={`${config.PublicAppUrl}/data-tables/fast-track/${dataBlockId}`}
                  />
                </SummaryListItem>
              </SummaryList>

              {canUpdate && (
                <Button variant="warning" onClick={toggleDeleting.on}>
                  Delete this data block
                </Button>
              )}

              {canUpdate ? (
                <DataBlockPageTabs
                  key={dataBlockId}
                  releaseId={releaseId}
                  dataBlock={dataBlock}
                  onDataBlockSave={handleDataBlockSave}
                />
              ) : (
                <DataBlockPageReadOnlyTabs
                  releaseId={releaseId}
                  dataBlock={dataBlock}
                />
              )}
            </section>

            {isDeleting && canUpdate && (
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
