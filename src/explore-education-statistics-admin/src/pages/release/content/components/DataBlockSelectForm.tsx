import useGetChartFile from '@admin/hooks/useGetChartFile';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import React, { useState } from 'react';

interface Props {
  id: string;
  releaseId: string;
  onSelect: (selectedDataBlockId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const DataBlockSelectForm = ({
  id,
  releaseId,
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a data block',
}: Props) => {
  const { unattachedDataBlocks, release } = useReleaseContentState();
  const [selectedDataBlockId, setSelectedDataBlockId] = useState('');

  const getChartFile = useGetChartFile(release.id);

  const getDataBlockPreview = (dataBlockId: string) => {
    const selectedDataBlock = unattachedDataBlocks.find(
      dataBlock => dataBlock.id === dataBlockId,
    );
    return selectedDataBlock ? (
      <section>
        <Details
          className="govuk-!-margin-top-3"
          summary="Data block preview"
          open
        >
          <DataBlockTabs
            releaseId={releaseId}
            dataBlock={selectedDataBlock}
            id={`${id}-dataBlockPreview`}
            getInfographic={getChartFile}
          />
        </Details>
      </section>
    ) : null;
  };

  return (
    <form className="dfe-align--left" id={id}>
      <FormSelect
        className="govuk-!-margin-right-1"
        id={`${id}-selectedDataBlock`}
        name="selectedDataBlock"
        label={label}
        value={selectedDataBlockId}
        onChange={e => setSelectedDataBlockId(e.target.value)}
        order={['style']}
        options={[
          {
            label: 'Select a data block',
            value: '',
          },
          ...unattachedDataBlocks.map(dataBlock => ({
            label: dataBlock.name || '',
            value: dataBlock.id || '',
          })),
        ]}
      />

      <Button variant="secondary" onClick={onCancel}>
        Cancel
      </Button>

      {getDataBlockPreview(selectedDataBlockId)}

      {selectedDataBlockId !== '' && (
        <ButtonGroup>
          <Button onClick={() => onSelect(selectedDataBlockId)}>Embed</Button>
          {!hideCancel && (
            <Button variant="secondary" onClick={onCancel}>
              Cancel
            </Button>
          )}
        </ButtonGroup>
      )}
    </form>
  );
};

export default DataBlockSelectForm;
