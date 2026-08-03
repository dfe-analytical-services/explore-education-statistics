import usersService from '@admin/services/user-management/usersService';
import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import React from 'react';
import { UserWithRoles } from '@admin/services/types/userWithRoles';
import { GlobalRole } from '@admin/services/types/GlobalRole';
import { mapFieldErrors } from '@common/validation/serverValidations';

interface FormValues {
  isSuperUser: boolean;
}

interface FormValues {
  targetGlobalRole: GlobalRole;
}

interface Props {
  user: UserWithRoles;
  onUpdate: () => void;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'isSuperUser',
    messages: {
      UserIsAlreadyBauUser: 'User is already a Super User',
      UserIsAlreadyStandardUser: 'User is already a Standard User',
    },
  }),
];

const RoleForm = ({ user, onUpdate }: Props) => {
  const handleSubmit = async (values: FormValues) => {
    await usersService.updateUserGlobalRole(user.id, {
      targetGlobalRole: values.isSuperUser
        ? GlobalRole.BauUser
        : GlobalRole.StandardUser,
    });

    onUpdate();
  };

  return (
    <FormProvider
      errorMappings={errorMappings}
      enableReinitialize
      initialValues={{
        isSuperUser: user.globalRole === GlobalRole.BauUser,
      }}
    >
      <Form id={user.id} onSubmit={handleSubmit}>
        <FormFieldset
          id="super-user"
          legend="Access level"
          legendSize="m"
          hint="Super Users have elevated permissions."
        >
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <FormFieldCheckbox<FormValues>
                name="isSuperUser"
                label="Super User"
              />
            </div>

            <div className="govuk-grid-column-one-quarter">
              <Button type="submit">Update access</Button>
            </div>
          </div>
        </FormFieldset>
      </Form>
    </FormProvider>
  );
};

export default RoleForm;
