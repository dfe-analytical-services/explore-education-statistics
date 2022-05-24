import { Role, User } from '@admin/services/userService';
import Button from '@common/components/Button';
import { FormFieldSelect, FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/validation/yup';
import { Formik, FormikHelpers } from 'formik';
import React from 'react';

export interface UpdateRoleFormValues {
  selectedRoleId: string;
}

interface Props {
  roles?: Role[];
  user: User;
  onSubmit: (
    values: UpdateRoleFormValues,
    actions: FormikHelpers<UpdateRoleFormValues>,
  ) => void;
}

const RoleForm = ({ roles, user, onSubmit }: Props) => {
  return (
    <Formik<UpdateRoleFormValues>
      enableReinitialize
      initialValues={{
        selectedRoleId: user.role ?? '',
      }}
      validationSchema={Yup.object<UpdateRoleFormValues>({
        selectedRoleId: Yup.string().required('Choose role for the user'),
      })}
      onSubmit={onSubmit}
    >
      {() => {
        return (
          <Form id={user.id}>
            <FormFieldset id="user" legend="Details" legendSize="m">
              <SummaryList>
                <SummaryListItem term="Name">{user.name}</SummaryListItem>
                <SummaryListItem term="Email">
                  <a href={`mailto:${user.email}`}>{user.email}</a>
                </SummaryListItem>
                <SummaryListItem term="Phone">-</SummaryListItem>
              </SummaryList>
            </FormFieldset>
            <FormFieldset
              id="role"
              legend="Role"
              legendSize="m"
              hint="The users's role within the service."
            >
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSelect<UpdateRoleFormValues>
                    label="Role"
                    name="selectedRoleId"
                    options={roles?.map(role => ({
                      label: role.name,
                      value: role.id,
                    }))}
                    placeholder="Choose role"
                  />
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <Button type="submit" className="govuk-!-margin-top-6">
                    Update role
                  </Button>
                </div>
              </div>
            </FormFieldset>
          </Form>
        );
      }}
    </Formik>
  );
};

export default RoleForm;
