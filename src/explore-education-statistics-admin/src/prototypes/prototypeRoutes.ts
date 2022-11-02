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
import PrototypeManageUsers from './PrototypeManageUsers';
import PrototypeAdminDashboard from './PrototypeAdminDashboard';
import PrototypeAdminPublication from './PrototypeAdminPublication';
import PrototypeAdminMethodology from './PrototypeAdminMethodology';
import PrototypeAdminContact from './PrototypeAdminContact';
import PrototypeAdminDetails from './PrototypeAdminDetails';
import PrototypeAdminAccess from './PrototypeAdminAccess';
import PrototypeAdminLegacy from './PrototypeAdminLegacy';
import PrototypeReleaseSummary from './PrototypeReleaseSummary';
import PrototypeFindStats from './PrototypeFindStats';
import PrototypeFindStats2 from './PrototypeFindStats2';
import PrototypeFindStats3 from './PrototypeFindStats3';
import PrototypeFindStats4 from './PrototypeFindStats4';
import PrototypeFindStats5 from './PrototypeFindStats5';
import PrototypeFindStats6 from './PrototypeFindStats6';
import PrototypeTableTool from './PrototypeTableToolPage';

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
  {
    name: 'Manage users',
    path: '/prototypes/manage-users',
    component: PrototypeManageUsers,
  },
  {
    name: 'Admin dashboard',
    path: '/prototypes/admin-dashboard',
    component: PrototypeAdminDashboard,
  },
  {
    name: 'Admin publication',
    path: '/prototypes/admin-publication',
    component: PrototypeAdminPublication,
  },
  {
    name: 'Admin methodology',
    path: '/prototypes/admin-methodology',
    component: PrototypeAdminMethodology,
  },
  {
    name: 'Admin contact',
    path: '/prototypes/admin-contact',
    component: PrototypeAdminContact,
  },
  {
    name: 'Admin publication details',
    path: '/prototypes/admin-details',
    component: PrototypeAdminDetails,
  },
  {
    name: 'Admin manage access',
    path: '/prototypes/admin-access',
    component: PrototypeAdminAccess,
  },
  {
    name: 'Admin legacy releases',
    path: '/prototypes/admin-legacy',
    component: PrototypeAdminLegacy,
  },
  {
    name: 'Admin release summary',
    path: '/prototypes/admin-release-summary',
    component: PrototypeReleaseSummary,
  },
  {
    name: 'Find statistics and data',
    path: '/prototypes/find-statistics',
    component: PrototypeFindStats,
  },
  {
    name: 'Find statistics and data v2',
    path: '/prototypes/find-statistics2',
    component: PrototypeFindStats2,
  },
  {
    name: 'Find statistics and data v3',
    path: '/prototypes/find-statistics3',
    component: PrototypeFindStats3,
  },
  {
    name: 'Find statistics and data v4',
    path: '/prototypes/find-statistics4',
    component: PrototypeFindStats4,
  },
  {
    name: 'Find statistics and data v5',
    path: '/prototypes/find-statistics5',
    component: PrototypeFindStats5,
  },
  {
    name: 'Find statistics and data v6',
    path: '/prototypes/find-statistics6',
    component: PrototypeFindStats6,
  },
  {
    name: 'Table tool',
    path: '/prototypes/table-tool',
    component: PrototypeTableTool,
  },
];

export default prototypeRoutes;
