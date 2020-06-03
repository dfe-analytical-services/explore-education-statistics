import Page from '@admin/components/Page';
import { useAuthContext } from '@admin/contexts/AuthContext';
import loginService from '@admin/services/loginService';
import React from 'react';

const ForbiddenPage = () => {
  const { user } = useAuthContext();

  return (
    <Page pageTitle="Forbidden">
      <p className="govuk-body">
        You do not have permission to access this page.
      </p>
      {!user && (
        <>
          <p className="govuk-body">Log in and try again.</p>
          <a
            href={loginService.getSignInLink()}
            className="govuk-button govuk-button--start"
          >
            Sign-in
          </a>
        </>
      )}
    </Page>
  );
};

export default ForbiddenPage;
