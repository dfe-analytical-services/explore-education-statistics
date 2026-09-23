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
  releaseVersionId: string;
  onSelect: (selectedDataBlockVersionId: string) => void;
  onCancel?: () => void;
  hideCancel?: boolean;
  label?: string;
}

const DataBlockSelectForm = ({
  id,
  releaseVersionId,
  onSelect,
  onCancel = () => {},
  hideCancel = false,
  label = 'Select a data block',
}: Props) => {
  const { unattachedDataBlocks, release } = useReleaseContentState();
  const [selectedDataBlockVersionId, setSelectedDataBlockVersionId] =
    useState('');

  const getChartFile = useGetChartFile(release.id);

  const getDataBlockPreview = (dataBlockVersionId: string) => {
    const selectedDataBlock = unattachedDataBlocks.find(
      dataBlock => dataBlock.id === dataBlockVersionId,
    );
    return selectedDataBlock ? (
      <section>
        <Details
          className="govuk-!-margin-top-3"
          summary="Data block preview"
          open
        >
          <DataBlockTabs
            releaseVersionId={releaseVersionId}
            dataBlock={selectedDataBlock}
            id={`${id}-dataBlockPreview`}
            getInfographic={getChartFile}
          />
        </Details>
      </section>
    ) : null;
  };

  return (
    <form className="govuk-!-text-align-left" id={id}>
      <FormSelect
        autoFocus
        className="govuk-!-margin-right-1"
        id={`${id}-selectedDataBlock`}
        name="selectedDataBlock"
        label={label}
        value={selectedDataBlockVersionId}
        onChange={e => setSelectedDataBlockVersionId(e.target.value)}
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

      {getDataBlockPreview(selectedDataBlockVersionId)}

      {selectedDataBlockVersionId !== '' && (
        <ButtonGroup>
          <Button onClick={() => onSelect(selectedDataBlockVersionId)}>
            Embed
          </Button>
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
