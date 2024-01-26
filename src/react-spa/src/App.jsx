import React, { useState } from 'react';

import { PageLayout } from './components/PageLayout';
import { adminApiLoginRequest, graphLoginRequest } from './authConfig';
import { ProfileData } from './components/ProfileData';

import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
  useMsal,
} from '@azure/msal-react';
import './App.css';
import Button from 'react-bootstrap/Button';
import { callAdminApi } from './adminApi';

/**
 * Renders information about the signed-in user or a button to retrieve data about the user
 */

const ProfileContent = () => {
  const { instance, accounts } = useMsal();
  const [themesData, setThemesData] = useState(null);

  function RequestThemesData() {
    // Silently acquires an access token which is then attached to the Admin API request.
    instance
      .acquireTokenSilent({
        ...adminApiLoginRequest,
        account: {
          ...accounts[0],
          environment: undefined, // it was necessary to remove this, as it was causing the refresh tokens to
          // not be found in the cache for some reason!
        },
      })
      .then(response => {
        callAdminApi(response.accessToken).then(response =>
          setThemesData(response),
        );
      });
  }

  return (
    <>
      <h5 className="profileContent">Welcome {accounts[0].name}</h5>
      {themesData ? (
        <p>{JSON.stringify(themesData)}</p>
      ) : (
        <Button variant="secondary" onClick={RequestThemesData}>
          See my themes
        </Button>
      )}
    </>
  );
};

/**
 * If a user is authenticated the ProfileContent component above is rendered. Otherwise a message indicating a user is not authenticated is rendered.
 */
const MainContent = () => {
  return (
    <div className="App">
      <AuthenticatedTemplate>
        <ProfileContent />
      </AuthenticatedTemplate>

      <UnauthenticatedTemplate>
        <h5 className="card-title">
          Please sign-in to see your profile information.
        </h5>
      </UnauthenticatedTemplate>
    </div>
  );
};

export default function App() {
  return (
    <PageLayout>
      <MainContent />
    </PageLayout>
  );
}
