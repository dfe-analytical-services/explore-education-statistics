import Page from '@admin/components/Page';
import React from 'react';
import SignInButton from '@admin/components/SignInButton';

const SignedOutPage = () => {
  return (
    <Page title="Signed out" caption="Explore education statistics">
      <p>You have successfully signed out.</p>
      <SignInButton />
    </Page>
  );
};

export default SignedOutPage;
