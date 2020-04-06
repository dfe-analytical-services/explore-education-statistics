import styles from '@admin/components/editable/EditableBlockWrapper.module.scss';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode } from 'react';

export interface EditableBlockProps {
  onEdit?: () => void;
  onDelete?: () => void;
}

const EditableBlockWrapper = ({
  children,
  onEdit,
  onDelete,
}: EditableBlockProps & { children: ReactNode }) => {
  const [showConfirmDelete, toggleConfirmDelete] = useToggle(false);

  return (
    <article>
      <div className={styles.block}>{children}</div>

      <div className={styles.buttons}>
        {onEdit && (
          <Button variant="secondary" onClick={() => onEdit()}>
            Edit block
          </Button>
        )}

        {onDelete && (
          <>
            <Button variant="warning" onClick={toggleConfirmDelete.on}>
              Remove block
            </Button>

            <ModalConfirm
              title="Delete section"
              mounted={showConfirmDelete}
              onConfirm={async () => {
                await onDelete();
                toggleConfirmDelete.off();
              }}
              onExit={toggleConfirmDelete.off}
              onCancel={toggleConfirmDelete.off}
            >
              <p>Are you sure you want to delete this block?</p>
            </ModalConfirm>
          </>
        )}
      </div>
    </article>
  );
};

export default EditableBlockWrapper;
