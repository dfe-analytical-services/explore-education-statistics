import ButtonLink from '@admin/components/ButtonLink';
import styles from '@admin/components/editable/EditableBlockWrapper.module.scss';
import { UserDetails } from '@admin/services/types/user';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tooltip from '@common/components/Tooltip';
import React, { ReactNode } from 'react';

export interface EditableBlockProps {
  dataBlockEditLink?: string;
  editButtonLabel?: ReactNode | string;
  isLoading?: boolean;
  lockedBy?: UserDetails;
  removeButtonLabel?: ReactNode | string;
  onEdit?: () => void;
  onDelete?: () => void;
  onEmbedBlockEdit?: () => void;
}

const EditableBlockWrapper = ({
  children,
  dataBlockEditLink,
  editButtonLabel,
  isLoading = false,
  lockedBy,
  removeButtonLabel = 'Remove block',
  onEdit,
  onDelete,
  onEmbedBlockEdit,
}: EditableBlockProps & { children: ReactNode }) => {
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
                testId="edit-block"
                variant="secondary"
                onClick={() => onEdit()}
              >
                {editButtonLabel ?? 'Edit block'}
              </Button>
            )}
          </Tooltip>
        )}

        {dataBlockEditLink && (
          <ButtonLink
            test-id="edit-data-block"
            to={dataBlockEditLink}
            variant="secondary"
          >
            {editButtonLabel ?? 'Edit data block'}
          </ButtonLink>
        )}

        {onEmbedBlockEdit && (
          <Button test-id="edit-embed-block" onClick={onEmbedBlockEdit}>
            {editButtonLabel ?? 'Edit embedded URL'}
          </Button>
        )}

        {onDelete && (
          <Tooltip text={lockedTooltip} enabled={!!lockedTooltip}>
            {({ ref }) => (
              <ModalConfirm
                title="Remove block"
                triggerButton={
                  <Button
                    ariaDisabled={!!lockedTooltip}
                    disabled={isLoading}
                    ref={ref}
                    testId="remove-block"
                    variant="warning"
                  >
                    {removeButtonLabel}
                  </Button>
                }
                onConfirm={onDelete}
              >
                <p>
                  Are you sure you want to remove this block from this content
                  section?
                </p>
              </ModalConfirm>
            )}
          </Tooltip>
        )}
      </ButtonGroup>
    </>
  );
};

export default EditableBlockWrapper;
