import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import EducationInNumbersContentPage from '@admin/pages/education-in-numbers/content/EducationInNumbersContentPage';
import { EducationInNumbersPageContextProvider } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import EducationInNumbersSignOffPage from '@admin/pages/education-in-numbers/sign-off/EducationInNumbersSignOffPage';
import EducationInNumbersSummaryEditPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryEditPage';
import EducationInNumbersSummaryPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryPage';
import {
  educationInNumbersContentRoute,
  EducationInNumbersRouteParams,
  EducationInNumbersRouteProps,
  educationInNumbersSignOffRoute,
  educationInNumbersSummaryEditRoute,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import useCurrentRouteTitle from '@admin/utils/useCurrentRouteTitle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';

const navRoutes: EducationInNumbersRouteProps[] = [
  educationInNumbersSummaryRoute,
  educationInNumbersContentRoute,
  educationInNumbersSignOffRoute,
];

const routes: EducationInNumbersRouteProps[] = [
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

const EducationInNumbersPage = ({
  match,
  location,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
  const { educationInNumbersPageId } = match.params;

  const {
    value: educationInNumbersPage,
    setState: setEducationInNumbersPage,
    isLoading,
  } = useAsyncHandledRetry(() => {
    return educationInNumbersService.getEducationInNumbersPage(
      educationInNumbersPageId,
    );
  }, [educationInNumbersPageId]);

  const currentRouteIndex =
    navRoutes.findIndex(
      route =>
        generatePath<EducationInNumbersRouteParams>(route.path, {
          educationInNumbersPageId,
        }) === location.pathname,
    ) || 0;

  const previousRoute =
    currentRouteIndex > 0 ? navRoutes[currentRouteIndex - 1] : undefined;

  const nextRoute =
    currentRouteIndex < navRoutes.length - 1
      ? navRoutes[currentRouteIndex + 1]
      : undefined;

  const previousSection = previousRoute
    ? {
        label: previousRoute.title,
        linkTo: generatePath<EducationInNumbersRouteParams>(
          previousRoute.path,
          {
            educationInNumbersPageId,
          },
        ),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: generatePath<EducationInNumbersRouteParams>(nextRoute.path, {
          educationInNumbersPageId,
        }),
      }
    : undefined;

  const pageRouteTitle = useCurrentRouteTitle(navRoutes);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Manage Education in Numbers', link: '/education-in-numbers' },
        { name: 'Edit Education in Numbers page' },
      ]}
    >
      <LoadingSpinner loading={isLoading}>
        {educationInNumbersPage ? (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <PageTitle
                  metaTitle={`${pageRouteTitle} - ${educationInNumbersPage.title}`}
                  title={educationInNumbersPage.title}
                  caption="Edit Education in numbers Page"
                />
              </div>
            </div>

            {GetStatusTag(educationInNumbersPage)}

            <NavBar
              routes={navRoutes.map(route => ({
                title: route.title,
                to: generatePath<EducationInNumbersRouteParams>(route.path, {
                  educationInNumbersPageId,
                }),
              }))}
              label="EducationInNumbers"
            />

            <EducationInNumbersPageContextProvider
              educationInNumbersPage={educationInNumbersPage}
              onEducationInNumbersPageChange={nextEducationInNumbersPage => {
                setEducationInNumbersPage({
                  value: nextEducationInNumbersPage,
                });
              }}
            >
              <Switch>
                {routes.map(route => (
                  <Route
                    exact
                    key={route.path}
                    path={route.path}
                    component={route.component}
                  />
                ))}
              </Switch>
            </EducationInNumbersPageContextProvider>

            <PreviousNextLinks
              previousSection={previousSection}
              nextSection={nextSection}
            />
          </>
        ) : (
          <WarningMessage>
            Could not load Education in Numbers page
          </WarningMessage>
        )}
      </LoadingSpinner>
    </Page>
  );
};

function GetStatusTag({ published, version }: EinSummary) {
  if (published === undefined) {
    return version === 0 ? <Tag>Draft</Tag> : <Tag>Draft amendment</Tag>;
  }

  return <Tag>Published</Tag>;
}

export default EducationInNumbersPage;
