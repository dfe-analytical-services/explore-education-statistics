import { handleLogin } from '@admin/auth/msal';
import React from 'react';
import useQueryParams from '@admin/hooks/useQueryParams';
import StartButton from '@common/components/StartButton';

const SignInButton = () => {
  const { returnUrl } = useQueryParams();
  return (
    <StartButton
      id="signin-button"
      label="Sign in"
      onClick={() => handleLogin(returnUrl as string)}
    />
  );
};

export default SignInButton;
