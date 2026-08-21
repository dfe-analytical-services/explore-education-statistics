import { NavRouteProps } from '@admin/routes/types';

export type EducationInNumbersRouteProps = NavRouteProps;

export type EducationInNumbersRouteParams = {
  educationInNumbersPageId: string;
};

export const educationInNumbersSummaryRoute: EducationInNumbersRouteProps = {
  path: '/education-in-numbers/:educationInNumbersPageId/summary',
  title: 'Summary',
};

export const educationInNumbersSummaryEditRoute: EducationInNumbersRouteProps =
  {
    path: '/education-in-numbers/:educationInNumbersPageId/summary/edit',
    title: 'Edit summary',
  };

export const educationInNumbersContentRoute: EducationInNumbersRouteProps = {
  path: '/education-in-numbers/:educationInNumbersPageId/content',
  title: 'Manage content',
};

export const educationInNumbersSignOffRoute: EducationInNumbersRouteProps = {
  path: '/education-in-numbers/:educationInNumbersPageId/sign-off',
  title: 'Sign off',
};

/**
 * The routes shown in the Education in Numbers nav bar, in the order they are
 * stepped through by the previous/next links.
 */
export const educationInNumbersNavRoutes: EducationInNumbersRouteProps[] = [
  educationInNumbersSummaryRoute,
  educationInNumbersContentRoute,
  educationInNumbersSignOffRoute,
];
