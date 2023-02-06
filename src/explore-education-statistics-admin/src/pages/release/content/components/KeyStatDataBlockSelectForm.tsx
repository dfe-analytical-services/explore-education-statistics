import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';

interface Props {
  releaseId: string;
  onSelect: (selectedDataBlockId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const KeyStatDataBlockSelectForm = ({
  releaseId,
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a key statistic',
}: Props) => {
  const { unattachedDataBlocks } = useReleaseContentState();
  const [selectedDataBlockId, setSelectedDataBlockId] = useState('');

  const keyStatDataBlocks = useMemo(() => {
    return unattachedDataBlocks.filter(dataBlock => {
      const { indicators, locationIds, timePeriod } = dataBlock.query;

      return (
        indicators.length === 1 &&
        locationIds.length === 1 &&
        timePeriod?.startYear === timePeriod?.endYear
        // NOTE(mark): No check for number of filters because they cannot tell us whether
        // there is a single result
      );
    });
  }, [unattachedDataBlocks]);

  function getKeyStatPreview() {
    const selectedDataBlock = unattachedDataBlocks.find(
      dataBlock => dataBlock.id === selectedDataBlockId,
    );
    return selectedDataBlock ? (
      <section>
        <Details
          className="govuk-!-margin-top-3"
          summary="Key statistic preview"
          open
        >
          <KeyStatDataBlock
            releaseId={releaseId}
            dataBlockId={selectedDataBlock.id}
          />
        </Details>
      </section>
    ) : null;
  }

  return (
    <>
      <FormSelect
        className="govuk-!-margin-right-1"
        id="keyIndicatorSelect"
        name="selectedDataBlock"
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
            keyStatDataBlocks.map(dataBlock => ({
              label: dataBlock.name || '',
              value: dataBlock.id || '',
            })),
            'label',
          ),
        ]}
      />
      {getKeyStatPreview()}

      <ButtonGroup>
        {selectedDataBlockId !== '' && (
          <Button onClick={() => onSelect(selectedDataBlockId)}>Embed</Button>
        )}
        {!hideCancel && (
          <Button className="govuk-button--secondary" onClick={onCancel}>
            Cancel
          </Button>
        )}
      </ButtonGroup>
    </>
  );
};

export default KeyStatDataBlockSelectForm;
