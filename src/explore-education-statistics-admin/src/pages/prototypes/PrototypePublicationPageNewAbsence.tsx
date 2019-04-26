import React from 'react';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';
import { EditablePublicationPage } from './components/EditablePublicationPage';
import { PrototypePublicationService } from '@admin/pages/prototypes/components/PrototypePublicationService';

const PublicationPage = () => {
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
      <PrototypeAdminNavigation sectionId="addContent" />

      <EditablePublicationPage
        editing={true}
        data={PrototypePublicationService.getNewPublication()}
      />
    </PrototypePage>
  );
};

export default PublicationPage;
