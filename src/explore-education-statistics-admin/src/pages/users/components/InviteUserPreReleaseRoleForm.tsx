import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { IdTitlePair } from '@admin/services/types/common';
import {
  InviteUserPreReleaseRole,
  UserInviteFormValues,
} from '@admin/pages/users/UserInvitePage';
import ButtonText from '@common/components/ButtonText';
import keyBy from 'lodash/keyBy';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';

import { useFormContext } from 'react-hook-form';

interface Props {
  releases?: IdTitlePair[];
}

export default function InviteUserPreReleaseRoleForm({ releases }: Props) {
  const releaseTitlesById = useMemo(() => {
    return keyBy(releases, release => release.id);
  }, [releases]);

  const { getValues, resetField, setError, setValue, watch } =
    useFormContext<UserInviteFormValues>();

  const selectedUserPreReleaseRoles = watch('userPreReleaseRoles') ?? [];

  const handleAddUserPreReleaseRole = () => {
    const releaseId = getValues('releaseId');

    if (
      selectedUserPreReleaseRoles.find(role => role.releaseId === releaseId)
    ) {
      setError('releaseId', {
        type: 'custom',
        message: 'You can only add one pre-release role for each release',
      });
      return;
    }

    if (!releaseId) {
      setError('releaseId', {
        type: 'custom',
        message: 'Choose release to give the user access to',
      });

      return;
    }

    setValue('userPreReleaseRoles', [
      ...selectedUserPreReleaseRoles,
      { releaseId },
    ]);
    resetField('releaseId');
  };

  const handleRemoveUserPreReleaseRole = (
    userPreReleaseRoleToRemove: InviteUserPreReleaseRole,
  ) => {
    const current: InviteUserPreReleaseRole[] =
      getValues('userPreReleaseRoles') ?? [];

    setValue(
      'userPreReleaseRoles',
      current.filter(
        userPreReleaseRole => userPreReleaseRoleToRemove !== userPreReleaseRole,
      ),
    );
  };

  return (
    <FormFieldset
      id="pre-release-roles"
      legend="Pre-release roles"
      legendSize="m"
      hint="The user's pre-release roles within the service."
    >
      <FormFieldSelect<UserInviteFormValues>
        label="Release"
        name="releaseId"
        placeholder="Choose release"
        options={releases?.map(release => ({
          label: release.title,
          value: release.id,
        }))}
      />
      <Button
        type="button"
        className="govuk-!-margin-top-6"
        onClick={handleAddUserPreReleaseRole}
      >
        Add pre-release role
      </Button>

      {selectedUserPreReleaseRoles.length === 0 ? (
        <p>No user pre-release roles added</p>
      ) : (
        <table data-testid="pre-release-role-table">
          <thead>
            <tr>
              <th scope="col">Release Title</th>
              <th scope="col">Options</th>
            </tr>
          </thead>
          <tbody>
            {orderBy(
              selectedUserPreReleaseRoles,
              userPreReleaseRole =>
                releaseTitlesById[userPreReleaseRole.releaseId].title,
            ).map(userPreReleaseRole => (
              <tr key={userPreReleaseRole.releaseId}>
                <td>{releaseTitlesById[userPreReleaseRole.releaseId].title}</td>
                <td>
                  <ButtonText
                    onClick={() => {
                      handleRemoveUserPreReleaseRole(userPreReleaseRole);
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
