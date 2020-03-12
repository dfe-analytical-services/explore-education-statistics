import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import DataBlock, {
  DataBlockProps,
} from '@common/modules/find-statistics/components/DataBlock';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useContext, useState } from 'react';

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

  const getDBPreview = (datablockId: string) => {
    const selectedDataBlock = availableDataBlocks.find(
      datablock => datablock.id === datablockId,
    );
    return selectedDataBlock ? (
      <section>
        <Details
          className="govuk-!-margin-top-3"
          summary="Data block preview"
          open
          onToggle={() => {}}
        >
          <DataBlock {...(selectedDataBlock as DataBlockProps)} />
        </Details>
      </section>
    ) : null;
  };

  return (
    <>
      <FormSelect
        className="govuk-!-margin-right-1"
        id="id"
        name="datablock_select"
        label={label}
        value={selectedDataBlockId}
        onChange={e => setSelectedDataBlockId(e.target.value)}
        order={['style']}
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
      {getDBPreview(selectedDataBlockId)}
      {selectedDataBlockId !== '' && (
        <Button onClick={() => onSelect(selectedDataBlockId)}>Embed</Button>
      )}
      {!hideCancel && (
        <Button className="govuk-button--secondary" onClick={onCancel}>
          Cancel
        </Button>
      )}
    </>
  );
};

export default DatablockSelectForm;
