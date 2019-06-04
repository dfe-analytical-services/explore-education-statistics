import React from 'react';
import Details from '@common/components/Details';
import PrototypeAdminExampleTables from './components/PrototypeAdminExampleTables';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

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
    </PrototypePage>
  );
};

export default PublicationDataPage;
