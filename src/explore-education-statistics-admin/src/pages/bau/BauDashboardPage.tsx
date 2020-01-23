import Link from '@admin/components/Link';
import LoginContext from '@admin/components/Login';
import Page from '@admin/components/Page';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React, { useContext } from 'react';

const BauDashboardPage = ({ handleApiErrors }: ErrorControlProps) => {
  const { user } = useContext(LoginContext);

  const canManageUsers = user
    ? user.permissions.canAccessUserAdministrationPages
    : false;
  const canManageMethodologies = user
    ? user.permissions.canAccessMethodologyAdministrationPages
    : false;

  return (
    <Page wide>
      <h1 className="govuk-heading-xl">Platform administration</h1>
      <ul>
        {canManageMethodologies && (
          <li>
            <Link to="/administration/methodology">
              View methodology status
            </Link>
          </li>
        )}
        {canManageUsers && (
          <li>
            <Link to="/administration/users">View service users</Link>
          </li>
        )}
      </ul>
    </Page>
  );
};

export default withErrorControl(BauDashboardPage);
