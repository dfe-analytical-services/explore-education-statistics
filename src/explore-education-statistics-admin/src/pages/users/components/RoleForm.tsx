import userService, { Role, User } from '@admin/services/userService';
import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

const updateRoleFormErrorMappings = [
  mapFieldErrors<FormValues>({
    target: 'roleId',
    messages: {
      RoleDoesNotExist: 'Role does not exist',
    },
  }),
];

interface FormValues {
  roleId: string;
}

interface Props {
  roles?: Role[];
  user: User;
  onUpdate: () => void;
}

const RoleForm = ({ roles, user, onUpdate }: Props) => {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      roleId: Yup.string().required('Choose role for the user'),
    });
  }, []);

  const handleSubmit = async (values: FormValues) => {
    await userService.updateUser(user.id, values);
    onUpdate();
  };

  return (
    <FormProvider
      enableReinitialize
      errorMappings={updateRoleFormErrorMappings}
      initialValues={{
        roleId: user.role ?? '',
      }}
      validationSchema={validationSchema}
    >
      <RHFForm id={user.id} onSubmit={handleSubmit}>
        <FormFieldset
          id="role"
          legend="Role"
          legendSize="m"
          hint="The users' role within the service."
        >
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <RHFFormFieldSelect<FormValues>
                label="Role"
                name="roleId"
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
      </RHFForm>
    </FormProvider>
  );
};

export default RoleForm;
