import dataBlocksService from '@admin/services/dataBlockService';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';

interface Props {
  releaseId: string;
  dataBlockId: string;
  onConfirm: () => void;
  onCancel: () => void;
  onExit: () => void;
}

const DataBlockDeletePlanModal = ({
  releaseId,
  dataBlockId,
  onConfirm,
  onCancel,
  onExit,
}: Props) => {
  const { value: deletePlan } = useAsyncRetry(
    () => dataBlocksService.getDeleteBlockPlan(releaseId, dataBlockId),
    [releaseId, dataBlockId],
  );

  if (!deletePlan) {
    return null;
  }

  return (
    <ModalConfirm
      title="Delete data block"
      open
      onConfirm={async () => {
        await dataBlocksService.deleteDataBlock(releaseId, dataBlockId);
        onConfirm();
      }}
      onExit={onExit}
      onCancel={onCancel}
    >
      <p>Are you sure you wish to delete this data block?</p>

      <ul>
        {deletePlan.dependentDataBlocks.map(block => (
          <li key={block.name}>
            <p>
              <strong data-testid="deleteDataBlock-name">{block.name}</strong>
            </p>

            {block.contentSectionHeading && (
              <p>
                It will be removed from the{' '}
                <strong data-testid="deleteDataBlock-contentSectionHeading">
                  {block.contentSectionHeading}
                </strong>{' '}
                content section.
              </p>
            )}

            {block.infographicFilesInfo.length > 0 && (
              <p>
                The following infographic files will also be removed:
                <ul>
                  {block.infographicFilesInfo.map(info => (
                    <li key={info.filename}>
                      <p>{info.filename}</p>
                    </li>
                  ))}
                </ul>
              </p>
            )}
            {block.isKeyStatistic && (
              <p>
                A key statistic associated with this data block will also be
                removed.
              </p>
            )}
          </li>
        ))}
      </ul>
    </ModalConfirm>
  );
};

export default DataBlockDeletePlanModal;
