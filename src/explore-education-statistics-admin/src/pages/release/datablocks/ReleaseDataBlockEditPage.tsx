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

interface Model {
  dataBlock: ReleaseDataBlock;
  canUpdateRelease: boolean;
}

const ReleaseDataBlockEditPage = ({
  match,
  history,
}: RouteComponentProps<ReleaseDataBlockRouteParams>) => {
  const {
    params: { publicationId, releaseVersionId, dataBlockId },
  } = match;

  const config = useConfig();
  const pageRef = useRef<HTMLDivElement>(null);

  const [isDeleting, toggleDeleting] = useToggle(false);

  const {
    value: model,
    isLoading,
    setState: setModel,
  } = useAsyncHandledRetry<Model>(async () => {
    const [dataBlock, canUpdateRelease] = await Promise.all([
      dataBlocksService.getDataBlock(dataBlockId),
      permissionService.canUpdateRelease(releaseVersionId),
    ]);

    return {
      dataBlock,
      canUpdateRelease,
    };
  }, [releaseVersionId, dataBlockId]);

  const handleDataBlockSave = useCallback(
    async (nextDataBlock: ReleaseDataBlock) => {
      if (!model) {
        return;
      }

      setModel({
        value: {
          ...model,
          dataBlock: nextDataBlock,
        },
      });

      if (pageRef.current) {
        pageRef.current.scrollIntoView({
          behavior: 'smooth',
          block: 'start',
        });
      }
    },
    [model, setModel],
  );

  const handleDataBlockDelete = useCallback(() => {
    history.push(
      generatePath<ReleaseRouteParams>(releaseDataBlocksRoute.path, {
        publicationId,
        releaseVersionId,
      }),
    );
  }, [history, publicationId, releaseVersionId]);

  const { canUpdateRelease, dataBlock } = model ?? {};

  return (
    <div ref={pageRef}>
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
        <h2>{canUpdateRelease ? 'Edit data block' : 'View data block'}</h2>

        <DataBlockSelector
          canUpdate={canUpdateRelease}
          publicationId={publicationId}
          releaseVersionId={releaseVersionId}
          dataBlockId={dataBlockId}
        />

        <hr className="govuk-!-margin-bottom-6" />

        {dataBlock && (
          <>
            <h2 className="govuk-heading-m">{dataBlock.name}</h2>

            <section>
              <SummaryList smallKey noBorder>
                {!canUpdateRelease && (
                  <>
                    {dataBlock.highlightName && (
                      <SummaryListItem term="Featured table name">
                        {dataBlock.highlightName || 'None'}
                      </SummaryListItem>
                    )}

                    {dataBlock.highlightDescription && (
                      <SummaryListItem term="Featured table description">
                        {dataBlock.highlightDescription || 'None'}
                      </SummaryListItem>
                    )}
                  </>
                )}

                <SummaryListItem term="Fast track URL">
                  <UrlContainer
                    id="fastTrackUrl"
                    url={`${config.publicAppUrl}/data-tables/fast-track/${dataBlock.dataBlockParentId}`}
                  />
                </SummaryListItem>
                {dataBlock.dataSetName && (
                  <SummaryListItem term="Data set name">
                    {dataBlock.dataSetName}
                  </SummaryListItem>
                )}
              </SummaryList>

              {canUpdateRelease && (
                <DataBlockDeletePlanModal
                  open={isDeleting}
                  releaseVersionId={releaseVersionId}
                  dataBlockId={dataBlockId}
                  triggerButton={
                    <Button variant="warning" onClick={toggleDeleting.on}>
                      Delete this data block
                    </Button>
                  }
                  onConfirm={handleDataBlockDelete}
                  onCancel={toggleDeleting.off}
                  onExit={toggleDeleting.off}
                />
              )}

              {canUpdateRelease ? (
                <DataBlockPageTabs
                  key={dataBlockId}
                  releaseVersionId={releaseVersionId}
                  dataBlock={dataBlock}
                  onDataBlockSave={handleDataBlockSave}
                />
              ) : (
                <DataBlockPageReadOnlyTabs
                  releaseVersionId={releaseVersionId}
                  dataBlock={dataBlock}
                />
              )}
            </section>
          </>
        )}
      </LoadingSpinner>
    </div>
  );
};

export default ReleaseDataBlockEditPage;
