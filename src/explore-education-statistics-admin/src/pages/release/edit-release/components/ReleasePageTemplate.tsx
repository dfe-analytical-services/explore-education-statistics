import NavLink from '@admin/components/NavLink';
import Page from '@admin/components/Page';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import {viewRoutes} from '@admin/routes/edit-release/routes';
import React, {ReactNode} from 'react';
import {RouteComponentProps, withRouter} from 'react-router';

interface Props extends RouteComponentProps {
  releaseId: string;
  children: ReactNode;
  publicationTitle: string;
}

const ReleasePageTemplate = withRouter(
  ({ releaseId, publicationTitle, children, location }: Props) => {
    const currentRouteIndex =
      viewRoutes.findIndex(
        route => route.generateLink(releaseId) === location.pathname,
      ) || 0;

    const previousRoute =
      currentRouteIndex > 0 ? viewRoutes[currentRouteIndex - 1] : undefined;

    const nextRoute =
      currentRouteIndex < viewRoutes.length - 1
        ? viewRoutes[currentRouteIndex + 1]
        : undefined;

    const previousSection = previousRoute
      ? {
          label: previousRoute.title,
          linkTo: previousRoute.generateLink(releaseId),
        }
      : undefined;

    const nextSection = nextRoute
      ? { label: nextRoute.title, linkTo: nextRoute.generateLink(releaseId) }
      : undefined;

    return (
      <Page
        wide
        breadcrumbs={[
          { name: 'Edit release' },
        ]}
      >
        <h1 className="govuk-heading-l">
          {publicationTitle}
          <span className="govuk-caption-l">Edit release</span>
        </h1>
        <nav className="app-navigation govuk-!-margin-bottom-9">
          <ul className="app-navigation__list govuk-!-margin-bottom-0">
            <li>
              {viewRoutes.map(route => (
                <NavLink
                  key={route.path}
                  to={route.generateLink(releaseId)}
                  activeClassName="app-navigation--current-page"
                >
                  {route.title}
                </NavLink>
              ))}
            </li>
          </ul>
        </nav>
        {children}
        <PreviousNextLinks
          previousSection={previousSection}
          nextSection={nextSection}
        />
      </Page>
    );
  },
);

export default ReleasePageTemplate;
