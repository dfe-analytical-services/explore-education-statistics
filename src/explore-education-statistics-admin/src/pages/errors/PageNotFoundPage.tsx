import Page from '@admin/components/Page';
import React from 'react';

const PageNotFoundPage = () => {
  return (
    <Page pageTitle="Page not found">
      <h1 className="govuk-heading-l">Page not found</h1>
      <p className="govuk-body">
        If you typed the web address, check it is correct.
      </p>
      <p className="govuk-body">
        If you pasted the web address, check you copied the entire address.
      </p>
    </Page>
  );
};

export default PageNotFoundPage;
