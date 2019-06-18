import React from 'react';
import { FormSelect, FormGroup, FormFieldset } from '@common/components/form';
import PrototypeAdminExampleTables from './components/PrototypeAdminExampleTables';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

import Data from './PrototypeData';

const PublicationDataPage = () => {
  const [selectedTable, updateSelectedTable] = React.useState(
    Object.values(Data.tables)[0].value,
  );

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=editNewRelease',
          text: 'Administrator dashboard',
        },
        { text: 'Create new release', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId="viewTables" />

      <FormFieldset
        id="tableFieldset"
        legend="View and edit tables configured for use in this release"
      >
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
    </PrototypePage>
  );
};

export default PublicationDataPage;
