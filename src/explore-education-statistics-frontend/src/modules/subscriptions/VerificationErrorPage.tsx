import Page from '@frontend/components/Page';
import { NextPage } from 'next';
import React from 'react';
import VerificationErrorMessage from '@frontend/modules/subscriptions/components/VerificationErrorMessage';

const VerificationErrorPage: NextPage = () => {
  return (
    <Page title="Verification failed">
      <VerificationErrorMessage />
    </Page>
  );
};

export default VerificationErrorPage;
