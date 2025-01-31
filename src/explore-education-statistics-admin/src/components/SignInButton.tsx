import { handleLogin } from '@admin/auth/msal';
import useQueryParams from '@admin/hooks/useQueryParams';
import StartButton from '@common/components/StartButton';
import getFirst from '@common/utils/getFirst';
import React from 'react';

export default function SignInButton() {
  const { returnUrl } = useQueryParams();
  return (
    <StartButton
      id="signin-button"
      label="Sign in"
      onClick={() => handleLogin(getFirst(returnUrl))}
    />
  );
}
