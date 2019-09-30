/* eslint-disable */
import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import React, { useContext } from 'react';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import tableBuilderService, {
  ThemeMeta, TableDataQuery,
} from '@common/modules/full-table/services/tableBuilderService';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import TableTool from '@common/modules/table-tool/components/TableTool';
import Button from '@common/components/Button';
import { DataBlockRequest, GeographicLevel, TimeIdentifier } from '@common/services/dataBlockService';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import { FormFieldset, FormTextInput } from '@common/components/form';
import FormTextArea from '@common/components/form/FormTextArea';

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

  const [dataBlockTitle, setDataBlockTitle] = React.useState<string>();
  const [dataBlockSource, setDataBlockSource] = React.useState<string>();
  const [dataBlockFootnotes, setDataBlockFootnotes] = React.useState<string>();
  const [dataBlockName, setDataBlockName] = React.useState<string>();

  return (
    <div>
      {themeMeta && publicationId !== undefined && (
        <TableTool
          publicationId={publicationId}
          themeMeta={themeMeta}
          finalStepHeading="Configure data block"
          finalStepExtra={({query, tableHeaders}) => (
            <DataBlockDetailsForm
              query={query}
              tableHeaders={tableHeaders}
            />
          )}
        />
      )}
    </div>
  );
};

export default CreateDataBlocks;
