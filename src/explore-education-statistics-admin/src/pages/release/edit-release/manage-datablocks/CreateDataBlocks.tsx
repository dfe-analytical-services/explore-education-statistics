import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import TableTool from '@common/modules/table-tool/components/TableTool';
import { DataBlockRequest, DataBlockResponse } from '@common/services/dataBlockService';
import React from 'react';

interface Props {
  releaseId: string,
  dataBlockRequest?: DataBlockRequest;
  dataBlockResponse?: DataBlockResponse;
  dataBlock?: DataBlock;

  onDataBlockSave: (dataBlock: DataBlock) => Promise<DataBlock>;
  onTableToolLoaded?: () => void;
}

const CreateDataBlocks = ({
  releaseId,
  dataBlockRequest,
  dataBlock,
  onDataBlockSave,
  onTableToolLoaded,
}: Props) => {

  console.log(dataBlock, dataBlockRequest);

  return (
    <div>
      {releaseId !== undefined && (
        <TableTool
          releaseId={releaseId}
          themeMeta={[]}
          initialQuery={dataBlockRequest}
          initialTableHeaders={
            (dataBlock &&
              dataBlock.tables &&
              dataBlock.tables.length > 0 &&
              dataBlock.tables[0].tableHeaders) ||
            undefined
          }
          onInitialQueryCompleted={onTableToolLoaded}
          finalStepHeading="Configure data block"
          finalStepExtra={({ query, tableHeaders }) => (
            <DataBlockDetailsForm
              query={query}
              tableHeaders={tableHeaders}
              initialDataBlock={dataBlock}
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
