import Button from '@common/components/Button';
import { FormFieldSelect, FormFieldset } from '@common/components/form';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useMemo } from 'react';
import { InviteUserPublicationRole } from '@admin/pages/users/UserInvitePage';
import ButtonText from '@common/components/ButtonText';
import { keyBy } from 'lodash';
import orderBy from 'lodash/orderBy';
import { PublicationSummary } from '@common/services/publicationService';

interface FormValues {
  publicationId: string;
  publicationRole: string;
}

interface Props {
  publications?: PublicationSummary[];
  publicationRoles?: string[];
  userPublicationRoles: InviteUserPublicationRole[];
  onAddUserPublicationRole: (
    userPublicationRole: InviteUserPublicationRole,
  ) => void;
  onRemoveUserPublicationRole: (
    userPublicationRole: InviteUserPublicationRole,
  ) => void;
}

const InviteUserPublicationRoleForm = ({
  publications,
  publicationRoles,
  userPublicationRoles,
  onAddUserPublicationRole,
  onRemoveUserPublicationRole,
}: Props) => {
  const publicationTitlesById = useMemo(() => {
    return keyBy(publications, publication => publication.id);
  }, [publications]);

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        publicationId: '',
        publicationRole: '',
      }}
      validationSchema={Yup.object<FormValues>({
        publicationId: Yup.string()
          .required('Choose publication to give the user access to')
          .test(
            'uniquePublicationRole',
            'You can only add one role for each publication',
            function uniquePublicationRole(publicationId: string) {
              return !userPublicationRoles.some(
                userPublicationRole =>
                  userPublicationRole.publicationId === publicationId,
              );
            },
          ),
        publicationRole: Yup.string().required(
          'Choose publication role for the user',
        ),
      })}
      onSubmit={newUserPublicationRole =>
        onAddUserPublicationRole(newUserPublicationRole)
      }
    >
      {form => {
        return (
          <FormFieldset
            id="publication-roles"
            legend="Publication roles"
            legendSize="m"
            hint="The user's publication roles within the service."
          >
            <FormFieldSelect<FormValues>
              label="Publication"
              name="publicationId"
              placeholder="Choose publication"
              options={publications?.map(publication => ({
                label: publication.title,
                value: publication.id,
              }))}
            />
            <FormFieldSelect<FormValues>
              label="Publication role"
              name="publicationRole"
              placeholder="Choose publication role"
              options={publicationRoles?.map(role => ({
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
              Add publication role
            </Button>

            {userPublicationRoles.length === 0 ? (
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
                    userPublicationRoles,
                    userPublicationRole =>
                      publicationTitlesById[userPublicationRole.publicationId]
                        .title,
                  ).map(userPublicationRole => (
                    <tr
                      key={`${userPublicationRole.publicationId}_${userPublicationRole.publicationRole}`}
                    >
                      <td>
                        {
                          publicationTitlesById[
                            userPublicationRole.publicationId
                          ].title
                        }
                      </td>
                      <td>{userPublicationRole.publicationRole}</td>
                      <td>
                        <ButtonText
                          onClick={() => {
                            onRemoveUserPublicationRole(userPublicationRole);
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

export default InviteUserPublicationRoleForm;
