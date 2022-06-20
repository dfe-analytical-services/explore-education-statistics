import Button from '@common/components/Button';
import { FormFieldSelect, FormFieldset } from '@common/components/form';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useMemo } from 'react';
import { IdTitlePair } from '@admin/services/types/common';
import { InviteUserReleaseRole } from '@admin/pages/users/UserInvitePage';
import ButtonText from '@common/components/ButtonText';
import { keyBy } from 'lodash';
import orderBy from 'lodash/orderBy';

interface FormValues {
  releaseId: string;
  releaseRole: string;
}

interface Props {
  releases?: IdTitlePair[];
  releaseRoles?: string[];
  userReleaseRoles: InviteUserReleaseRole[];
  onAddUserReleaseRole: (userReleaseRole: InviteUserReleaseRole) => void;
  onRemoveUserReleaseRole: (userReleaseRole: InviteUserReleaseRole) => void;
}

const InviteUserReleaseRoleForm = ({
  releases,
  releaseRoles,
  userReleaseRoles,
  onAddUserReleaseRole,
  onRemoveUserReleaseRole,
}: Props) => {
  const releaseTitlesById = useMemo(() => {
    return keyBy(releases, release => release.id);
  }, [releases]);

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        releaseId: '',
        releaseRole: '',
      }}
      validationSchema={Yup.object<FormValues>({
        releaseId: Yup.string()
          .required('Choose release to give the user access to')
          .test(
            'uniqueReleaseRole',
            'You can only add one role for each release',
            function uniqueReleaseRole(releaseId: string) {
              return !userReleaseRoles.some(
                userReleaseRole => userReleaseRole.releaseId === releaseId,
              );
            },
          ),
        releaseRole: Yup.string().required('Choose release role for the user'),
      })}
      onSubmit={newUserReleaseRole => onAddUserReleaseRole(newUserReleaseRole)}
    >
      {form => {
        return (
          <FormFieldset
            id="release-roles"
            legend="Release roles"
            legendSize="m"
            hint="The user's release roles within the service."
          >
            <FormFieldSelect<FormValues>
              label="Release"
              name="releaseId"
              placeholder="Choose release"
              options={releases?.map(release => ({
                label: release.title,
                value: release.id,
              }))}
            />
            <FormFieldSelect<FormValues>
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
              onClick={async () => {
                await form.submitForm();
              }}
            >
              Add release role
            </Button>

            {userReleaseRoles.length === 0 ? (
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
                    userReleaseRoles,
                    userReleaseRole =>
                      releaseTitlesById[userReleaseRole.releaseId].title,
                  ).map(userReleaseRole => (
                    <tr
                      key={`${userReleaseRole.releaseId}_${userReleaseRole.releaseRole}`}
                    >
                      <td>
                        {releaseTitlesById[userReleaseRole.releaseId].title}
                      </td>
                      <td>{userReleaseRole.releaseRole}</td>
                      <td>
                        <ButtonText
                          onClick={() => {
                            onRemoveUserReleaseRole(userReleaseRole);
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
      }}
    </Formik>
  );
};

export default InviteUserReleaseRoleForm;
