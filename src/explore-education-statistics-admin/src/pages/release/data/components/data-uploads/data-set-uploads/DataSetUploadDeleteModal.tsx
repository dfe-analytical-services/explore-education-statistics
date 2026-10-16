import releaseDataFileService from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import React, { useCallback } from 'react';

interface Props {
  dataSetTitle: string;
  dataSetUploadId: string;
  isReplacement: boolean;
  releaseVersionId: string;
  onDeleteUpload: (deletedUploadId: string) => void;
}

/**
 * Deletes a data set upload that has not been imported yet. When the upload is
 * replacing an existing file this is presented as cancelling the replacement.
 */
export default function DataSetUploadDeleteModal({
  dataSetTitle,
  dataSetUploadId,
  isReplacement,
  releaseVersionId,
  onDeleteUpload,
}: Props) {
  const [open, toggleOpen] = useToggle(false);

  const handleDeleteConfirm = useCallback(async () => {
    try {
      await releaseDataFileService.deleteDataSetUpload(
        releaseVersionId,
        dataSetUploadId,
      );
      onDeleteUpload(dataSetUploadId);
    } catch (err) {
      logger.error(err);
    } finally {
      toggleOpen.off();
    }
  }, [releaseVersionId, dataSetUploadId, toggleOpen, onDeleteUpload]);

  return (
    <ModalConfirm
      open={open}
      title={
        isReplacement
          ? 'Cancel replacement'
          : 'Confirm deletion of selected data files'
      }
      triggerButton={
        <ButtonText onClick={toggleOpen.on} variant="warning">
          {isReplacement ? 'Cancel replacement' : 'Delete files'}
          <VisuallyHidden>{` for ${dataSetTitle}`}</VisuallyHidden>
        </ButtonText>
      }
      onConfirm={handleDeleteConfirm}
    >
      {isReplacement ? (
        <p>
          Are you sure you want to cancel this data replacement? The pending
          replacement data file will be deleted.
        </p>
      ) : (
        <>
          <p>
            Are you sure you want to delete <strong>{dataSetTitle}</strong>?
          </p>
          <p>This version of the data set has not yet been imported.</p>
        </>
      )}
    </ModalConfirm>
  );
}
