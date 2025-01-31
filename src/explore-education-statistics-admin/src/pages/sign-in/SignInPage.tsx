import Page from '@admin/components/Page';
import SignInButton from '@admin/components/SignInButton';
import React from 'react';

export default function SignInPage() {
  return (
    <Page title="Sign in" caption="Explore education statistics">
      <p>
        Use this service to create and publish Department for Education (DfE)
        official and accredited official statistics.
      </p>
      <SignInButton />
    </Page>
  );
}
