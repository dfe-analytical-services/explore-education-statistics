import LoginContext from '@admin/components/Login';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import permissionService from '@admin/services/permissions/service';
import React, { useContext, useEffect, useState } from 'react';
import Page from '@admin/components/Page';
import Link from '@admin/components/Link';

interface Permissions {
  canManageUsers: boolean;
  canManageMethodologies: boolean;
}

const BauDashboardPage = ({ handleApiErrors }: ErrorControlProps) => {
  const { user } = useContext(LoginContext);
  const [permissions, setPermissions] = useState<Permissions>();

  useEffect(() => {
    Promise.all([
      permissionService.canManageAllUsers(user),
      permissionService.canManageAllMethodologies(user),
    ])
      .then(([canManageUsers, canManageMethodologies]) =>
        setPermissions({
          canManageUsers,
          canManageMethodologies,
        }),
      )
      .catch(handleApiErrors);
  }, [handleApiErrors]);

  return (
    <Page wide>
      <h1 className="govuk-heading-xl">Platform administration</h1>
      <ul>
        {permissions && permissions.canManageMethodologies && (
          <li>
            <Link to="/administration/methodology">
              View methodology status
            </Link>
          </li>
        )}
        {permissions && permissions.canManageUsers && (
          <li>
            <Link to="/administration/users">View service users</Link>
          </li>
        )}
      </ul>
    </Page>
  );
};

export default withErrorControl(BauDashboardPage);
