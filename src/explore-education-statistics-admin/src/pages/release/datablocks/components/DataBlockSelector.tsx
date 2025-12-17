import {
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import dataBlockService, {
  ReleaseDataBlockSummary,
} from '@admin/services/dataBlockService';
import { FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React, { useMemo } from 'react';
import { generatePath, useHistory } from 'react-router';
import ButtonLink from '@admin/components/ButtonLink';

const emptyDataBlocks: ReleaseDataBlockSummary[] = [];

interface Props {
  canUpdate?: boolean;
  publicationId: string;
  releaseVersionId: string;
  dataBlockId: string;
}

const DataBlockSelector = ({
  canUpdate = true,
  publicationId,
  releaseVersionId,
  dataBlockId,
}: Props) => {
  const history = useHistory();

  const { value: dataBlocks = emptyDataBlocks, isLoading } = useAsyncRetry(
    () => dataBlockService.listDataBlocks(releaseVersionId),
    [releaseVersionId],
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

  const releaseDataBlockPath = generatePath<ReleaseRouteParams>(
    releaseDataBlockCreateRoute.path,
    {
      publicationId,
      releaseVersionId,
    },
  );

  return (
    <div className="dfe-flex dfe-align-items--center dfe-flex-wrap govuk-!-margin-top-1">
      <FormSelect
        label={
          canUpdate
            ? 'Select a data block to edit'
            : 'Select a data block to view'
        }
        labelClassName="govuk-!-width-full govuk-!-margin-bottom-1"
        id="selectedDataBlock"
        name="selectedDataBlock"
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
                releaseVersionId,
                dataBlockId: e.target.value,
              },
            ),
          );
        }}
      />
      {canUpdate && (
        <>
          <p className="govuk-!-font-weight-bold govuk-!-margin-right-4 govuk-!-margin-left-4 govuk-!-margin-bottom-0">
            or
          </p>
          <ButtonLink className="govuk-!-margin-0" to={releaseDataBlockPath}>
            Create another data block
          </ButtonLink>
        </>
      )}
    </div>
  );
};

export default DataBlockSelector;
