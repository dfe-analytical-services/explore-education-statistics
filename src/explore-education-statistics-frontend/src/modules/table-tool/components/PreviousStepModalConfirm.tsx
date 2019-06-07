import ModalConfirm from '@common/components/ModalConfirm';
import { useConfirmContext } from '@common/context/ConfirmContext';
import React from 'react';

const PreviousStepModalConfirm = () => {
  const { isConfirming, confirm, cancel } = useConfirmContext();

  return (
    <ModalConfirm
      title="Go back to previous step"
      onExit={cancel}
      onConfirm={confirm}
      onCancel={cancel}
      mounted={isConfirming}
    >
      <p>You will lose any changes you have made in the current step.</p>
    </ModalConfirm>
  );
};

export default PreviousStepModalConfirm;
