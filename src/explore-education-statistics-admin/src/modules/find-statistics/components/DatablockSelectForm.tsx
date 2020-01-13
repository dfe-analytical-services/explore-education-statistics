import React, { useContext, useState } from 'react';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';

interface Props {
  onSelect: (selectedDataBlockId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const DatablockSelectForm = ({
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a data block',
}: Props) => {
  const { availableDataBlocks } = useContext(EditingContext);
  const [selectedDataBlockId, setSelectedDataBlockId] = useState('');

  return (
    <>
      <FormSelect
        className="govuk-!-margin-right-1"
        id="id"
        name="datablock_select"
        label={label}
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
      {selectedDataBlockId && <div>show db preview</div>}
      {selectedDataBlockId !== '' && (
        <Button onClick={() => onSelect(selectedDataBlockId)}>Embed</Button>
      )}
      {!hideCancel && (
        <Button
          className="govuk-button--secondary"
          onClick={onCancel /*() => setShowDataBlocks(false)*/}
        >
          Cancel
        </Button>
      )}
    </>
  );
};

export default DatablockSelectForm;
