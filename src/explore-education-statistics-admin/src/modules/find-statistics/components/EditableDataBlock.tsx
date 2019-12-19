import React from 'react';
import DataBlock, {
  DataBlockProps,
} from '@common/modules/find-statistics/components/DataBlock';
import Button from '@common/components/Button';

type Props = {
  canDelete?: boolean;
  onDelete?: () => void;
} & DataBlockProps;

const EditableDataBlock = ({ id, onDelete, ...restOfProps }: Props) => {
  return (
    <div>
      <DataBlock id={id} {...restOfProps} />
      <Button onClick={() => onDelete && onDelete()}>Delete</Button>
    </div>
  );
};

export default EditableDataBlock;

// export default wrapEditableComponent(EditableDataBlock, DataBlock);
