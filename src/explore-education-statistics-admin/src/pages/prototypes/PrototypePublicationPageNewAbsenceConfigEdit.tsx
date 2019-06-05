import React from 'react';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';
import PrototypeReleaseConfig from './components/PrototypeReleasePageConfig';

const PublicationConfigEditPage = () => {
  const sectionId = 'setup';

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
      <PrototypeReleaseConfig sectionId={sectionId} />
    </PrototypePage>
  );
};

export default PublicationConfigEditPage;
