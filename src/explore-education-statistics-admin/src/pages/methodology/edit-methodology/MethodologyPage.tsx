import Link from '@admin/components/Link';
import NavLink from '@admin/components/NavLink';
import Page from '@admin/components/Page';
import methodologyRoutes from '@admin/routes/edit-methodology/routes';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import RelatedInformation from '@common/components/RelatedInformation';
import React from 'react';
import { RouteComponentProps, Switch, Route } from 'react-router';

const MethodologyPage = ({
  match,
}: RouteComponentProps<{ methodologyId: string }> & ErrorControlProps) => {
  const { methodologyId } = match.params;

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Manage methodologies', link: '/methodologies' },
        { name: 'Edit methodology' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl">Edit methodology</span>
            [[methodology title]]
          </h1>
        </div>

        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation" target="blank">
                  Creating new methodology
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <nav className="app-navigation govuk-!-margin-top-6 govuk-!-margin-bottom-9">
        <ul className="app-navigation__list govuk-!-margin-bottom-0">
          {methodologyRoutes.map(route => (
            <li key={route.path}>
              <NavLink key={route.path} to={route.generateLink(methodologyId)}>
                {route.title}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      <Switch>
        {methodologyRoutes.map(route => (
          <Route
            exact
            key={route.path}
            path={route.path}
            component={route.component}
          />
        ))}
      </Switch>
    </Page>
  );
};

export default withErrorControl(MethodologyPage);
