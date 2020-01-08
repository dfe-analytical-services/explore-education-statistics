import React from 'react';
import Button from '@common/components/Button';
import { DataBlock } from '@common/services/dataBlockService';
import FormSelect from '@common/components/form/FormSelect';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';

interface AddContentButtonProps {
  onClick: (type: string, data: string) => void;
  availableDataBlocks: DataBlock[];
  textOnly?: boolean;
}

const AddContentButton = ({
  onClick,
  textOnly = false,
}: AddContentButtonProps) => {
  const editingContext = React.useContext(EditingContext);

  const { availableDataBlocks } = editingContext;
  const [selectedDataBlockId, setSelectedDataBlockId] = React.useState();
  const [showDataBlocks, setShowDataBlocks] = React.useState(false);

  return (
    <>
      <Button
        className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
        onClick={() => onClick('HtmlBlock', 'Click to edit')}
      >
        Add HTML
      </Button>
      {!textOnly && (
        <Button
          className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
          onClick={() => setShowDataBlocks(true)}
        >
          Add DataBlock
        </Button>
      )}

      {showDataBlocks && (
        <>
          <FormSelect
            id="id"
            name="datablock_select"
            label="Select a data block"
            value={selectedDataBlockId}
            onChange={e => setSelectedDataBlockId(e.target.value)}
            options={[
              {
                label: 'Select a data block',
                value: '',
              },
              ...availableDataBlocks.map(dataBlock => ({
                label: dataBlock.name || '',
                value: dataBlock.id || '',
              })),
            ]}
          />
          <Button
            onClick={() => {
              onClick('DataBlock', selectedDataBlockId);
              setShowDataBlocks(false);
            }}
          >
            Add
          </Button>
          <Button
            className="govuk-button--secondary"
            onClick={() => setShowDataBlocks(false)}
          >
            Cancel
          </Button>
        </>
      )}
    </>
  );
};

export default AddContentButton;
