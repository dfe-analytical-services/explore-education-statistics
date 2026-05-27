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
import {
  UserPreReleaseRole,
  UserWithRoles,
} from '@admin/services/types/userWithRoles';
import preReleaseUsersService from '@admin/services/user-management/preReleaseUsersService';

const addPreReleaseFormErrorMappings = [
  mapFieldErrors<FormValues>({
    target: 'releaseId',
    messages: {
      UserAlreadyHasResourceRole: 'The user already has this pre-release role',
    },
  }),
];

interface FormValues {
  releaseId: string;
}

interface Props {
  releases?: IdTitlePair[];
  user: UserWithRoles;
  onUpdate: () => void;
}

export default function PreReleaseAccessForm({
  releases,
  user,
  onUpdate,
}: Props) {
  const handleAddPreReleaseRole = async (values: FormValues) => {
    await preReleaseUsersService.grantPreReleaseAccess(
      user.id,
      values.releaseId,
    );
    onUpdate();
  };

  const handleRemovePreReleaseAccess = async (
    userPreReleaseRole: UserPreReleaseRole,
  ) => {
    await preReleaseUsersService.revokePreReleaseAccessById(
      userPreReleaseRole.id,
    );
    onUpdate();
  };

  return (
    <FormProvider
      errorMappings={addPreReleaseFormErrorMappings}
      initialValues={{
        releaseId: orderBy(releases, release => release)?.[0]?.id ?? '',
      }}
    >
      {({ formState }) => {
        return (
          <Form
            id={`${user.id}-preReleaseRole`}
            onSubmit={handleAddPreReleaseRole}
          >
            <FormFieldset
              id="role"
              legend="Pre-release access"
              legendSize="m"
              hint="The releases a user has pre-release access to within the service."
            >
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-half">
                  <FormFieldSelect<FormValues>
                    label="Release"
                    name="releaseId"
                    options={releases?.map(release => ({
                      label: release.title,
                      value: release.id,
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
                      Add pre-release access
                    </Button>
                  )}
                </div>
              </div>
            </FormFieldset>
            <table className="govuk-table" data-testid="preReleaseAccessTable">
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th scope="col" className="govuk-table__header">
                    Publication
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Release
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody className="govuk-table__body">
                {user.userPreReleaseRoles.map(userPreReleaseRole => (
                  <tr className="govuk-table__row" key={userPreReleaseRole.id}>
                    <td className="govuk-table__cell">
                      {userPreReleaseRole.publication}
                    </td>
                    <td className="govuk-table__cell">
                      {userPreReleaseRole.release}
                    </td>
                    <td className="govuk-table__cell">
                      <ButtonText
                        onClick={() =>
                          handleRemovePreReleaseAccess(userPreReleaseRole)
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
