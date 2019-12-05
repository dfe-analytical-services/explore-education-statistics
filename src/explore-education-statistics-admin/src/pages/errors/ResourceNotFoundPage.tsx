import Page from '@admin/components/Page';
import React from 'react';

const ResourceNotFoundPage = () => {
  return (
    <Page pageTitle="Resource not found">
      <h1 className="govuk-heading-l">Resource not found</h1>
      <p className="govuk-body">There was a problem accessing a resource.</p>
      <p className="govuk-body">Try again later.</p>
    </Page>
  );
};

export default ResourceNotFoundPage;
