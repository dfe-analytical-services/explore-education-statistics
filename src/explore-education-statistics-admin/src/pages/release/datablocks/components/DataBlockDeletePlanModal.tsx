import dataBlockQueries from '@admin/queries/dataBlockQueries';
import dataBlocksService from '@admin/services/dataBlockService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback } from 'react';

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

  const { data: deletePlan, isLoading } = useQuery({
    ...dataBlockQueries.getDeleteBlockPlan(releaseVersionId, dataBlockId),
    enabled: open,
  });

  const handleDeleteConfirm = useCallback(async () => {
    try {
      await dataBlocksService.deleteDataBlock(releaseVersionId, dataBlockId);

      onConfirm();

      toggleOpen.off();
    } catch (err) {
      logger.error(err);
      toggleOpen.off();
    }
  }, [releaseVersionId, dataBlockId, onConfirm, toggleOpen]);

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
      onConfirm={handleDeleteConfirm}
      onExit={toggleOpen.off}
      onCancel={toggleOpen.off}
    >
      <LoadingSpinner loading={isLoading}>
        <p>Are you sure you wish to delete this data block?</p>

        <ul>
          {deletePlan?.dependentDataBlocks?.map(block => (
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
      </LoadingSpinner>
    </ModalConfirm>
  );
};

export default DataBlockDeletePlanModal;
