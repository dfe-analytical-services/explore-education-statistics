import { User, UserPublicationRole } from '@admin/services/userService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldSelect, FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import { IdTitlePair } from 'src/services/types/common';
import { Formik, FormikHelpers } from 'formik';
import orderBy from 'lodash/orderBy';
import React from 'react';

export interface AddPublicationRoleFormValues {
  selectedPublicationId: string;
  selectedPublicationRole: string;
}

interface Props {
  publications?: IdTitlePair[];
  publicationRoles?: string[];
  user: User;
  onRemove: (userPublicationRole: UserPublicationRole) => void;
  onSubmit: (
    values: AddPublicationRoleFormValues,
    actions: FormikHelpers<AddPublicationRoleFormValues>,
  ) => void;
}

const PublicationAccessForm = ({
  publications,
  publicationRoles,
  user,
  onRemove,
  onSubmit,
}: Props) => {
  return (
    <Formik<AddPublicationRoleFormValues>
      initialValues={{
        selectedPublicationId:
          orderBy(publications, publication => publication)?.[0]?.id ?? '',
        selectedPublicationRole:
          orderBy(publicationRoles, publicationRole => publicationRole)?.[0] ??
          '',
      }}
      enableReinitialize
      onSubmit={onSubmit}
    >
      {form => {
        return (
          <Form id={`${user.id}-publicationRole`}>
            <FormFieldset
              id="role"
              legend="Publication access"
              legendSize="m"
              hint="The publications a user can access within the service."
            >
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-half">
                  <FormFieldSelect<AddPublicationRoleFormValues>
                    label="Publication"
                    name="selectedPublicationId"
                    options={publications?.map(publication => ({
                      label: publication.title,
                      value: publication.id,
                    }))}
                  />
                </div>

                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSelect<AddPublicationRoleFormValues>
                    label="Publication role"
                    name="selectedPublicationRole"
                    options={publicationRoles?.map(role => ({
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
                        {userPublicationRole.role}
                      </td>
                      <td className="govuk-table__cell">
                        <ButtonText
                          onClick={() => onRemove(userPublicationRole)}
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
    </Formik>
  );
};

export default PublicationAccessForm;
