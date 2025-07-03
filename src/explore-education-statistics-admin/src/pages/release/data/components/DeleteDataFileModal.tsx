import releaseDataFileQueries from '@admin/queries/releaseDataFileQueries';
import releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback } from 'react';

interface Props {
  dataFile: DataFile;
  releaseVersionId: string;
  onConfirm: () => void;
}

export default function DeleteDataFileModal({
  dataFile,
  releaseVersionId,
  onConfirm,
}: Props) {
  const [open, toggleOpen] = useToggle(false);
  const { data: plan, isLoading } = useQuery({
    ...releaseDataFileQueries.getDeleteFilePlan(releaseVersionId, dataFile.id),
    enabled: open,
  });

  const handleDeleteConfirm = useCallback(async () => {
    try {
      await releaseDataFileService.deleteDataFiles(
        releaseVersionId,
        dataFile.id,
      );

      onConfirm();

      toggleOpen.off();
    } catch (err) {
      logger.error(err);
      toggleOpen.off();
    }
  }, [releaseVersionId, dataFile.id, onConfirm, toggleOpen]);

  return (
    <ModalConfirm
      open={open}
      title="Confirm deletion of selected data files"
      triggerButton={
        <ButtonText variant="warning" onClick={toggleOpen.on}>
          Delete files
        </ButtonText>
      }
      onConfirm={handleDeleteConfirm}
    >
      <p>
        Are you sure you want to delete <strong>{dataFile.title}</strong>?
      </p>
      <p>This data will no longer be available for use in this release.</p>

      <LoadingSpinner loading={isLoading}>
        {plan && (
          <>
            {plan.deleteDataBlockPlan.dependentDataBlocks.length > 0 && (
              <>
                <p>The following data blocks will also be deleted:</p>

                <ul>
                  {plan.deleteDataBlockPlan.dependentDataBlocks.map(block => (
                    <li key={block.name}>
                      <p>{block.name}</p>

                      {block.contentSectionHeading && (
                        <p>
                          It will also be removed from the "
                          {block.contentSectionHeading}" content section.
                        </p>
                      )}
                      {block.infographicFilesInfo.length > 0 && (
                        <p>
                          The following infographic files will also be removed:
                          <ul>
                            {block.infographicFilesInfo.map(fileInfo => (
                              <li key={fileInfo.filename}>
                                <p>{fileInfo.filename}</p>
                              </li>
                            ))}
                          </ul>
                        </p>
                      )}
                      {block.isKeyStatistic && (
                        <p>
                          A key statistic associated with this data block will
                          be removed.
                        </p>
                      )}
                      {block.featuredTable && (
                        <p>
                          The featured table "{block.featuredTable.name}" using
                          this data block will be removed.
                        </p>
                      )}
                    </li>
                  ))}
                </ul>
              </>
            )}
            {plan.footnoteIds.length > 0 && (
              <p>
                {`${plan.footnoteIds.length} ${
                  plan.footnoteIds.length > 1 ? 'footnotes' : 'footnote'
                } will be removed or updated.`}
              </p>
            )}
          </>
        )}
      </LoadingSpinner>
    </ModalConfirm>
  );
}
