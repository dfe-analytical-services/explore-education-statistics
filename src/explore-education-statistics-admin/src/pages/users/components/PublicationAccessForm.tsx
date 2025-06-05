import userService, {
  User,
  UserPublicationRole,
} from '@admin/services/userService';
import { IdTitlePair } from '@admin/services/types/common';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import { mapFieldErrors } from '@common/validation/serverValidations';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import publicationRoleDisplayName from '@admin/utils/publicationRoleDisplayName';

const addPublicationFormErrorMappings = [
  mapFieldErrors<FormValues>({
    target: 'publicationRole',
    messages: {
      UserAlreadyHasResourceRole: 'The user already has this publication role',
    },
  }),
];

interface FormValues {
  publicationId: string;
  publicationRole: PublicationRole;
}

interface Props {
  publications?: IdTitlePair[];
  publicationRoles?: PublicationRole[];
  user: User;
  onUpdate: () => void;
}

export default function PublicationAccessForm({
  publications,
  publicationRoles,
  user,
  onUpdate,
}: Props) {
  const handleAddPublicationRole = async (values: FormValues) => {
    await userService.addUserPublicationRole(user.id, values);
    onUpdate();
  };

  const handleRemovePublicationAccess = async (
    userPublicationRole: UserPublicationRole,
  ) => {
    await userService.removeUserPublicationRole(user.id, userPublicationRole);
    onUpdate();
  };

  return (
    <FormProvider
      errorMappings={addPublicationFormErrorMappings}
      initialValues={{
        publicationId:
          orderBy(publications, publication => publication)?.[0]?.id ?? '',
        publicationRole:
          orderBy(publicationRoles, publicationRole => publicationRole)?.[0] ??
          '',
      }}
    >
      {({ formState }) => {
        return (
          <Form
            id={`${user.id}-publicationRole`}
            onSubmit={handleAddPublicationRole}
          >
            <FormFieldset
              id="role"
              legend="Publication access"
              legendSize="m"
              hint="The publications a user can access within the service."
            >
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-half">
                  <FormFieldSelect<FormValues>
                    label="Publication"
                    name="publicationId"
                    options={publications?.map(publication => ({
                      label: publication.title,
                      value: publication.id,
                    }))}
                  />
                </div>

                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSelect<FormValues>
                    label="Publication role"
                    name="publicationRole"
                    options={publicationRoles?.map(role => ({
                      // Temporarily transforming the displayed role name whilst we have the temporary 'Allower'
                      // publication role. Once the new 'Approver' role is introduced in STEP 9 (EES-6196) of the permissions
                      // rework, this can be reverted to display the role without transformation.
                      label: publicationRoleDisplayName(role),
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
                      Add publication access
                    </Button>
                  )}
                </div>
              </div>
            </FormFieldset>
            <table className="govuk-table" data-testid="publicationAccessTable">
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th scope="col" className="govuk-table__header">
                    Publication
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
                {user.userPublicationRoles &&
                  user.userPublicationRoles.map(userPublicationRole => (
                    <tr
                      className="govuk-table__row"
                      key={userPublicationRole.id}
                    >
                      <td className="govuk-table__cell">
                        {userPublicationRole.publication}
                      </td>
                      <td className="govuk-table__cell">
                        {
                          // Temporarily transforming the displayed role name whilst we have the temporary 'Allower'
                          // publication role. Once the new 'Approver' role is introduced in STEP 9 (EES-6196) of the permissions
                          // rework, this can be reverted to display the role without transformation.
                          publicationRoleDisplayName(userPublicationRole.role)
                        }
                      </td>
                      <td className="govuk-table__cell">
                        <ButtonText
                          onClick={() =>
                            handleRemovePublicationAccess(userPublicationRole)
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
