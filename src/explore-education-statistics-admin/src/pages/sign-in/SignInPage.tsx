import React from 'react';
import Page from '@admin/components/Page';

const SignInPage = () => {
  return (
    <Page>
      <h1 className="govuk-heading-l">
        <span className="govuk-caption-l">Explore education statistics</span>
        Sign-in
      </h1>
      <p className="govuk-body">
        Use this service to publish official Department for Education (DfE)
        statistics and data for state-funded schools in England.
      </p>
      <a href="/api/signin" className="govuk-button govuk-button--start">
        Sign-in
      </a>
    </Page>
  );
};

export default SignInPage;
