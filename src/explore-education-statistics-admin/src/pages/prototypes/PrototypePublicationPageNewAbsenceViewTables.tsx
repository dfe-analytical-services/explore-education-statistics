import React from 'react';
import { FormSelect, FormFieldset } from '@common/components/form';
import Link from '@admin/components/Link';
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
      <PrototypeAdminNavigation sectionId="addTable" />

      <div className="govuk-tabs">
        <ul className="govuk-tabs__list">
          <li className="govuk-tabs__list-item">
            <Link
              to="/prototypes/publication-create-new-absence-table"
              className="govuk-tabs__tab"
            >
              Create tables and charts
            </Link>
          </li>
          <li className="govuk-tabs__list-item">
            <Link
              to="/prototypes/publication-create-new-absence-view-table"
              className="govuk-tabs__tab  govuk-tabs__tab--selected"
            >
              View saved tables and charts
            </Link>
          </li>
        </ul>
        <div className="govuk-tabs__panel">
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
        </div>
      </div>

      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to="/prototypes/publication-create-new-absence-data">
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Previous step
            </span>
            Manage data
          </Link>
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          <Link to="/prototypes/publication-create-new-absence">
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Next step
            </span>
            Manage content
          </Link>
        </div>
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
