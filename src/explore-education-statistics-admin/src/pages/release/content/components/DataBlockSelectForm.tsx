import useGetChartFile from '@admin/hooks/useGetChartFile';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import React, { useState } from 'react';

interface Props {
  releaseId: string;
  onSelect: (selectedDataBlockId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const DataBlockSelectForm = ({
  releaseId,
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a data block',
}: Props) => {
  const { availableDataBlocks, release } = useReleaseContentState();
  const [selectedDataBlockId, setSelectedDataBlockId] = useState('');

  const getChartFile = useGetChartFile(release.id);

  const getDataBlockPreview = (dataBlockId: string) => {
    const selectedDataBlock = availableDataBlocks.find(
      dataBlock => dataBlock.id === dataBlockId,
    );
    return selectedDataBlock ? (
      <section>
        <Details
          className="govuk-!-margin-top-3"
          summary="Data block preview"
          open
          onToggle={() => {}}
        >
          <DataBlockTabs
            releaseId={releaseId}
            dataBlock={selectedDataBlock}
            id={`dataBlockSelectForm-${
              selectedDataBlock ? `${selectedDataBlock.id}-tabs` : 'tabs'
            }`}
            getInfographic={getChartFile}
          />
        </Details>
      </section>
    ) : null;
  };

  return (
    <div className="dfe-align--left">
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

      <Button className="govuk-button--secondary" onClick={onCancel}>
        Cancel
      </Button>

      {getDataBlockPreview(selectedDataBlockId)}

      {selectedDataBlockId !== '' && (
        <ButtonGroup>
          <Button onClick={() => onSelect(selectedDataBlockId)}>Embed</Button>
          {!hideCancel && (
            <Button className="govuk-button--secondary" onClick={onCancel}>
              Cancel
            </Button>
          )}
        </ButtonGroup>
      )}
    </div>
  );
};

export default DataBlockSelectForm;
