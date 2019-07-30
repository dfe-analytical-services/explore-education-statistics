import React from 'react';
import Page from '@admin/components/Page';

const SignedOutPage = () => {
  return (
    <Page>
      <h1 className="govuk-heading-l">
        <span className="govuk-caption-l">Explore education statistics</span>
        Signed out
      </h1>
      <p className="govuk-body">
        You have successfully signed out.
      </p>
      <a href="/api/signin" className="govuk-button govuk-button--start">
        Sign-in
      </a>
    </Page>
  );
};

export default SignedOutPage;
