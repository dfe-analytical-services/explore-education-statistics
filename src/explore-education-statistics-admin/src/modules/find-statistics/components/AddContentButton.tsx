import React from 'react';
import Button from '@common/components/Button';
import { DataBlock } from '@common/services/dataBlockService';

interface AddContentButtonProps {
  order?: number;
  onClick: (type: string, order: number | undefined) => void;
  availableDataBlocks: DataBlock[];
}

const AddContentButton = ({ order, onClick }: AddContentButtonProps) => {
  return (
    <>
      <Button
        className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
        onClick={() => onClick('HtmlBlock', order)}
      >
        Add HTML
      </Button>
    </>
  );
};

export default AddContentButton;
