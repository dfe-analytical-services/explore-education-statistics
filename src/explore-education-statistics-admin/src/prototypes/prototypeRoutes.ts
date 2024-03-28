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
import PrototypeHomepage3 from './PrototypeHomepage3';
import PrototypeHomepage4 from './PrototypeHomepage4';
import PrototypeHomepage5 from './PrototypeHomepage5';
import PrototypeHomepage6 from './PrototypeHomepage6';
import PrototypeRelease from './PrototypeRelease';
import PrototypeReleaseData from './PrototypeReleaseData';
import PrototypeReleaseData2 from './PrototypeReleaseData2';
import PrototypeManageUsers from './PrototypeManageUsers';
import PrototypeAdminDashboard from './PrototypeAdminDashboard';
import PrototypeAdminDashboard2 from './PrototypeAdminDashboard2';
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
import PrototypeTableHighlightsV2 from './PrototypeTableHighlightsUpdateV2';
import PrototypeTableHighlightsV2b from './PrototypeTableHighlightsUpdateV2-b';
import PrototypeTableHighlightsV2c from './PrototypeTableHighlightsUpdateV2-c';
import PrototypeDataCatalog from './PrototypeDataCatalog';
import PrototypeDataSelected from './PrototypeDataSelected';
import PrototypeDataSelected3 from './PrototypeDataSelected3';
import PrototypeDataSelected4 from './PrototypeDataSelected4';
import PrototypeDataSelected5 from './PrototypeDataSelected5';
import PrototypeMethodology from './PrototypeMethodology';
import PrototypeDashboard from './PrototypeDashboard';
import PrototypeDashboard2 from './PrototypeDashboard2';
import PrototypeHomepageDashboard from './PrototypeHomepageDashboard';
import PrototypeHomepageDashboard2 from './PrototypeHomepageDashboard2';
import PrototypeReleasePage from './admin-api/PrototypeReleasePage';
import PrototypeReleaseNav from './PrototypeReleaseNavigation';
import PrototypeReleaseNav2 from './PrototypeReleaseNavigation2';
import PrototypeReleaseNav3 from './PrototypeReleaseNavigation3';
import PrototypeReleaseNav4 from './PrototypeReleaseNavigation4';
import PrototypeReleaseNav5 from './PrototypeReleaseNavigation5';
import PrototypeReleaseNav6 from './PrototypeReleaseNavigation6';
import PrototypeReleaseNav7 from './PrototypeReleaseNavigation7';
import PrototypeReleaseNav8 from './PrototypeReleaseNavigation8';
import PrototypeReleaseNav9 from './PrototypeReleaseNavigation9';
import PrototypeReleaseContentPageView from './page-view/PrototypeReleaseContentPageView';

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
    name: 'ReleaseData',
    path: '/prototypes/releaseData2',
    component: PrototypeReleaseData2,
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
  {
    name: 'Table tool highlights V2',
    path: '/prototypes/table-highlights-2',
    component: PrototypeTableHighlightsV2,
  },
  {
    name: 'Table tool highlights V2-b',
    path: '/prototypes/table-highlights-2b',
    component: PrototypeTableHighlightsV2b,
  },
  {
    name: 'Table tool highlights V2-c',
    path: '/prototypes/table-highlights-2c',
    component: PrototypeTableHighlightsV2c,
  },
  {
    name: 'Data catalog',
    path: '/prototypes/data-catalog',
    component: PrototypeDataCatalog,
  },
  {
    name: 'Dataset selected',
    path: '/prototypes/data-selected',
    component: PrototypeDataSelected5,
  },
  {
    name: 'Dataset selected ORIGINAL',
    path: '/prototypes/data-selected2',
    component: PrototypeDataSelected,
  },
  {
    name: 'Dataset selected3',
    path: '/prototypes/data-selected3',
    component: PrototypeDataSelected3,
  },
  {
    name: 'Dataset selected4',
    path: '/prototypes/data-selected4',
    component: PrototypeDataSelected4,
  },
  {
    name: 'Dataset selected5',
    path: '/prototypes/data-selected5',
    component: PrototypeDataSelected5,
  },
  {
    name: 'Methodology',
    path: '/prototypes/methodology',
    component: PrototypeMethodology,
  },
  {
    name: 'Homepage Refresh 1',
    path: '/prototypes/homepage3',
    component: PrototypeHomepage3,
  },
  {
    name: 'Homepage Refresh 2',
    path: '/prototypes/homepage4',
    component: PrototypeHomepage4,
  },
  {
    name: 'Homepage Refresh 3',
    path: '/prototypes/homepage5',
    component: PrototypeHomepage5,
  },
  {
    name: 'Homepage Refresh 4',
    path: '/prototypes/homepage6',
    component: PrototypeHomepage6,
  },
  {
    name: 'ReleaseData',
    path: '/prototypes/releaseData',
    component: PrototypeReleaseData,
  },
  {
    name: 'Dashboard',
    path: '/prototypes/dashboard',
    component: PrototypeDashboard,
  },
  {
    name: 'Dashboard2',
    path: '/prototypes/dashboard2',
    component: PrototypeDashboard2,
  },
  {
    name: 'Homepage Dashboard',
    path: '/prototypes/homepageDashboard',
    component: PrototypeHomepageDashboard,
  },
  {
    name: 'Homepage Dashboard2',
    path: '/prototypes/homepageDashboard2',
    component: PrototypeHomepageDashboard2,
  },
  {
    name: 'API admin',
    path: '/prototypes/admin-api',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin summary',
    path: '/prototypes/admin-api/summary/:id',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin data',
    path: '/prototypes/admin-api/data/:id',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin data',
    path: '/prototypes/admin-api/data/:id/prepare-subject/:psid',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin footnotes',
    path: '/prototypes/admin-api/footnotes/:id',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin data-blocks',
    path: '/prototypes/admin-api/data-blocks/:id',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin content',
    path: '/prototypes/admin-api/content/:id',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin status',
    path: '/prototypes/admin-api/status/:id',
    component: PrototypeReleasePage,
  },
  {
    name: 'API admin pre-release',
    path: '/prototypes/admin-api/pre-release/:id',
    component: PrototypeReleasePage,
  },
  {
    name: 'Admin dashboard',
    path: '/prototypes/admin-dashboard2',
    component: PrototypeAdminDashboard2,
  },
  {
    name: 'Release navigation',
    path: '/prototypes/release-nav',
    component: PrototypeReleaseNav,
  },
  {
    name: 'Release navigation 2',
    path: '/prototypes/release-nav2',
    component: PrototypeReleaseNav2,
  },
  {
    name: 'Release navigation 3',
    path: '/prototypes/release-nav3',
    component: PrototypeReleaseNav3,
  },
  {
    name: 'Release navigation 4',
    path: '/prototypes/release-nav4',
    component: PrototypeReleaseNav4,
  },
  {
    name: 'Release navigation 5',
    path: '/prototypes/release-nav5',
    component: PrototypeReleaseNav5,
  },
  {
    name: 'Release navigation 6',
    path: '/prototypes/release-nav6',
    component: PrototypeReleaseNav6,
  },
  {
    name: 'Release navigation 7',
    path: '/prototypes/release-nav7',
    component: PrototypeReleaseNav7,
  },
  {
    name: 'Release navigation 8 updated',
    path: '/prototypes/release-nav8',
    component: PrototypeReleaseNav8,
  },
  {
    name: 'Release navigation 9 updated top nav',
    path: '/prototypes/release-nav9',
    component: PrototypeReleaseNav9,
  },
  {
    name: 'Release content - set page view',
    path: '/prototypes/release-content-page-view',
    component: PrototypeReleaseContentPageView,
  },
];

export default prototypeRoutes;
