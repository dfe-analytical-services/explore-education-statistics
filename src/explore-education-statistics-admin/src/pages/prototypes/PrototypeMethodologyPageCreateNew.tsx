import React from 'react';
import PrototypePage from './components/PrototypePage';
import PrototypeMethodologyConfig from './components/PrototypeMethodologyPageConfig';

const PublicationPage = () => {
  return (
    <PrototypePage
      wide
      breadcrumbs={[{ text: 'Create new methodology', link: '#' }]}
    >
      <h1 className="govuk-heading-xl">Create new methodology</h1>

      <PrototypeMethodologyConfig />
    </PrototypePage>
  );
};

export default PublicationPage;
