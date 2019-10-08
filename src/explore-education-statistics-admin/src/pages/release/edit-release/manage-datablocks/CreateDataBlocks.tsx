import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import TableTool from '@common/modules/table-tool/components/TableTool';
import {
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useContext } from 'react';

interface Props {
  dataBlockRequest?: DataBlockRequest;
  dataBlockResponse?: DataBlockResponse;
  dataBlock?: DataBlock;

  onDataBlockSave: (dataBlock: DataBlock) => Promise<DataBlock>;
}

const CreateDataBlocks = ({
  dataBlockRequest,
  dataBlockResponse,
  dataBlock,
  onDataBlockSave,
}: Props) => {
  const { publication, releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  return (
    <div>
      {releaseId !== undefined && (
        <TableTool
          releaseId={releaseId}
          themeMeta={[]}
          initialQuery={dataBlockRequest}
          finalStepHeading="Configure data block"
          finalStepExtra={({ query, tableHeaders }) => (
            <DataBlockDetailsForm
              query={query}
              tableHeaders={tableHeaders}
              releaseId={releaseId}
              onDataBlockSave={onDataBlockSave}
            />
          )}
        />
      )}
    </div>
  );
};

export default CreateDataBlocks;
