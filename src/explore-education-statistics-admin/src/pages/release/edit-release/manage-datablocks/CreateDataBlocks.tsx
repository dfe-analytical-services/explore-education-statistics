/* eslint-disable */
import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import tableBuilderService, {
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import TableTool from '@common/modules/table-tool/components/TableTool';
import {
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useContext } from 'react';

interface Props {
  dataBlockRequest?: DataBlockRequest;
  dataBlockResponse?: DataBlockResponse;

  onDataBlockSave: (dataBlock: DataBlock) => Promise<DataBlock>;
}

const CreateDataBlocks = ({
  dataBlockRequest,
  dataBlockResponse,
  onDataBlockSave,
}: Props) => {
  const { publication, releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  const [themeMeta, setThemeMeta] = React.useState<ThemeMeta[]>();
  const [publicationId, setPublicationId] = React.useState<string>(
    publication.id,
  );

  React.useEffect(() => {
    tableBuilderService.getThemes().then(themeMeta => {
      setThemeMeta(themeMeta);
    });
  }, []);

  return (
    <div>
      {themeMeta && publicationId !== undefined && (
        <TableTool
          publicationId={publicationId}
          themeMeta={themeMeta}
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
