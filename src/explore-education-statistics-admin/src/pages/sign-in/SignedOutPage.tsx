import Page from '@admin/components/Page';
import loginService from '@admin/services/loginService';
import React from 'react';

const SignedOutPage = () => {
  return (
    <Page>
      <h1 className="govuk-heading-l">
        <span className="govuk-caption-l">Explore education statistics</span>
        Signed out
      </h1>
      <p className="govuk-body">You have successfully signed out.</p>
      <a
        href={loginService.getSignInLink()}
        className="govuk-button govuk-button--start"
      >
        Sign-in
        <svg
          className="govuk-button__start-icon"
          xmlns="http://www.w3.org/2000/svg"
          width="17.5"
          height="19"
          viewBox="0 0 33 40"
          aria-hidden="true"
          focusable="false"
        >
          <path fill="currentColor" d="M0 0h13l20 20-20 20H0l20-20z" />
        </svg>
      </a>
    </Page>
  );
};

export default SignedOutPage;
