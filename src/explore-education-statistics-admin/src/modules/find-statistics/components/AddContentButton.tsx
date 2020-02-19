import React from 'react';
import Button from '@common/components/Button';
import { DataBlock } from '@common/services/dataBlockService';
import DatablockSelectForm from './DatablockSelectForm';

interface AddContentButtonProps {
  onClick: (type: string, data: string) => void;
  availableDataBlocks: DataBlock[];
  textOnly?: boolean;
  buttonText?: string;
  datablockOnly?: boolean;
}

const AddContentButton = ({
  onClick,
  textOnly = false,
  buttonText = 'Add content',
  datablockOnly = false,
}: AddContentButtonProps) => {
  const [showDataBlocks, setShowDataBlocks] = React.useState(false);

  return (
    <>
      <div className="govuk-!-margin-top-9 govuk-!-margin-bottom-9 dfe-align--centre">
        {!datablockOnly && (
          <Button
            variant="secondary"
            onClick={() => onClick('MarkdownBlock', 'Click to edit')}
          >
            {buttonText}
          </Button>
        )}
        {!textOnly && (
          <Button variant="secondary" onClick={() => setShowDataBlocks(true)}>
            Add DataBlock
          </Button>
        )}
      </div>

      {showDataBlocks && (
        <DatablockSelectForm
          onSelect={selectedDataBlockId => {
            onClick('DataBlock', selectedDataBlockId);
            setShowDataBlocks(false);
          }}
          onCancel={() => {
            setShowDataBlocks(false);
          }}
        />
      )}
    </>
  );
};

export default AddContentButton;
