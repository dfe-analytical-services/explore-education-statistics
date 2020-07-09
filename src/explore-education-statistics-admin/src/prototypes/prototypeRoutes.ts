import { RouteProps } from 'react-router';
import PrototypeExamplePage from './PrototypeExamplePage';

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
];

export default prototypeRoutes;
