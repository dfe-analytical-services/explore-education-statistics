import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { IdTitlePair } from '@admin/services/types/common';
import {
  InviteUserReleaseRole,
  UserInviteFormValues,
} from '@admin/pages/users/UserInvitePage';
import ButtonText from '@common/components/ButtonText';
import keyBy from 'lodash/keyBy';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';

import { useFormContext } from 'react-hook-form';

interface Props {
  releases?: IdTitlePair[];
  releaseRoles?: string[];
}

export default function InviteUserReleaseRoleForm({
  releases,
  releaseRoles,
}: Props) {
  const releaseTitlesById = useMemo(() => {
    return keyBy(releases, release => release.id);
  }, [releases]);

  const { getValues, resetField, setError, setValue, watch } =
    useFormContext<UserInviteFormValues>();

  const selectedUserReleaseRoles = watch('userReleaseRoles') ?? [];

  const handleAddUserReleaseRole = () => {
    const releaseVersionId = getValues('releaseVersionId');
    const releaseRole = getValues('releaseRole');

    if (
      selectedUserReleaseRoles.find(
        role => role.releaseVersionId === releaseVersionId,
      )
    ) {
      setError('releaseVersionId', {
        type: 'custom',
        message: 'You can only add one role for each release',
      });
      return;
    }

    if (!releaseVersionId || !releaseRole) {
      if (!releaseVersionId) {
        setError('releaseVersionId', {
          type: 'custom',
          message: 'Choose release to give the user access to',
        });
      }
      if (!releaseRole) {
        setError('releaseRole', {
          type: 'custom',
          message: 'Choose release role for the user',
        });
      }

      return;
    }

    if (releaseVersionId && releaseRole) {
      setValue('userReleaseRoles', [
        ...selectedUserReleaseRoles,
        { releaseVersionId, releaseRole },
      ]);
      resetField('releaseVersionId');
      resetField('releaseRole');
    }
  };

  const handleRemoveUserReleaseRole = (
    userReleaseRoleToRemove: InviteUserReleaseRole,
  ) => {
    const current: InviteUserReleaseRole[] =
      getValues('userReleaseRoles') ?? [];

    setValue(
      'userReleaseRoles',
      current.filter(
        userReleaseRole => userReleaseRoleToRemove !== userReleaseRole,
      ),
    );
  };

  return (
    <FormFieldset
      id="release-roles"
      legend="Release roles"
      legendSize="m"
      hint="The user's release roles within the service."
    >
      <FormFieldSelect<UserInviteFormValues>
        label="Release"
        name="releaseVersionId"
        placeholder="Choose release"
        options={releases?.map(release => ({
          label: release.title,
          value: release.id,
        }))}
      />
      <FormFieldSelect<UserInviteFormValues>
        label="Release role"
        name="releaseRole"
        placeholder="Choose release role"
        options={releaseRoles?.map(role => ({
          label: role,
          value: role,
        }))}
      />
      <Button
        type="button"
        className="govuk-!-margin-top-6"
        onClick={handleAddUserReleaseRole}
      >
        Add release role
      </Button>

      {selectedUserReleaseRoles.length === 0 ? (
        <p>No user release roles added</p>
      ) : (
        <table data-testid="release-role-table">
          <thead>
            <tr>
              <th scope="col">Release Title</th>
              <th scope="col">Release Role</th>
              <th scope="col">Options</th>
            </tr>
          </thead>
          <tbody>
            {orderBy(
              selectedUserReleaseRoles,
              userReleaseRole =>
                releaseTitlesById[userReleaseRole.releaseVersionId].title,
            ).map(userReleaseRole => (
              <tr
                key={`${userReleaseRole.releaseVersionId}_${userReleaseRole.releaseRole}`}
              >
                <td>
                  {releaseTitlesById[userReleaseRole.releaseVersionId].title}
                </td>
                <td>{userReleaseRole.releaseRole}</td>
                <td>
                  <ButtonText
                    onClick={() => {
                      handleRemoveUserReleaseRole(userReleaseRole);
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
