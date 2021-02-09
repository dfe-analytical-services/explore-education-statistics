import ModalConfirm from '@common/components/ModalConfirm';
import { useConfirmContext } from '@common/contexts/ConfirmContext';
import React from 'react';

const PreviousStepModalConfirm = () => {
  const { isConfirming, confirm, cancel } = useConfirmContext();

  return (
    <ModalConfirm
      title="Go back to previous step"
      onExit={cancel}
      onConfirm={confirm}
      onCancel={cancel}
      open={isConfirming}
    >
      <p>You'll need to reselect some of the options you've already chosen.</p>
    </ModalConfirm>
  );
};

export default PreviousStepModalConfirm;
