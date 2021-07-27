import releaseDataFileService from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import ErrorMessage from '@common/components/ErrorMessage';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

interface Props {
  releaseId: string;
  fileId: string;
}

const DataUploadCancelButton = ({ releaseId, fileId }: Props) => {
  const [{ error }, cancelImport] = useAsyncCallback(() =>
    releaseDataFileService.cancelImport(releaseId, fileId),
  );
  const [showCancelModal, toggleShowCancelModal] = useToggle(false);
  const [showCancelButton, toggleShowCancelButton] = useToggle(true);

  return (
    <>
      {showCancelButton && (
        <ButtonText onClick={toggleShowCancelModal.on}>Cancel</ButtonText>
      )}
      {error && <ErrorMessage>Cancellation failed</ErrorMessage>}
      <ModalConfirm
        open={showCancelModal}
        title="Confirm cancellation of selected data file"
        onExit={toggleShowCancelModal.off}
        onCancel={toggleShowCancelModal.off}
        onConfirm={async () => {
          await cancelImport();
          toggleShowCancelModal.off();
          toggleShowCancelButton.off();
        }}
      >
        <p>This file upload will be cancelled and may then be removed.</p>
      </ModalConfirm>
    </>
  );
};

export default DataUploadCancelButton;
