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
