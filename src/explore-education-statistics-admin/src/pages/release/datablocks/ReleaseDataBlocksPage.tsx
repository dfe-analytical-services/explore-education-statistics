import { useConfig } from '@admin/contexts/ConfigContext';
import ReleaseDataBlocksPageTabs from '@admin/pages/release/datablocks/components/ReleaseDataBlocksPageTabs';
import {
  releaseDataBlocksRoute,
  ReleaseDataBlocksRouteParams,
} from '@admin/routes/releaseRoutes';
import dataBlocksService, {
  DeleteDataBlockPlan,
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import permissionService from '@admin/services/permissionService';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import Gate from '@common/components/Gate';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import UrlContainer from '@common/components/UrlContainer';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React, { useCallback, useMemo, useRef, useState } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const emptyDataBlocks: ReleaseDataBlock[] = [];

const ReleaseDataBlocksPageInternal = ({
  match,
  history,
}: RouteComponentProps<ReleaseDataBlocksRouteParams>) => {
  const { publicationId, releaseId, dataBlockId } = match.params;

  const pageRef = useRef<HTMLDivElement>(null);

  const config = useConfig();

  const [deletePlan, setDeletePlan] = useState<DeleteDataBlockPlan>();

  const {
    value: dataBlocks = emptyDataBlocks,
    isLoading,
    retry: fetchDataBlocks,
    setState: setDataBlocks,
  } = useAsyncRetry(() => dataBlocksService.getDataBlocks(releaseId), [
    releaseId,
  ]);

  const dataBlockOptions = useMemo(
    () =>
      dataBlocks.map(({ name, id }, index) => ({
        label: `${name || index}`,
        value: `${id}`,
      })),
    [dataBlocks],
  );

  const selectedDataBlock = useMemo<ReleaseDataBlock | undefined>(() => {
    return dataBlocks.find(({ id }) => dataBlockId === id);
  }, [dataBlockId, dataBlocks]);

  const handleDataBlockSave = useCallback(
    async (dataBlock: ReleaseDataBlock) => {
      const currentBlockIndex = dataBlocks.findIndex(
        db => db.id === dataBlock.id,
      );

      const nextDataBlocks = [...dataBlocks];

      if (currentBlockIndex > -1) {
        nextDataBlocks[currentBlockIndex] = dataBlock;
      } else {
        nextDataBlocks.push(dataBlock);

        history.push(
          generatePath<ReleaseDataBlocksRouteParams>(
            releaseDataBlocksRoute.path,
            {
              publicationId,
              releaseId,
              dataBlockId: dataBlock.id,
            },
          ),
        );
      }

      setDataBlocks({
        isLoading: false,
        value: nextDataBlocks,
      });

      if (pageRef.current) {
        pageRef.current.scrollIntoView({
          behavior: 'smooth',
          block: 'start',
        });
      }
    },
    [dataBlocks, setDataBlocks, history, publicationId, releaseId],
  );

  const handleDataBlockDelete = useCallback(async () => {
    if (!selectedDataBlock) {
      return;
    }

    setDeletePlan(undefined);

    await dataBlocksService.deleteDataBlock(releaseId, selectedDataBlock.id);
    await fetchDataBlocks();

    history.push(
      generatePath<ReleaseDataBlocksRouteParams>(releaseDataBlocksRoute.path, {
        publicationId,
        releaseId,
      }),
    );
  }, [fetchDataBlocks, history, publicationId, releaseId, selectedDataBlock]);

  return (
    <>
      <h2>Data blocks</h2>

      <InsetText>
        <h3>Before you start</h3>
        <p>
          A data block is a smaller cut of data from your original file that you
          can embed into your publication as a presentation table, build charts
          from, or link users directly to.
        </p>
      </InsetText>

      <div ref={pageRef}>
        {dataBlockOptions.length > 0 && (
          <>
            <FormSelect
              id="selectedDataBlock"
              name="selectedDataBlock"
              label="Select an existing data block to edit or create a new one"
              disabled={isLoading}
              order={[]}
              value={dataBlockId}
              optGroups={{
                'Create data block': [
                  {
                    label: 'Create new data block',
                    value: '',
                  },
                ],
                'Edit existing': dataBlockOptions,
              }}
              onChange={e => {
                history.push(
                  generatePath<ReleaseDataBlocksRouteParams>(
                    releaseDataBlocksRoute.path,
                    {
                      publicationId,
                      releaseId,
                      dataBlockId: e.target.value ? e.target.value : undefined,
                    },
                  ),
                );
              }}
            />
            <hr />
          </>
        )}

        <LoadingSpinner loading={isLoading}>
          {selectedDataBlock?.name ? (
            <h2>{selectedDataBlock.name}</h2>
          ) : (
            <h2>Create new data block</h2>
          )}

          {selectedDataBlock && (
            <>
              {config && (
                <p className="govuk-!-margin-bottom-6">
                  <strong>Fast track URL:</strong>

                  <UrlContainer
                    className="govuk-!-margin-left-4"
                    url={`${config.PublicAppUrl}/data-tables/fast-track/${selectedDataBlock.id}`}
                  />
                </p>
              )}

              <Button
                type="button"
                variant="warning"
                onClick={() => {
                  dataBlocksService
                    .getDeleteBlockPlan(releaseId, selectedDataBlock.id)
                    .then(setDeletePlan);
                }}
              >
                Delete this data block
              </Button>

              {deletePlan && (
                <ModalConfirm
                  title="Delete data block"
                  mounted
                  onConfirm={handleDataBlockDelete}
                  onExit={() => setDeletePlan(undefined)}
                  onCancel={() => setDeletePlan(undefined)}
                >
                  <p>Are you sure you wish to delete this data block?</p>
                  <ul>
                    {deletePlan.dependentDataBlocks.map(block => (
                      <li key={block.name}>
                        <p>{block.name}</p>
                        {block.contentSectionHeading && (
                          <p>
                            {`It will be removed from the "${block.contentSectionHeading}" content section.`}
                          </p>
                        )}
                        {block.infographicFilesInfo.length > 0 && (
                          <p>
                            The following infographic files will also be
                            removed:
                            <ul>
                              {block.infographicFilesInfo.map(finfo => (
                                <li key={finfo.filename}>
                                  <p>{finfo.filename}</p>
                                </li>
                              ))}
                            </ul>
                          </p>
                        )}
                      </li>
                    ))}
                  </ul>
                </ModalConfirm>
              )}
            </>
          )}

          <ReleaseDataBlocksPageTabs
            key={selectedDataBlock?.id}
            releaseId={releaseId}
            selectedDataBlock={selectedDataBlock}
            onDataBlockSave={handleDataBlockSave}
          />
        </LoadingSpinner>
      </div>
    </>
  );
};

const ReleaseDataBlocksPage = (
  props: RouteComponentProps<ReleaseDataBlocksRouteParams>,
) => {
  const {
    match: {
      params: { releaseId },
    },
  } = props;

  return (
    <Gate
      condition={() => permissionService.canUpdateRelease(releaseId)}
      fallback={<p>This release is currently not editable.</p>}
    >
      <ReleaseDataBlocksPageInternal {...props} />
    </Gate>
  );
};

export default ReleaseDataBlocksPage;
