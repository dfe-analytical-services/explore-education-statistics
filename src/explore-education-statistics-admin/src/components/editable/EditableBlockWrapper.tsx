import ButtonLink from '@admin/components/ButtonLink';
import styles from '@admin/components/editable/EditableBlockWrapper.module.scss';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode } from 'react';

export interface EditableBlockProps {
  dataBlockEditLink?: string;
  onEdit?: () => void;
  onDelete?: () => void;
}

const EditableBlockWrapper = ({
  children,
  dataBlockEditLink,
  onEdit,
  onDelete,
}: EditableBlockProps & { children: ReactNode }) => {
  const [showConfirmDelete, toggleConfirmDelete] = useToggle(false);

  return (
    <article>
      <div className={styles.block}>{children}</div>

      <ButtonGroup className={styles.buttons}>
        {onEdit && (
          <Button variant="secondary" onClick={() => onEdit()}>
            Edit block
          </Button>
        )}

        {dataBlockEditLink && (
          <ButtonLink to={dataBlockEditLink} variant="secondary">
            Edit data block
          </ButtonLink>
        )}

        {onDelete && (
          <>
            <Button variant="warning" onClick={toggleConfirmDelete.on}>
              Remove block
            </Button>

            <ModalConfirm
              title="Remove block"
              open={showConfirmDelete}
              onConfirm={async () => {
                await onDelete();
                toggleConfirmDelete.off();
              }}
              onExit={toggleConfirmDelete.off}
              onCancel={toggleConfirmDelete.off}
            >
              <p>
                Are you sure you want to remove this block from this content
                section?
              </p>
            </ModalConfirm>
          </>
        )}
      </ButtonGroup>
    </article>
  );
};

export default EditableBlockWrapper;
