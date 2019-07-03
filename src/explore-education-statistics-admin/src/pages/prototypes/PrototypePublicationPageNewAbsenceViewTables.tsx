import React from 'react';
import { FormSelect, FormFieldset } from '@common/components/form';
import PrototypeAdminExampleTables from './components/PrototypeAdminExampleTables';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';
import Link from '../../components/Link';

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
      <PrototypeAdminNavigation sectionId="addTable" />

      <Link to="/prototypes/publication-create-new-absence-table?status=step1">
        Build new table and chart
      </Link>

      <hr />

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

      <hr />

      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to="/prototypes/publication-create-new-absence-table?status=step1">
            Previous step, build tables
          </Link>
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          <Link to="/prototypes/publication-create-new-absence">
            Next step, add / edit content
          </Link>
        </div>
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
