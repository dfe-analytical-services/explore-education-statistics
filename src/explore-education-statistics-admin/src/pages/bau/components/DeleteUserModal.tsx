import ModalConfirm from '@common/components/ModalConfirm';
import React, { ReactNode } from 'react';

interface Props {
  triggerButton: ReactNode;
  onConfirm: () => void;
}

const DeleteUserModal = ({ triggerButton, onConfirm }: Props) => {
  return (
    <ModalConfirm
      title="Confirm you want to delete this user"
      triggerButton={triggerButton}
      onConfirm={onConfirm}
    >
      <p>
        By deleting this User you will remove all access and permissions they
        have on the service. This change cannot be reversed. Users who are
        removed and then need access at a later point will need to be re-invited
        to the service as a new user.
      </p>
    </ModalConfirm>
  );
};

export default DeleteUserModal;
