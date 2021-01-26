import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import dataBlockService, {
  ReleaseDataBlockSummary,
} from '@admin/services/dataBlockService';
import { FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React, { useMemo } from 'react';
import { generatePath, useHistory } from 'react-router';

const emptyDataBlocks: ReleaseDataBlockSummary[] = [];

interface Props {
  publicationId: string;
  releaseId: string;
  dataBlockId: string;
}

const DataBlockSelector = ({
  publicationId,
  releaseId,
  dataBlockId,
}: Props) => {
  const history = useHistory();

  const { value: dataBlocks = emptyDataBlocks, isLoading } = useAsyncRetry(
    () => dataBlockService.listDataBlocks(releaseId),
    [releaseId],
  );

  const dataBlockOptions = useMemo<SelectOption[]>(() => {
    return dataBlocks.map(dataBlock => ({
      value: dataBlock.id,
      label: dataBlock.name,
    }));
  }, [dataBlocks]);

  if (isLoading) {
    return null;
  }

  return (
    <FormSelect
      id="selectedDataBlock"
      name="selectedDataBlock"
      className="govuk-!-margin-bottom-4"
      label="Select a data block to edit"
      disabled={isLoading}
      order={FormSelect.unordered}
      value={dataBlockId}
      options={dataBlockOptions}
      onChange={e => {
        history.push(
          generatePath<ReleaseDataBlockRouteParams>(
            releaseDataBlockEditRoute.path,
            {
              publicationId,
              releaseId,
              dataBlockId: e.target.value,
            },
          ),
        );
      }}
    />
  );
};

export default DataBlockSelector;
