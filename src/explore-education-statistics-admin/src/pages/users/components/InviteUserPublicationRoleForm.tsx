import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import React, { useMemo } from 'react';
import {
  InviteUserPublicationRole,
  UserInviteFormValues,
} from '@admin/pages/users/UserInvitePage';
import ButtonText from '@common/components/ButtonText';
import keyBy from 'lodash/keyBy';
import orderBy from 'lodash/orderBy';
import { PublicationSummary } from '@common/services/publicationService';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { useFormContext } from 'react-hook-form';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import publicationRoleDisplayName from '@admin/utils/publicationRoleDisplayName';

interface Props {
  publications?: PublicationSummary[];
  publicationRoles?: PublicationRole[];
}

export default function InviteUserPublicationRoleForm({
  publications,
  publicationRoles,
}: Props) {
  const publicationTitlesById = useMemo(() => {
    return keyBy(publications, publication => publication.id);
  }, [publications]);

  const { getValues, resetField, setError, setValue, watch } =
    useFormContext<UserInviteFormValues>();

  const selectedUserPublicationRoles: InviteUserPublicationRole[] =
    watch('userPublicationRoles') ?? [];

  const handleAddUserPublicationRole = () => {
    const publicationId = getValues('publicationId');
    const publicationRole = getValues('publicationRole');

    if (
      selectedUserPublicationRoles.find(
        role => role.publicationId === publicationId,
      )
    ) {
      setError('publicationId', {
        type: 'custom',
        message: 'You can only add one role for each publication',
      });
      return;
    }

    if (!publicationId || !publicationRole) {
      if (!publicationId) {
        setError('publicationId', {
          type: 'custom',
          message: 'Choose publication to give the user access to',
        });
      }
      if (!publicationRole) {
        setError('publicationRole', {
          type: 'custom',
          message: 'Choose publication role for the user',
        });
      }

      return;
    }

    if (publicationId && publicationRole) {
      setValue('userPublicationRoles', [
        ...selectedUserPublicationRoles,
        { publicationId, publicationRole },
      ]);
      resetField('publicationId');
      resetField('publicationRole');
    }
  };

  const handleRemoveUserPublicationRole = (
    userPublicationRoleToRemove: InviteUserPublicationRole,
  ) => {
    setValue(
      'userPublicationRoles',
      selectedUserPublicationRoles.filter(
        userPublicationRole =>
          userPublicationRoleToRemove !== userPublicationRole,
      ),
    );
  };

  return (
    <FormFieldset
      id="publication-roles"
      legend="Publication roles"
      legendSize="m"
      hint="The user's publication roles within the service."
    >
      <FormFieldSelect<UserInviteFormValues>
        label="Publication"
        name="publicationId"
        placeholder="Choose publication"
        options={publications?.map(publication => ({
          label: publication.title,
          value: publication.id,
        }))}
      />
      <FormFieldSelect<UserInviteFormValues>
        label="Publication role"
        name="publicationRole"
        placeholder="Choose publication role"
        options={publicationRoles?.map(role => ({
          // Temporarily transforming the displayed role name whilst we have the temporary 'Allower'
          // publication role. Once the new 'Approver' role is introduced in STEP 9 (EES-6196) of the permissions
          // rework, this can be reverted to display the role without transformation.
          label: publicationRoleDisplayName(role),
          value: role,
        }))}
      />
      <Button
        type="button"
        className="govuk-!-margin-top-6"
        onClick={handleAddUserPublicationRole}
      >
        Add publication role
      </Button>

      {selectedUserPublicationRoles.length === 0 ? (
        <p>No user publication roles added</p>
      ) : (
        <table data-testid="publication-role-table">
          <thead>
            <tr>
              <th scope="col">Publication Title</th>
              <th scope="col">Publication Role</th>
              <th scope="col">Options</th>
            </tr>
          </thead>
          <tbody>
            {orderBy(
              selectedUserPublicationRoles,
              userPublicationRole =>
                publicationTitlesById[userPublicationRole.publicationId].title,
            ).map(userPublicationRole => (
              <tr
                key={`${userPublicationRole.publicationId}_${userPublicationRole.publicationRole}`}
              >
                <td>
                  {
                    publicationTitlesById[userPublicationRole.publicationId]
                      .title
                  }
                </td>
                <td>
                  {publicationRoleDisplayName(
                    userPublicationRole.publicationRole,
                  )}
                </td>
                <td>
                  <ButtonText
                    onClick={() => {
                      handleRemoveUserPublicationRole(userPublicationRole);
                    }}
                  >
                    Remove
                  </ButtonText>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </FormFieldset>
  );
}
