import { Authentication } from '@admin/services/sign-in/types';
import PrototypeLoginService from '@admin/services/PrototypeLoginService';
import * as React from 'react';

const LoginContext = React.createContext<Authentication>({
  user: {
    id: 'guest',
    name: 'logged out',
    permissions: [],
  },
});

export const PrototypeLoginContext = React.createContext<Authentication>(
  PrototypeLoginService.login(),
);

export default LoginContext;
