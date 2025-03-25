import dataBlocksService from '@admin/services/dataBlockService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

interface Props {
  releaseVersionId: string;
  dataBlockId: string;
  // The edit dataBlock page has a button, but other instances are
  // within a table and appear as button text
  triggerButtonVariant?: 'TEXT' | 'BUTTON';
  onConfirm: () => void;
}

const DataBlockDeletePlanModal = ({
  releaseVersionId,
  dataBlockId,
  triggerButtonVariant = 'TEXT',
  onConfirm,
}: Props) => {
  const [open, toggleOpen] = useToggle(false);

  const { value: deletePlan } = useAsyncRetry(
    () => dataBlocksService.getDeleteBlockPlan(releaseVersionId, dataBlockId),
    [releaseVersionId, dataBlockId],
  );

  if (!deletePlan) {
    return null;
  }

  return (
    <ModalConfirm
      title="Delete data block"
      open={open}
      triggerButton={
        triggerButtonVariant === 'TEXT' ? (
          <ButtonText
            className="govuk-!-margin-bottom-0"
            onClick={toggleOpen.on}
          >
            Delete block
          </ButtonText>
        ) : (
          <Button variant="warning" onClick={toggleOpen.on}>
            Delete this data block
          </Button>
        )
      }
      onConfirm={async () => {
        await dataBlocksService.deleteDataBlock(releaseVersionId, dataBlockId);
        onConfirm();
        toggleOpen.off();
      }}
      onExit={toggleOpen.off}
      onCancel={toggleOpen.off}
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
