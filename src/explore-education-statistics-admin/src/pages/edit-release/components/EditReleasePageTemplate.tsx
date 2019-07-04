import React, { ReactNode } from 'react';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import NavLink from '@admin/components/NavLink';
import { RouteComponentProps, withRouter } from 'react-router';
import Page from '../../../components/Page';
import editReleaseRoutes from '../../../routes/editReleaseRoutes';

interface Props extends RouteComponentProps {
  releaseId: string;
  children: ReactNode;
  publicationTitle: string;
}

const EditReleasePageTemplate = withRouter(
  ({ releaseId, publicationTitle, children, location }: Props) => {
    const currentRouteIndex =
      editReleaseRoutes.findIndex(
        route => route.generateLink(releaseId) === location.pathname,
      ) || 0;

    const previousRoute =
      currentRouteIndex > 0
        ? editReleaseRoutes[currentRouteIndex - 1]
        : undefined;

    const nextRoute =
      currentRouteIndex < editReleaseRoutes.length - 1
        ? editReleaseRoutes[currentRouteIndex + 1]
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
          {
            link: '/admin-dashboard',
            name: 'Administrator dashboard',
          },
          { name: 'Edit release', link: '#' },
        ]}
      >
        <h1 className="govuk-heading-l">
          {publicationTitle}
          <span className="govuk-caption-l">Edit release</span>
        </h1>
        <nav className="app-navigation govuk-!-margin-bottom-9">
          <ul className="app-navigation__list govuk-!-margin-bottom-0">
            <li>
              {editReleaseRoutes.map(route => (
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

export default EditReleasePageTemplate;
