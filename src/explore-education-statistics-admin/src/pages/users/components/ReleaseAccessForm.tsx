import userService, {
  User,
  UserReleaseRole,
} from '@admin/services/userService';
import { IdTitlePair } from '@admin/services/types/common';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { mapFieldErrors } from '@common/validation/serverValidations';
import orderBy from 'lodash/orderBy';
import React from 'react';

const addReleaseFormErrorMappings = [
  mapFieldErrors<FormValues>({
    target: 'releaseRole',
    messages: {
      UserAlreadyHasResourceRole: 'The user already has this release role',
    },
  }),
];

interface FormValues {
  releaseVersionId: string;
  releaseRole: string;
}

interface Props {
  releases?: IdTitlePair[];
  releaseRoles?: string[];
  user: User;
  onUpdate: () => void;
}

export default function ReleaseAccessForm({
  releases,
  releaseRoles,
  user,
  onUpdate,
}: Props) {
  const handleAddReleaseRole = async (values: FormValues) => {
    await userService.addUserReleaseRole(user.id, values);
    onUpdate();
  };

  const handleRemoveReleaseAccess = async (
    userReleaseRole: UserReleaseRole,
  ) => {
    await userService.removeUserReleaseRole(userReleaseRole.id);
    onUpdate();
  };

  return (
    <FormProvider
      errorMappings={addReleaseFormErrorMappings}
      initialValues={{
        releaseVersionId: orderBy(releases, release => release)?.[0]?.id ?? '',
        releaseRole:
          orderBy(releaseRoles, releaseRole => releaseRole)?.[0] ?? '',
      }}
    >
      {({ formState }) => {
        return (
          <Form id={`${user.id}-releaseRole`} onSubmit={handleAddReleaseRole}>
            <FormFieldset
              id="role"
              legend="Release access"
              legendSize="m"
              hint="The releases a user can access within the service."
            >
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-half">
                  <FormFieldSelect<FormValues>
                    label="Release"
                    name="releaseVersionId"
                    options={releases?.map(release => ({
                      label: release.title,
                      value: release.id,
                    }))}
                  />
                </div>

                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSelect<FormValues>
                    label="Release role"
                    name="releaseRole"
                    options={releaseRoles?.map(role => ({
                      label: role,
                      value: role,
                    }))}
                  />
                </div>
                <div className="govuk-grid-column-one-quarter">
                  {user && (
                    <Button
                      type="submit"
                      disabled={formState.isSubmitting}
                      className="govuk-!-margin-top-6"
                    >
                      Add release access
                    </Button>
                  )}
                </div>
              </div>
            </FormFieldset>
            <table className="govuk-table" data-testid="releaseAccessTable">
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th scope="col" className="govuk-table__header">
                    Publication
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Release
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Role
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody className="govuk-table__body">
                {user.userReleaseRoles.map(userReleaseRole => (
                  <tr className="govuk-table__row" key={userReleaseRole.id}>
                    <td className="govuk-table__cell">
                      {userReleaseRole.publication}
                    </td>
                    <td className="govuk-table__cell">
                      {userReleaseRole.release}
                    </td>
                    <td className="govuk-table__cell">
                      {userReleaseRole.role}
                    </td>
                    <td className="govuk-table__cell">
                      <ButtonText
                        onClick={() =>
                          handleRemoveReleaseAccess(userReleaseRole)
                        }
                      >
                        Remove
                      </ButtonText>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Form>
        );
      }}
    </FormProvider>
  );
}
