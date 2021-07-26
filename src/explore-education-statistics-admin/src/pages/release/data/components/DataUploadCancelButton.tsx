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
  const [showCancelModal, setShowCancelModal] = useToggle(false);
  const [showCancelButton, setShowCancelButton] = useToggle(true);

  return (
    <>
      {showCancelButton && (
        <ButtonText onClick={() => setShowCancelModal(true)}>Cancel</ButtonText>
      )}
      {error && <ErrorMessage>Cancellation failed</ErrorMessage>}
      <ModalConfirm
        open={showCancelModal}
        title="Confirm cancellation of selected data file"
        onExit={() => setShowCancelModal(false)}
        onCancel={() => setShowCancelModal(false)}
        onConfirm={async () => {
          await cancelImport();
          setShowCancelModal(false);
          setShowCancelButton(false);
        }}
      >
        <p>This file upload will be cancelled and may then be removed.</p>
      </ModalConfirm>
    </>
  );
};

export default DataUploadCancelButton;
