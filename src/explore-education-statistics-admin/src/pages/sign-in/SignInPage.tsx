import React from 'react';
import Page from '@admin/components/Page';
import ButtonLink from '@admin/components/ButtonLink';

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
      <ButtonLink
        to="/tools/azuread/account/signin"
        className="govuk-button--start"
      >
        Sign-in
      </ButtonLink>
    </Page>
  );
};

export default SignInPage;
