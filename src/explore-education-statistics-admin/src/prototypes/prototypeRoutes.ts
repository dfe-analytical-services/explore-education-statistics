import { RouteProps } from 'react-router';
import PrototypeExamplePage from './PrototypeExamplePage';
import PrototypePublicMetadata from './PrototypePublicMetadata';
import PrototypeBauGlossary from './PrototypeBauGlossary';
import PrototypePreRelease from './PrototypePreRelease';

interface PrototypeRoute extends RouteProps {
  name: string;
  path: string;
}

const prototypeRoutes: PrototypeRoute[] = [
  {
    name: 'Example prototype',
    path: '/prototypes/example',
    component: PrototypeExamplePage,
  },
  {
    name: 'Public metadata',
    path: '/prototypes/public-metadata',
    component: PrototypePublicMetadata,
  },
  {
    name: 'BAU manage glossary',
    path: '/prototypes/manage-glossary',
    component: PrototypeBauGlossary,
  },
  {
    name: 'Pre release access',
    path: '/prototypes/pre-release',
    component: PrototypePreRelease,
  },
];

export default prototypeRoutes;
