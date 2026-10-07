import EducationInNumbersContentPage from '@admin/pages/education-in-numbers/content/EducationInNumbersContentPage';
import EducationInNumbersSignOffPage from '@admin/pages/education-in-numbers/sign-off/EducationInNumbersSignOffPage';
import EducationInNumbersSummaryEditPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryEditPage';
import EducationInNumbersSummaryPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryPage';
import {
  educationInNumbersContentRoute,
  educationInNumbersSignOffRoute,
  educationInNumbersSummaryEditRoute,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import { NavRouteProps } from '@admin/routes/types';

const educationInNumbersPageRoutes: NavRouteProps[] = [
  {
    ...educationInNumbersSummaryRoute,
    element: <EducationInNumbersSummaryPage />,
  },
  {
    ...educationInNumbersContentRoute,
    element: <EducationInNumbersContentPage />,
  },
  {
    ...educationInNumbersSignOffRoute,
    element: <EducationInNumbersSignOffPage />,
  },
  {
    ...educationInNumbersSummaryEditRoute,
    element: <EducationInNumbersSummaryEditPage />,
  },
];

export default educationInNumbersPageRoutes;
