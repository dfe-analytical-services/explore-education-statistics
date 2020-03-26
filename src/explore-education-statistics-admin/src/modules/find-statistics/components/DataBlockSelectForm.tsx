import { useReleaseState } from '@admin/pages/release/edit-release/content/ReleaseContext';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import React, { useState } from 'react';

interface Props {
  onSelect: (selectedDataBlockId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const DataBlockSelectForm = ({
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a data block',
}: Props) => {
  const { availableDataBlocks, release } = useReleaseState();
  const [selectedDataBlockId, setSelectedDataBlockId] = useState('');

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
            dataBlock={selectedDataBlock}
            id={`dataBlockSelectForm-${
              selectedDataBlock ? `${selectedDataBlock.id}-tabs` : 'tabs'
            }`}
            releaseId={release.id}
            getInfographic={editReleaseDataService.downloadChartFile}
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
        <>
          <Button onClick={() => onSelect(selectedDataBlockId)}>Embed</Button>
          {!hideCancel && (
            <Button className="govuk-button--secondary" onClick={onCancel}>
              Cancel
            </Button>
          )}
        </>
      )}
    </div>
  );
};

export default DataBlockSelectForm;
