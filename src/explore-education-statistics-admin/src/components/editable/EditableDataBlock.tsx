import EditableProps from '@admin/components/editable/types/EditableProps';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import DataBlockRenderer, {
  DataBlockRendererProps,
} from '@common/modules/find-statistics/components/DataBlockRenderer';
import classNames from 'classnames';
import React from 'react';
import styles from './EditableDataBlock.module.scss';

export type EditableDataBlockProps = EditableProps & DataBlockRendererProps;

const EditableDataBlock = ({
  id,
  onDelete,
  editable,
  ...props
}: EditableDataBlockProps) => {
  const [showConfirmation, toggleConfirmation] = useToggle(false);

  return (
    <div className={styles.wrapper}>
      <DataBlockRenderer {...props} id={id} />

      {editable && (
        <Button
          className={classNames(
            styles.delete,
            'govuk-button--warning govuk-!-margin-bottom-0',
          )}
          onClick={toggleConfirmation.on}
        >
          Remove this data block
        </Button>
      )}

      <ModalConfirm
        onConfirm={() => {
          if (onDelete) {
            onDelete();
          }
          toggleConfirmation.off();
        }}
        onExit={toggleConfirmation.off}
        onCancel={toggleConfirmation.off}
        title="Delete section"
        mounted={showConfirmation}
      >
        <p>Are you sure you want to remove this data block?</p>
      </ModalConfirm>
    </div>
  );
};

export default EditableDataBlock;
