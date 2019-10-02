/* eslint-disable */
import React, { useContext } from 'react';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import tableBuilderService, {
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import TableTool from '@common/modules/table-tool/components/TableTool';
import Button from '@common/components/Button';

const CreateDataBlocks = () => {
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
          finalStepExtra={({ query }) => (
            <div>
              <Button
                type="button"
                onClick={() => {
                  console.log(query);
                }}
              >
                Save
              </Button>
            </div>
          )}
        />
      )}
    </div>
  );
};

export default CreateDataBlocks;
