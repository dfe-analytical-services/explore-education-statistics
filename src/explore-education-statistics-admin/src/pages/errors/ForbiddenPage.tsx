import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import React from 'react';
import SignInButton from '@admin/components/SignInButton';

const ForbiddenPage = () => {
  const { user } = useAuthContext();

  return (
    <Page title="Forbidden">
      <p className="govuk-body">
        You do not have permission to access this page.
      </p>
      {!user && (
        <>
          <p className="govuk-body">Log in and try again.</p>
          <SignInButton />
        </>
      )}
    </Page>
  );
};

export default ForbiddenPage;
