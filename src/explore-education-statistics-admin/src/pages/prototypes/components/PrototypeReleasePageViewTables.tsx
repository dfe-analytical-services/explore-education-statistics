import React from 'react';
import { FormSelect, FormFieldset } from '@common/components/form';
import PrototypeAdminExampleTables from './PrototypeAdminExampleTables';

import Data from '../PrototypeData';

const PublicationDataPage = () => {
  const [selectedTable, updateSelectedTable] = React.useState(
    Object.values(Data.tables)[0].value,
  );

  return (
    <>
      <FormFieldset id="tableFieldset" legend="">
        <FormSelect
          id="tables"
          label="Select a table to view"
          name="tables"
          options={Object.values(Data.tables)}
          order={[]}
          value={selectedTable}
          onChange={e => updateSelectedTable(e.target.value)}
        />
      </FormFieldset>

      <PrototypeAdminExampleTables
        table={Data.tables[selectedTable]}
        task="view"
      />
    </>
  );
};

export default PublicationDataPage;
