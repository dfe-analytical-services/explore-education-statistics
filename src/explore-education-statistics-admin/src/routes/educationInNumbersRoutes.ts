import EducationInNumbersContentPage from '@admin/pages/education-in-numbers/content/EducationInNumbersContentPage';
import EducationInNumbersSummaryEditPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryEditPage';
import EducationInNumbersSummaryPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryPage';
import { RouteProps } from 'react-router';

export interface EducationInNumbersRouteProps extends RouteProps {
  path: string;
  title: string;
}

export type EducationInNumbersRouteParams = {
  educationInNumbersPageId: string;
};

export const educationInNumbersSummaryRoute: EducationInNumbersRouteProps = {
  path: '/education-in-numbers/:educationInNumbersPageId/summary',
  title: 'Summary',
  component: EducationInNumbersSummaryPage,
};

export const educationInNumbersSummaryEditRoute: EducationInNumbersRouteProps =
  {
    path: '/education-in-numbers/:educationInNumbersPageId/summary/edit',
    title: 'Edit summary',
    component: EducationInNumbersSummaryEditPage,
  };

export const educationInNumbersContentRoute: EducationInNumbersRouteProps = {
  path: '/education-in-numbers/:educationInNumbersPageId/content',
  title: 'Manage content',
  component: EducationInNumbersContentPage,
};
