import Page from '@admin/components/Page';
import React from 'react';

const ResourceNotFoundPage = () => {
  return (
    <Page title="Resource not found">
      <p className="govuk-body">There was a problem accessing a resource.</p>
      <p className="govuk-body">Try again later.</p>
    </Page>
  );
};

export default ResourceNotFoundPage;
