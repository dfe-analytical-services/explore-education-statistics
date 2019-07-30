import Page from '@admin/components/Page';
import loginService from '@admin/services/sign-in/service';
import React from 'react';

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
      <a href={loginService.getSignInLink()} className="govuk-button govuk-button--start">
        Sign-in
      </a>
    </Page>
  );
};

export default SignInPage;
