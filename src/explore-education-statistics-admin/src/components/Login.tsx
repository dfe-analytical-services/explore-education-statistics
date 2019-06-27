import * as React from 'react';
import { Authentication } from '@admin/services/PrototypeLoginService';

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
