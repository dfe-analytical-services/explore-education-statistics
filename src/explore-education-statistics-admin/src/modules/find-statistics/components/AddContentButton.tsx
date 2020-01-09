import React from 'react';
import Button from '@common/components/Button';
import { DataBlock } from '@common/services/dataBlockService';
import FormSelect from '@common/components/form/FormSelect';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';

interface AddContentButtonProps {
  onClick: (type: string, data: string) => void;
  availableDataBlocks: DataBlock[];
}

const AddContentButton = ({ onClick }: AddContentButtonProps) => {
  const editingContext = React.useContext(EditingContext);

  const { availableDataBlocks } = editingContext;
  const [selectedDataBlockId, setSelectedDataBlockId] = React.useState();
  const [showDataBlocks, setShowDataBlocks] = React.useState(false);

  return (
    <>
      <div className="govuk-!-margin-top-9 govuk-!-margin-bottom-9 dfe-align--centre">
        <Button
          variant="secondary"
          onClick={() => onClick('HtmlBlock', 'Click to edit')}
        >
          Add content
        </Button>
        <Button variant="secondary" onClick={() => setShowDataBlocks(true)}>
          Add DataBlock
        </Button>
      </div>

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
