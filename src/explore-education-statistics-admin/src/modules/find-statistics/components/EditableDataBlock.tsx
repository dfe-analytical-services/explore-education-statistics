import React from "react";
import {DataBlockProps} from "@common/modules/find-statistics/components/DataBlock";


type Props = DataBlockProps;

const EditableDataBlock = ({
  id
}: Props) => {

  return (
    <div>
      Select a data block to use
    </div>
  );

};

export default EditableDataBlock;