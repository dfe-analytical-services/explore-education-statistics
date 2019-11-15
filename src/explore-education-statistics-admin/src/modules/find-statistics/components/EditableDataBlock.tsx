import React from 'react';
import DataBlock, {
  DataBlockProps,
} from '@common/modules/find-statistics/components/DataBlock';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';

type Props = DataBlockProps;

const EditableDataBlock = ({ id }: Props) => {
  return <div id={id}>Select a data block to use</div>;
};

export default wrapEditableComponent(EditableDataBlock, DataBlock);
