import PreReleaseContentPage from '@admin/pages/release/pre-release/PreReleaseContentPage';
import PreReleaseMethodologiesPage from '@admin/pages/release/pre-release/PreReleaseMethodologiesPage';
import PreReleaseMethodologyPage from '@admin/pages/release/pre-release/PreReleaseMethodologyPage';
import PreReleaseTableToolPage from '@admin/pages/release/pre-release/PreReleaseTableToolPage';
import {
  preReleaseContentRoute,
  preReleaseMethodologiesRoute,
  preReleaseMethodologyRoute,
  preReleaseTableToolRoute,
} from '@admin/routes/preReleaseRoutes';
import { NavRouteProps } from '@admin/routes/types';

const preReleasePageRoutes: NavRouteProps[] = [
  {
    ...preReleaseContentRoute,
    component: PreReleaseContentPage,
  },
  {
    ...preReleaseTableToolRoute,
    component: PreReleaseTableToolPage,
  },
  {
    ...preReleaseMethodologiesRoute,
    component: PreReleaseMethodologiesPage,
  },
  {
    ...preReleaseMethodologyRoute,
    component: PreReleaseMethodologyPage,
  },
];

export default preReleasePageRoutes;
