import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import prototypeRoutes from '@admin/prototypes/prototypeRoutes';
import React from 'react';

const PrototypeIndexPage = () => {
  return (
    <PrototypePage title="Prototypes">
      <ul>
        {prototypeRoutes.map(route => {
          return (
            <li key={route.path}>
              <Link to={route.path}>{route.name}</Link>
            </li>
          );
        })}
      </ul>
    </PrototypePage>
  );
};

export default PrototypeIndexPage;
