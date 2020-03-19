import { useReleaseState } from '@admin/pages/release/edit-release/content/ReleaseContext';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import {
  locationLevelKeys,
  TimePeriodQuery,
} from '@common/modules/table-tool/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';

interface Props {
  onSelect: (selectedDataBlockId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const KeyStatSelectForm = ({
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a key statistic',
}: Props) => {
  const { availableDataBlocks } = useReleaseState();
  const [selectedDataBlockId, setSelectedDataBlockId] = useState('');

  const keyStatDatablocks = useMemo(() => {
    return availableDataBlocks.filter(db => {
      const req = db.dataBlockRequest;
      const timePeriod = req.timePeriod as TimePeriodQuery;
      if (
        Object.keys(req).filter(key => locationLevelKeys.includes(key as any))
          .length !== 1
      ) {
        console.log(
          'WARN: Request should contain single location from locationLevelKeys!',
        );
      }
      return (
        req.indicators.length !== 1 ||
        timePeriod.startYear !== timePeriod.endYear ||
        locationLevelKeys.some(key => req[key]?.length !== 1)
        // NOTE(mark): No check for number of filters because they cannot tell us whether
        // there is a single result
      );
    });
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
        order={[]}
        options={[
          {
            label: 'Select a data block',
            value: '',
          },
          ...orderBy(
            keyStatDatablocks.map(dataBlock => ({
              label: dataBlock.name || '',
              value: dataBlock.id || '',
            })),
            'label',
          ),
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

export default KeyStatSelectForm;
