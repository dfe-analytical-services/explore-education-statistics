import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import DataBlock, {
  DataBlockProps,
} from '@common/modules/find-statistics/components/DataBlock';
import classNames from 'classnames';
import React, { useState } from 'react';
import styles from './EditableDataBlock.module.scss';

type Props = {
  canDelete?: boolean;
  onDelete?: () => void;
  editable?: boolean;
} & DataBlockProps;

const EditableDataBlock = ({
  id,
  onDelete,
  editable,
  ...restOfProps
}: Props) => {
  const { releaseId } = useManageReleaseContext();
  const [showConfirmation, setShowConfirmation] = useState(false);

  return (
    <div className={styles.wrapper}>
      <DataBlock id={id} {...restOfProps} releaseId={releaseId} />
      {editable && (
        <Button
          className={classNames(
            styles.delete,
            'govuk-button--warning govuk-!-margin-bottom-0',
          )}
          onClick={() => setShowConfirmation(true)}
        >
          Remove this data block
        </Button>
      )}

      <ModalConfirm
        onConfirm={() => {
          if (onDelete) onDelete();
          setShowConfirmation(false);
        }}
        onExit={() => {
          setShowConfirmation(false);
        }}
        onCancel={() => {
          setShowConfirmation(false);
        }}
        title="Delete section"
        mounted={showConfirmation}
      >
        <p>Are you sure you want to remove this data block?</p>
      </ModalConfirm>
    </div>
  );
};

export default EditableDataBlock;
