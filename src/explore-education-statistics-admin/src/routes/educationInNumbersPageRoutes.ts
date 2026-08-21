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
    component: EducationInNumbersSummaryPage,
  },
  {
    ...educationInNumbersContentRoute,
    component: EducationInNumbersContentPage,
  },
  {
    ...educationInNumbersSignOffRoute,
    component: EducationInNumbersSignOffPage,
  },
  {
    ...educationInNumbersSummaryEditRoute,
    component: EducationInNumbersSummaryEditPage,
  },
];

export default educationInNumbersPageRoutes;
