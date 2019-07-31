import { Authentication } from '@admin/services/sign-in/service';
import * as React from 'react';

export const LoginContext = React.createContext<Authentication>({
  user: {
    id: 'guest',
    name: 'logged out',
    permissions: [],
  },
});

export default function() {
  // no-op at the moment
}
