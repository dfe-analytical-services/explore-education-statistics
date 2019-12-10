import Page from '@admin/components/Page';
import React from 'react';

const ServiceProblemsPage = () => {
  return (
    <Page pageTitle="Sorry, there is a problem with the service">
      <h1 className="govuk-heading-l">
        Sorry, there is a problem with the service
      </h1>
      <p className="govuk-body">Try again later.</p>
    </Page>
  );
};

export default ServiceProblemsPage;
