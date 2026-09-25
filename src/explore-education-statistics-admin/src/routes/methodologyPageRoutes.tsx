import MethodologyContentPage from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import MethodologyStatusPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusPage';
import MethodologySummaryEditPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryEditPage';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import {
  methodologyContentRoute,
  methodologyStatusRoute,
  methodologySummaryEditRoute,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import { NavRouteProps } from '@admin/routes/types';

const methodologyPageRoutes: NavRouteProps[] = [
  {
    ...methodologySummaryRoute,
    element: <MethodologySummaryPage />,
  },
  {
    ...methodologyContentRoute,
    element: <MethodologyContentPage />,
  },
  {
    ...methodologyStatusRoute,
    element: <MethodologyStatusPage />,
  },
  {
    ...methodologySummaryEditRoute,
    element: <MethodologySummaryEditPage />,
  },
];

export default methodologyPageRoutes;
