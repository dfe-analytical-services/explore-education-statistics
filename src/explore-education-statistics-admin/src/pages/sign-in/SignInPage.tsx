import Page from '@admin/components/Page';
import React from 'react';
import SignInButton from '@admin/components/SignInButton';

const SignInPage = () => {
  return (
    <Page title="Sign in" caption="Explore education statistics">
      <p>
        Use this service to create and publish Department for Education (DfE)
        official and national statistics.
      </p>
      <SignInButton />
    </Page>
  );
};

export default SignInPage;
