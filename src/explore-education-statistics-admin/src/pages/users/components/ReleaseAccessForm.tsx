import { User, UserReleaseRole } from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldSelect, FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import { IdTitlePair } from 'src/services/types/common';
import { Formik, FormikHelpers } from 'formik';
import orderBy from 'lodash/orderBy';
import React from 'react';

export interface AddReleaseRoleFormValues {
  selectedReleaseId: string;
  selectedReleaseRole: string;
}

interface Props {
  releases?: IdTitlePair[];
  releaseRoles?: string[];
  user: User;
  onRemove: (userReleaseRole: UserReleaseRole) => void;
  onSubmit: (
    values: AddReleaseRoleFormValues,
    actions: FormikHelpers<AddReleaseRoleFormValues>,
  ) => void;
}

const ReleaseAccessForm = ({
  releases,
  releaseRoles,
  user,
  onRemove,
  onSubmit,
}: Props) => {
  return (
    <Formik<AddReleaseRoleFormValues>
      initialValues={{
        selectedReleaseId: orderBy(releases, release => release)?.[0]?.id ?? '',
        selectedReleaseRole:
          orderBy(releaseRoles, releaseRole => releaseRole)?.[0] ?? '',
      }}
      enableReinitialize
      onSubmit={onSubmit}
    >
      {form => {
        return (
          <Form id={`${user.id}-releaseRole`}>
            <FormFieldset
              id="role"
              legend="Release access"
              legendSize="m"
              hint="The releases a user can access within the service."
            >
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-half">
                  <FormFieldSelect<AddReleaseRoleFormValues>
                    label="Release"
                    name="selectedReleaseId"
                    options={releases?.map(release => ({
                      label: release.title,
                      value: release.id,
                    }))}
                  />
                </div>

                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSelect<AddReleaseRoleFormValues>
                    label="Release role"
                    name="selectedReleaseRole"
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
                      disabled={form.isSubmitting}
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
                      <ButtonText onClick={() => onRemove(userReleaseRole)}>
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
    </Formik>
  );
};

export default ReleaseAccessForm;
