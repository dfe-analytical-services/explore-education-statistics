import React from 'react';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';
import PrototypePublicationSummary from './components/PrototypePublicationPageSummary';

const PublicationConfigPage = () => {
  let sectionId = 'setup';

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=editRelease',
          text: 'Administrator dashboard',
        },
        { text: 'Create new release', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId={sectionId} />

      <PrototypePublicationSummary />
    </PrototypePage>
  );
};

export default PublicationConfigPage;
