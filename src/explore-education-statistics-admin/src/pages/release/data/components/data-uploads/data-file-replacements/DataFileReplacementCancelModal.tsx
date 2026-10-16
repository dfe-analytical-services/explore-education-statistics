import releaseDataFileService from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';

interface Props {
  dataFileTitle: string;
  releaseVersionId: string;
  replacementDataFileId: string;
  onCancelReplacement?: () => void;
}

/**
 * Cancels an in-progress data replacement by deleting the pending replacement
 * file. The original data file is left untouched.
 */
export default function DataFileReplacementCancelModal({
  dataFileTitle,
  releaseVersionId,
  replacementDataFileId,
  onCancelReplacement,
}: Props) {
  return (
    <ModalConfirm
      title="Cancel data replacement"
      triggerButton={
        <ButtonText variant="secondary">
          Cancel replacement
          <VisuallyHidden>{` for ${dataFileTitle}`}</VisuallyHidden>
        </ButtonText>
      }
      onConfirm={async () => {
        await releaseDataFileService.deleteDataFiles(
          releaseVersionId,
          replacementDataFileId,
        );
        onCancelReplacement?.();
      }}
    >
      <p>
        Are you sure you want to cancel this data replacement? The pending
        replacement data file will be deleted.
      </p>
    </ModalConfirm>
  );
}
