import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import DataBlockTabs, {
  DataBlockTabsProps,
} from '@common/modules/find-statistics/components/DataBlockTabs';
import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import React, { useState } from 'react';
import styles from './EditableDataBlock.module.scss';

type Props = {
  canDelete?: boolean;
  onDelete?: () => void;
  editable?: boolean;
} & OmitStrict<DataBlockTabsProps, 'getInfographic'>;

const EditableDataBlock = ({
  id,
  onDelete,
  editable,
  releaseId,
  ...props
}: Props) => {
  const [showConfirmation, setShowConfirmation] = useState(false);

  return (
    <div className={styles.wrapper}>
      <DataBlockTabs
        {...props}
        id={id}
        releaseId={releaseId}
        getInfographic={editReleaseDataService.downloadChartFile}
      />

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
