import Page from '@admin/components/Page';
import loginService from '@admin/services/sign-in/service';
import React from 'react';

const SignedOutPage = () => {
  return (
    <Page>
      <h1 className="govuk-heading-l">
        <span className="govuk-caption-l">Explore education statistics</span>
        Signed out
      </h1>
      <p className="govuk-body">You have successfully signed out.</p>
      <a href={loginService.getSignInLink()} className="govuk-button govuk-button--start">
        Sign-in
      </a>
    </Page>
  );
};

export default SignedOutPage;
