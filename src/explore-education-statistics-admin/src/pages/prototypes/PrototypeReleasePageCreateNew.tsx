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
        <span className="govuk-caption-xl">Create new release</span>
        Pupil absence statistics and data for schools in England
      </h1>

      <PrototypeReleaseConfig />
    </PrototypePage>
  );
};

export default PublicationPage;
