import MethodologyContentPage from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import MethodologyReleaseStatusPage from '@admin/pages/methodology/edit-methodology/release-status/MethodologyReleaseStatusPage';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';

export interface MethodologyRoute {
  path: string;
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element;
  title: string;
  generateLink: (methodologyId: string) => string;
}

const createReadonlyRoute = (
  section: string,
  title: string,
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element,
): MethodologyRoute => {
  const path = `/methodologies/:methodologyId/${section}`;
  return {
    path,
    component,
    title,
    generateLink: (methodologyId: string) =>
      path.replace(':methodologyId', methodologyId),
  };
};

export const summaryRoute = createReadonlyRoute(
  'summary',
  'Summary',
  MethodologySummaryPage,
);
export const contentRoute = createReadonlyRoute(
  'content',
  'Manage content',
  MethodologyContentPage,
);
export const publishStatusRoute = createReadonlyRoute(
  'status',
  'Release status',
  MethodologyReleaseStatusPage,
);

export default [summaryRoute, contentRoute, publishStatusRoute];
