import React from 'react';
import Details from '@common/components/Details';
import PrototypeAdminExampleTables from './components/PrototypeAdminExampleTables';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';
import Link from '../../components/Link';

const PublicationDataPage = () => {
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

      <h2 className="govuk-heading-m">
        View and edit tables configured for use in this release
      </h2>

      <Details summary="Table for absence highlights panel">
        <PrototypeAdminExampleTables task="view" />
      </Details>

      <Details summary="Example table 2">
        <PrototypeAdminExampleTables task="view" />
      </Details>

      <Details summary="Example table 3">
        <PrototypeAdminExampleTables task="view" />
      </Details>

      <Details summary="Example table 4">
        <PrototypeAdminExampleTables task="view" />
      </Details>

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
