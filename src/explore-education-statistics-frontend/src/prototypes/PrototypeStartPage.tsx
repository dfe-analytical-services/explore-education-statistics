import React from 'react';
// import CollapsibleSection from '../components/CollapsibleSection';
// import Details from '../components/Details';
// import PrototypeDataSample from './components/PrototypeDataSample';
// import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';

const StartPage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Schools' }]}>
      <h1 className="govuk-heading-l">START PAGE</h1>
      <p className="govuk-body">
        Here you can find DfE stats for schools, customise and download as excel
        files, and access them via an API. <a href="#">Find out more</a>
      </p>
    </PrototypePage>
  );
};

export default StartPage;
