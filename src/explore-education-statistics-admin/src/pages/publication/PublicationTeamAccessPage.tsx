import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  managePublicationDraftersPageRoute,
  ManagePublicationDraftersRouteParams,
} from '@admin/routes/publicationRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router-dom';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import orderBy from 'lodash/orderBy';
import ButtonLink from '@admin/components/ButtonLink';
import publicationRolesService from '@admin/services/user-management/publicationRolesService';

const PublicationTeamAccessPage = () => {
  const { publicationId, permissions } = usePublicationContext();

  const { value: publicationRoles, isLoading } = useAsyncHandledRetry(() =>
    publicationRolesService.listPublicationRoles(publicationId),
  );

  if (isLoading || !publicationRoles) {
    return <LoadingSpinner />;
  }

  const noDrafters = !publicationRoles.some(
    pr => pr.role === PublicationRole.Drafter,
  );
  const noApprovers = !publicationRoles.some(
    pr => pr.role === PublicationRole.Approver,
  );
  const hasRoles = publicationRoles.length > 0;

  let publicationAccessMessage = '';

  if (!hasRoles || (noDrafters && noApprovers)) {
    publicationAccessMessage =
      'There are no publication drafters or approvers. To change this please contact ';
  } else if (noDrafters) {
    publicationAccessMessage =
      'There are no publication drafters. To change this please contact ';
  } else if (noApprovers) {
    publicationAccessMessage =
      'There are no publication approvers. To change this please contact ';
  } else {
    publicationAccessMessage =
      "To edit the publication's drafters or approvers please contact ";
  }

  return (
    <>
      <h2>Manage team access</h2>

      <h3>
        {permissions.canUpdateDrafters
          ? 'Update publication access'
          : 'Publication access'}
      </h3>

      {hasRoles && (
        <div className="table-container">
          <table data-testid="publicationRoles">
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Email</th>
                <th scope="col">Publication role</th>
              </tr>
            </thead>
            <tbody>
              {orderBy(publicationRoles, role => [
                role.userName,
                role.role,
              ]).map(role => (
                <tr key={`${role.id}_${role.role}`}>
                  <td>{role.userName}</td>
                  <td>{role.email}</td>
                  <td>{role.role}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <p>
        {publicationAccessMessage}
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
        .
      </p>

      {permissions.canUpdateDrafters && (
        <ButtonLink
          to={generatePath<ManagePublicationDraftersRouteParams>(
            managePublicationDraftersPageRoute.path,
            {
              publicationId,
            },
          )}
        >
          Manage publication drafters
        </ButtonLink>
      )}
    </>
  );
};

export default PublicationTeamAccessPage;
