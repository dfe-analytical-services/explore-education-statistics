import EditableProps from '@admin/components/editable/types/EditableProps';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import DataBlockRenderer, {
  DataBlockRendererProps,
} from '@common/modules/find-statistics/components/DataBlockRenderer';
import classNames from 'classnames';
import React from 'react';
import styles from './EditableDataBlock.module.scss';

export interface EditableDataBlockProps
  extends EditableProps,
    Omit<DataBlockRendererProps, 'getInfographic'> {
  releaseId: string;
}

const EditableDataBlock = ({
  id,
  onDelete,
  editable,
  releaseId,
  ...props
}: EditableDataBlockProps) => {
  const [showConfirmation, toggleConfirmation] = useToggle(false);

  const getChartFile = useGetChartFile(releaseId);

  return (
    <div className={styles.wrapper}>
      <DataBlockRenderer {...props} id={id} getInfographic={getChartFile} />

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
