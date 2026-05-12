import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import styles from '@admin/pages/publication/PublicationTeamAccessPage.module.scss';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import orderBy from 'lodash/orderBy';
import { useQuery } from '@tanstack/react-query';
import publicationRolesQueries from '@admin/queries/user-management/publicationRolesQueries';
import ModalConfirm from '@common/components/ModalConfirm';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import publicationRolesService from '@admin/services/user-management/publicationRolesService';
import InvitePublicationDrafterForm from './components/InvitePublicationDrafterForm';

const PublicationTeamAccessPage = () => {
  const { publicationId, permissions } = usePublicationContext();

  const {
    data: publicationRoles,
    isLoading: isLoadingRoles,
    refetch: refetchRoles,
  } = useQuery(publicationRolesQueries.listPublicationRoles(publicationId));

  const {
    data: publicationRoleInvites,
    isLoading: isLoadingInvites,
    refetch: refetchInvites,
  } = useQuery(
    publicationRolesQueries.listPublicationRoleInvites(publicationId),
  );

  if (
    isLoadingRoles ||
    isLoadingInvites ||
    !publicationRoles ||
    !publicationRoleInvites
  ) {
    return <LoadingSpinner />;
  }

  const publicationApprovers = publicationRoles.filter(
    pr => pr.role === PublicationRole.Approver,
  );
  const publicationApproverInvites = publicationRoleInvites.filter(
    pri => pri.role === PublicationRole.Approver,
  );
  const publicationDrafters = publicationRoles.filter(
    pr => pr.role === PublicationRole.Drafter,
  );
  const publicationDrafterInvites = publicationRoleInvites.filter(
    pri => pri.role === PublicationRole.Drafter,
  );

  const hasApprovers = publicationRoles.some(
    pr => pr.role === PublicationRole.Approver,
  );
  const hasApproverInvites = publicationRoleInvites.some(
    pri => pri.role === PublicationRole.Approver,
  );
  const hasDrafters = publicationRoles.some(
    pr => pr.role === PublicationRole.Drafter,
  );
  const hasDrafterInvites = publicationRoleInvites.some(
    pri => pri.role === PublicationRole.Drafter,
  );

  const hasAnyApprovers = hasApprovers || hasApproverInvites;
  const hasAnyDrafters = hasDrafters || hasDrafterInvites;

  const supportEmail = (
    <a href="mailto:explore.statistics@education.gov.uk">
      explore.statistics@education.gov.uk
    </a>
  );

  let approverAccessMessage: React.ReactNode;
  let drafterAccessMessage: React.ReactNode = null;

  if (hasAnyApprovers) {
    approverAccessMessage = (
      <p>To edit the publication's approvers please contact {supportEmail}.</p>
    );
  } else {
    approverAccessMessage = (
      <p>
        There are no approvers, or pending approver invites, for this
        publication. To change this please contact {supportEmail}.
      </p>
    );
  }

  if (permissions.canUpdateDrafters) {
    if (!hasAnyDrafters) {
      drafterAccessMessage = (
        <p>
          There are no drafters, or pending drafter invites, for this
          publication.
        </p>
      );
    }
  } else if (hasAnyDrafters) {
    drafterAccessMessage = (
      <p>To edit the publication's drafters please contact {supportEmail}.</p>
    );
  } else {
    drafterAccessMessage = (
      <p>
        There are no drafters, or pending drafter invites, for this publication.
        To change this please contact {supportEmail}.
      </p>
    );
  }

  const handleRemoveDrafterAccess = permissions.canUpdateDrafters
    ? async (roleId: string) => {
        await publicationRolesService.removePublicationDrafter(roleId);

        await refreshRolesAndInvites();
      }
    : undefined;

  const onInviteDrafter = async (email: string) => {
    await publicationRolesService.inviteDrafter({
      publicationId,
      email,
    });

    await refreshRolesAndInvites();
  };

  const refreshRolesAndInvites = async () => {
    await refetchRoles();
    await refetchInvites();
  };

  return (
    <>
      <h2>
        {permissions.canUpdateDrafters ? 'Manage team access' : 'Team access'}
      </h2>

      <h3 className="govuk-!-margin-bottom-0">Publication drafters</h3>

      {hasAnyDrafters && (
        <div className="table-container">
          <table data-testid="publicationDrafterRoles">
            <thead>
              <tr>
                <th className={styles.name} scope="col">
                  Name
                </th>
                <th scope="col">Email</th>
                {permissions.canUpdateDrafters && (
                  <th className={styles.actions} scope="col">
                    Actions
                  </th>
                )}
              </tr>
            </thead>
            <tbody>
              {orderBy(publicationDrafters, role => role.userName).map(role => (
                <tr key={role.email}>
                  <td>{role.userName}</td>
                  <td>{role.email}</td>
                  {handleRemoveDrafterAccess && (
                    <td>
                      <ModalConfirm
                        title="Confirm drafter removal"
                        triggerButton={
                          <ButtonText variant="warning">
                            Remove
                            <VisuallyHidden> {role.userName}</VisuallyHidden>
                          </ButtonText>
                        }
                        onConfirm={async () => {
                          await handleRemoveDrafterAccess(role.id);
                        }}
                      >
                        <p>
                          Are you sure you want to remove{' '}
                          <strong>{role?.userName}</strong> from this
                          publication?
                        </p>
                      </ModalConfirm>
                    </td>
                  )}
                </tr>
              ))}
              {orderBy(publicationDrafterInvites, invite => invite.email).map(
                invite => (
                  <tr key={invite.email}>
                    <td />
                    <td>
                      {invite.email}
                      <Tag className="govuk-!-margin-left-3">
                        Pending invite
                      </Tag>
                    </td>
                    {handleRemoveDrafterAccess && (
                      <td>
                        <ModalConfirm
                          title="Confirm cancelling of user invite"
                          triggerButton={
                            <ButtonText variant="warning">
                              Cancel invite
                              <VisuallyHidden>{` for ${invite.email}`}</VisuallyHidden>
                            </ButtonText>
                          }
                          onConfirm={async () => {
                            await handleRemoveDrafterAccess(invite.roleId);
                          }}
                        >
                          <p>
                            Are you sure you want to cancel the drafter invite
                            to this publication for email address{' '}
                            <strong>{invite.email}</strong>?
                          </p>
                        </ModalConfirm>
                      </td>
                    )}
                  </tr>
                ),
              )}
            </tbody>
          </table>
        </div>
      )}

      {drafterAccessMessage}

      {permissions.canUpdateDrafters && (
        <InvitePublicationDrafterForm
          isLoading={isLoadingRoles || isLoadingInvites}
          onInviteDrafter={onInviteDrafter}
        />
      )}

      <h3 className="govuk-!-margin-bottom-0">Publication approvers</h3>

      {hasAnyApprovers && (
        <div className="table-container">
          <table data-testid="publicationApproverRoles">
            <thead>
              <tr>
                <th className={styles.name} scope="col">
                  Name
                </th>
                <th scope="col">Email</th>
              </tr>
            </thead>
            <tbody>
              {orderBy(publicationApprovers, role => role.userName).map(
                role => (
                  <tr key={`${role.id}_${role.role}`}>
                    <td>{role.userName}</td>
                    <td>{role.email}</td>
                  </tr>
                ),
              )}
              {orderBy(publicationApproverInvites, invite => invite.email).map(
                invite => (
                  <tr key={invite.email}>
                    <td />
                    <td>
                      {invite.email}
                      <Tag className="govuk-!-margin-left-3">
                        Pending invite
                      </Tag>
                    </td>
                  </tr>
                ),
              )}
            </tbody>
          </table>
        </div>
      )}

      {approverAccessMessage}
    </>
  );
};

export default PublicationTeamAccessPage;
