import ReleaseManageDataBlocksPageTabs from '@admin/pages/release/edit-release/manage-datablocks/components/ReleaseManageDataBlocksPageTabs';
import { manageDataBlocksRoute } from '@admin/routes/edit-release/routes';
import dataBlocksService, {
  ReleaseDataBlock,
} from '@admin/services/release/edit-release/datablocks/service';
import { DeleteDataBlockPlan } from '@admin/services/release/edit-release/datablocks/types';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React, { useCallback, useMemo, useState } from 'react';
import { RouteComponentProps } from 'react-router';

export interface ReleaseManageDataBlocksPageParams {
  publicationId: string;
  releaseId: string;
  dataBlockId?: string;
}

const emptyDataBlocks: ReleaseDataBlock[] = [];

const ReleaseManageDataBlocksPage = ({
  match,
  history,
}: RouteComponentProps<ReleaseManageDataBlocksPageParams>) => {
  const { publicationId, releaseId, dataBlockId } = match.params;

  const [deletePlan, setDeletePlan] = useState<DeleteDataBlockPlan>();

  const {
    value: dataBlocks = emptyDataBlocks,
    isLoading,
    retry: fetchDataBlocks,
    setValue: setDataBlocks,
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
          manageDataBlocksRoute.generateLink({
            publicationId,
            releaseId,
            dataBlockId: dataBlock.id,
          }),
        );
      }

      setDataBlocks(nextDataBlocks);
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
      manageDataBlocksRoute.generateLink({ publicationId, releaseId }),
    );
  }, [fetchDataBlocks, history, publicationId, releaseId, selectedDataBlock]);

  return (
    <>
      {dataBlockOptions.length > 0 && (
        <FormSelect
          id="selectDataBlock"
          name="selectDataBlock"
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
              manageDataBlocksRoute.generateLink({
                publicationId,
                releaseId,
                dataBlockId: e.target.value ? e.target.value : undefined,
              }),
            );
          }}
        />
      )}

      <hr />

      <LoadingSpinner loading={isLoading}>
        <h2>{selectedDataBlock?.name ?? 'Create new data block'}</h2>

        {selectedDataBlock && (
          <>
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
                      {block.infographicFilenames.length > 0 && (
                        <p>
                          The following infographic files will also be removed:
                          <ul>
                            {block.infographicFilenames.map(filename => (
                              <li key={filename}>
                                <p>{filename}</p>
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

        <ReleaseManageDataBlocksPageTabs
          key={selectedDataBlock?.id}
          releaseId={releaseId}
          selectedDataBlock={selectedDataBlock}
          onDataBlockSave={handleDataBlockSave}
        />
      </LoadingSpinner>
    </>
  );
};

export default ReleaseManageDataBlocksPage;
