import ButtonLink from '@admin/components/ButtonLink';
import styles from '@admin/components/editable/EditableBlockWrapper.module.scss';
import { UserDetails } from '@admin/services/types/user';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tooltip from '@common/components/Tooltip';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode } from 'react';

export interface EditableBlockProps {
  dataBlockEditLink?: string;
  isLoading?: boolean;
  lockedBy?: UserDetails;
  onEdit?: () => void;
  onDelete?: () => void;
  onEmbedBlockEdit?: () => void;
}

const EditableBlockWrapper = ({
  children,
  dataBlockEditLink,
  isLoading = false,
  lockedBy,
  onEdit,
  onDelete,
  onEmbedBlockEdit,
}: EditableBlockProps & { children: ReactNode }) => {
  const [showConfirmDelete, toggleConfirmDelete] = useToggle(false);

  const lockedTooltip = lockedBy
    ? `This block is being edited by ${lockedBy?.displayName}`
    : undefined;

  return (
    <>
      <div>{children}</div>

      <ButtonGroup className={styles.buttons}>
        <LoadingSpinner
          loading={isLoading}
          inline
          hideText
          size="md"
          text="Loading block editor"
        />
        {onEdit && (
          <Tooltip text={lockedTooltip} enabled={!!lockedTooltip}>
            {({ ref }) => (
              <Button
                ariaDisabled={!!lockedTooltip}
                disabled={isLoading}
                ref={ref}
                variant="secondary"
                onClick={() => onEdit()}
              >
                Edit block
              </Button>
            )}
          </Tooltip>
        )}

        {dataBlockEditLink && (
          <ButtonLink to={dataBlockEditLink} variant="secondary">
            Edit data block
          </ButtonLink>
        )}

        {onEmbedBlockEdit && (
          <Button onClick={onEmbedBlockEdit}>Edit embedded URL</Button>
        )}

        {onDelete && (
          <>
            <Tooltip text={lockedTooltip} enabled={!!lockedTooltip}>
              {({ ref }) => (
                <Button
                  ariaDisabled={!!lockedTooltip}
                  disabled={isLoading}
                  ref={ref}
                  variant="warning"
                  onClick={toggleConfirmDelete.on}
                >
                  Remove block
                </Button>
              )}
            </Tooltip>

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
    </>
  );
};

export default EditableBlockWrapper;
