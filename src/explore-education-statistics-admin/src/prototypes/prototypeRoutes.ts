import { RouteProps } from 'react-router';
import PrototypeExamplePage from './PrototypeExamplePage';
import PrototypeReplaceData from './PrototypeReplaceData';
import PrototypeMetadata from './PrototypeMetadata';
import PrototypePublicMetadata from './PrototypePublicMetadata';
import PrototypeBauGlossary from './PrototypeBauGlossary';
import PrototypePreRelease from './PrototypePreRelease';
import PrototypePublicPreRelease from './PrototypePublicPreRelease';
import PrototypeTableHighlights from './PrototypeTableHighlights';
import PrototypeHomepage from './PrototypeHomepage';
import PrototypeHomepage2 from './PrototypeHomepage2';
import PrototypeRelease from './PrototypeRelease';

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
    name: 'Replace existing data',
    path: '/prototypes/replaceData',
    component: PrototypeReplaceData,
  },
  {
    name: 'Create Public metadata',
    path: '/prototypes/metadata',
    component: PrototypeMetadata,
  },
  {
    name: 'View Public metadata',
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
  {
    name: 'Public Pre release list',
    path: '/prototypes/public-pre-release',
    component: PrototypePublicPreRelease,
  },
  {
    name: 'Homepage A',
    path: '/prototypes/homepage',
    component: PrototypeHomepage,
  },
  {
    name: 'Homepage B',
    path: '/prototypes/homepage2',
    component: PrototypeHomepage2,
  },
  {
    name: 'Table highlights',
    path: '/prototypes/table-highlights',
    component: PrototypeTableHighlights,
  },
  {
    name: 'Release',
    path: '/prototypes/release',
    component: PrototypeRelease,
  },
];

export default prototypeRoutes;
