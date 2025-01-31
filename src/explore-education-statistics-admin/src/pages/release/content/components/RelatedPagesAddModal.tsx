import RelatedPageForm, {
  RelatedPageFormValues,
} from '@admin/pages/release/content/components/RelatedPageForm';
import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

interface Props {
  onSubmit: (link: RelatedPageFormValues) => Promise<void>;
}

export default function RelatedPagesAddModal({ onSubmit }: Props) {
  const [open, toggleOpen] = useToggle(false);

  const handleSubmit = async (values: RelatedPageFormValues) => {
    await onSubmit(values);
    toggleOpen.off();
  };

  return (
    <Modal
      open={open}
      title="Add related page link"
      triggerButton={<Button onClick={toggleOpen.on}>Add related page</Button>}
      onExit={toggleOpen.off}
    >
      <RelatedPageForm onCancel={toggleOpen.off} onSubmit={handleSubmit} />
    </Modal>
  );
}
