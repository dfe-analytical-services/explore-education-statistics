import releaseDataFileService from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncCallback from '@common/hooks/useAsyncCallback';
import useToggle from '@common/hooks/useToggle';
import ErrorMessage from '@common/components/ErrorMessage';
import React from 'react';

interface Props {
  releaseId: string;
  fileId: string;
}

const DataUploadCancelButton = ({ releaseId, fileId }: Props) => {
  const [{ error }, cancelImport] = useAsyncCallback(() =>
    releaseDataFileService.cancelImport(releaseId, fileId),
  );
  const [showCancelButton, toggleShowCancelButton] = useToggle(true);

  return (
    <>
      <ModalConfirm
        title="Confirm cancellation of selected data file"
        triggerButton={showCancelButton && <ButtonText>Cancel</ButtonText>}
        onConfirm={async () => {
          await cancelImport();
          toggleShowCancelButton.off();
        }}
      >
        <p>This file upload will be cancelled and may then be removed.</p>
      </ModalConfirm>
      {error && <ErrorMessage>Cancellation failed</ErrorMessage>}
    </>
  );
};

export default DataUploadCancelButton;
