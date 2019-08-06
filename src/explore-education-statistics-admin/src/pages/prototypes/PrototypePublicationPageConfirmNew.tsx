import React from 'react';
import PrototypePage from './components/PrototypePage';
import PrototypePublicationConfig from './components/PrototypePublicationPageConfig';

const PublicationPage = () => {
  const sectionId = 'confirmPublication';
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard',
          text: 'Administrator dashboard',
        },
        { text: 'Create new publication', link: '#' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        {' '}
        <span className="govuk-caption-l">
          Pupils and schools / Pupil absence
        </span>{' '}
        Create new publication
      </h1>
      <PrototypePublicationConfig sectionId={sectionId} />
    </PrototypePage>
  );
};

export default PublicationPage;
