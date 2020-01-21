import { Authentication } from '@admin/services/sign-in/types';
import PrototypeLoginService from '@admin/services/PrototypeLoginService';
import * as React from 'react';

const LoginContext = React.createContext<Authentication>({
  user: undefined,
});

export const PrototypeLoginContext = React.createContext<Authentication>(
  PrototypeLoginService.login(),
);

export default LoginContext;
