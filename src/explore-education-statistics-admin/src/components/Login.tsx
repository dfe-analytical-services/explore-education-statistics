import * as React from 'react';
import { User } from '@admin/services/PrototypeLoginService';

export const LoginContext = React.createContext<User>({
  id: 'guest',
  name: 'logged out',
  permissions: [],
});

export default function() {
  // no-op at the moment
}
