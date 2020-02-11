import React, { useContext, useState, useEffect } from 'react';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import Details from '@common/components/Details';
import { TimePeriodQuery } from '@common/modules/table-tool/services/tableBuilderService';
import { DataBlock } from '@common/services/dataBlockService';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';

interface Props {
  onSelect: (selectedDataBlockId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const KeyIndicatorSelectForm = ({
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a key indicator',
}: Props) => {
  const { availableDataBlocks } = useContext(EditingContext);
  const [selectedDataBlockId, setSelectedDataBlockId] = useState('');

  const [keyIndicatorDatablocks, setKeyIndicatorDatablocks] = useState<
    DataBlock[]
  >([]);

  useEffect(() => {
    setKeyIndicatorDatablocks(
      availableDataBlocks.filter(db => {
        return (
          Object.entries(db.dataBlockRequest).filter(([key, val]) => {
            // only want datablocks with 1 of each filter type (filter, time period, location and indicator)
            if (key === 'timePeriod') {
              return !(
                (val as TimePeriodQuery).startYear ===
                (val as TimePeriodQuery).endYear
              );
            }
            return Array.isArray(val) ? val.length !== 1 : false;
          }).length === 0
        );
      }),
    );
  }, [availableDataBlocks]);

  function getKeyStatPreview() {
    const selectedDataBlock = availableDataBlocks.find(
      datablock => datablock.id === selectedDataBlockId,
    );
    return selectedDataBlock ? (
      <section>
        <Details
          className="govuk-!-margin-top-3"
          summary="Key statistic preview"
          open
          onToggle={() => {}}
        >
          <KeyStatTile {...selectedDataBlock} />
        </Details>
      </section>
    ) : null;
  }

  return (
    <>
      <FormSelect
        className="govuk-!-margin-right-1"
        id="id"
        name="key_indicator_select"
        label={label}
        value={selectedDataBlockId}
        onChange={e => setSelectedDataBlockId(e.target.value)}
        options={[
          {
            label: 'Select a data block',
            value: '',
          },
          ...keyIndicatorDatablocks.map(dataBlock => ({
            label: dataBlock.name || '',
            value: dataBlock.id || '',
          })),
        ]}
      />
      {getKeyStatPreview()}
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

export default KeyIndicatorSelectForm;
