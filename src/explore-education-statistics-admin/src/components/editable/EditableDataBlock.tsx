import useGetChartFile from '@admin/hooks/useGetChartFile';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import DataBlockRenderer, {
  DataBlockRendererProps,
} from '@common/modules/find-statistics/components/DataBlockRenderer';
import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import React, { useState } from 'react';
import styles from './EditableDataBlock.module.scss';

type Props = {
  canDelete?: boolean;
  onDelete?: () => void;
  editable?: boolean;
  releaseId: string;
} & OmitStrict<DataBlockRendererProps, 'getInfographic'>;

const EditableDataBlock = ({
  id,
  onDelete,
  editable,
  releaseId,
  ...props
}: Props) => {
  const [showConfirmation, setShowConfirmation] = useState(false);

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
