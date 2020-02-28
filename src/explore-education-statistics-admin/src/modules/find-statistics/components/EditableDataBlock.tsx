import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import React from 'react';
import DataBlock, {
  DataBlockProps,
} from '@common/modules/find-statistics/components/DataBlock';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import styles from './EditableDataBlock.module.scss';

type Props = {
  canDelete?: boolean;
  onDelete?: () => void;
} & DataBlockProps;

const EditableDataBlock = ({ id, onDelete, ...restOfProps }: Props) => {
  const [showConfirmation, setShowConfirmation] = React.useState(false);

  return (
    <div className={styles.wrapper}>
      <DataBlock
        {...restOfProps}
        id={id}
        getInfographic={editReleaseDataService.downloadChartFile}
      />

      <Button
        className="govuk-button--warning"
        onClick={() => setShowConfirmation(true)}
      >
        Remove this data block
      </Button>

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

// export default wrapEditableComponent(EditableDataBlock, DataBlock);
