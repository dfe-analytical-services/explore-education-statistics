import { RouteProps } from 'react-router';
import PrototypeExamplePage from './PrototypeExamplePage';
import PrototypePublicMetadata from './PrototypePublicMetadata';

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
];

export default prototypeRoutes;
