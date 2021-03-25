import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import DataBlockDeletePlanModal from '@admin/pages/release/datablocks/components/DataBlockDeletePlanModal';
import DataBlockPageReadOnlyTabs from '@admin/pages/release/datablocks/components/DataBlockPageReadOnlyTabs';
import DataBlockPageTabs from '@admin/pages/release/datablocks/components/DataBlockPageTabs';
import DataBlockSelector from '@admin/pages/release/datablocks/components/DataBlockSelector';
import {
  releaseDataBlockCreateRoute,
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
import ButtonLink from '@admin/components/ButtonLink';
import FormLabel from '@common/components/form/FormLabel';

interface Model {
  dataBlock: ReleaseDataBlock;
  canUpdateRelease: boolean;
}

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

  const { value: model, isLoading, setState: setModel } = useAsyncHandledRetry<
    Model
  >(async () => {
    const [dataBlock, canUpdateRelease] = await Promise.all([
      dataBlocksService.getDataBlock(dataBlockId),
      permissionService.canUpdateRelease(releaseId),
    ]);

    return {
      dataBlock,
      canUpdateRelease,
    };
  }, [releaseId, dataBlockId]);

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
        releaseId,
      }),
    );
  }, [history, publicationId, releaseId]);

  const { canUpdateRelease, dataBlock } = model ?? {};

  const createPath = generatePath<ReleaseRouteParams>(
    releaseDataBlockCreateRoute.path,
    {
      publicationId,
      releaseId,
    },
  );

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

      <LoadingSpinner loading={isLoading}>
        <h2>{canUpdateRelease ? 'Edit data block' : 'View data block'}</h2>

        <FormLabel
          id="selectedDataBlock"
          label={
            canUpdateRelease
              ? 'Select a data block to edit'
              : 'Select a data block to view'
          }
        />
        <div className="dfe-flex dfe-align-items--center govuk-!-margin-top-1">
          <DataBlockSelector
            canUpdate={canUpdateRelease}
            publicationId={publicationId}
            releaseId={releaseId}
            label={false}
            dataBlockId={dataBlockId}
          />
          <p className="govuk-!-font-weight-bold govuk-!-margin-right-4 govuk-!-margin-left-4 govuk-!-margin-bottom-0">
            or
          </p>
          <ButtonLink
            className="govuk-!-margin-0"
            to={createPath}
            onClick={toggleDeleting.on}
          >
            Create another data block
          </ButtonLink>
        </div>

        <hr className="govuk-!-margin-bottom-6" />

        {dataBlock && (
          <>
            <h2 className="govuk-heading-m">{dataBlock.name}</h2>

            <section>
              <SummaryList smallKey noBorder>
                {!canUpdateRelease && (
                  <>
                    {dataBlock.highlightName && (
                      <SummaryListItem term="Highlight name">
                        {dataBlock.highlightName || 'None'}
                      </SummaryListItem>
                    )}

                    {dataBlock.highlightDescription && (
                      <SummaryListItem term="Highlight description">
                        {dataBlock.highlightDescription || 'None'}
                      </SummaryListItem>
                    )}
                  </>
                )}

                <SummaryListItem term="Fast track URL">
                  <UrlContainer
                    data-testid="fastTrackUrl"
                    url={`${config.PublicAppUrl}/data-tables/fast-track/${dataBlockId}`}
                  />
                </SummaryListItem>
              </SummaryList>

              {canUpdateRelease && (
                <Button variant="warning" onClick={toggleDeleting.on}>
                  Delete this data block
                </Button>
              )}

              {canUpdateRelease ? (
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

            {isDeleting && canUpdateRelease && (
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
