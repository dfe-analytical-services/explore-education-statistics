import React from 'react';
import PrototypePage from './components/PrototypePage';
import PrototypeReleaseConfig from './components/PrototypeReleasePageConfig';

const PublicationPage = () => {
  return (
    <PrototypePage
      wide
      breadcrumbs={[{ text: 'Create new release', link: '#' }]}
    >
      <h1 className="govuk-heading-xl">
        Pupil absence statistics and data for schools in England
        <span className="govuk-caption-l">Create new release</span>
      </h1>

      <PrototypeReleaseConfig />
    </PrototypePage>
  );
};

export default PublicationPage;
