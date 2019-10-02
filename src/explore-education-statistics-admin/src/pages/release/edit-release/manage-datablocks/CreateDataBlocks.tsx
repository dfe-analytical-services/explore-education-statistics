/* eslint-disable */
import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import React, { useContext } from 'react';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import tableBuilderService, {
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import TableTool from '@common/modules/table-tool/components/TableTool';
import { DataBlockRequest, DataBlockResponse } from '@common/services/dataBlockService';

interface Props {
  dataBlockRequest?: DataBlockRequest,
  dataBlockResponse?: DataBlockResponse
}

const CreateDataBlocks = (
  {
    dataBlockRequest,
    dataBlockResponse
  } : Props
) => {
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
            />
          )}
        />
      )}
    </div>
  );
};

export default CreateDataBlocks;
