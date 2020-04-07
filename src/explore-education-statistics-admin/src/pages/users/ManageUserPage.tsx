import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import { FormFieldset, FormFieldSelect, Formik } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Form from '@common/components/form/Form';
import Page from '@admin/components/Page';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import userService from '@admin/services/users/service';
import { UserUpdate } from '@admin/services/users/types';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import Yup from '@common/validation/yup';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';

const errorCodeMappings = [
  errorCodeToFieldError('USER_DOES_NOT_EXIST', 'userId', 'User does not exist'),
  errorCodeToFieldError(
    'ROLE_DOES_NOT_EXIST',
    'selectedRoleId',
    'Role does not exist',
  ),
];

interface FormValues {
  userId: string;
  selectedRoleId: string;
}

const ManageUserPage = ({
  match,
  history,
}: RouteComponentProps<{ userId: string }>) => {
  const { userId } = match.params;
  const formId = userId;

  const { value: user, isLoading, error } = useAsyncRetry(() =>
    userService.getUser(userId),
  );

  const { value: roles } = useAsyncRetry(() => userService.getRoles());

  const cancelHandler = () => history.push('/administration/users');

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    const submission: UserUpdate = {
      id: userId,
      roleId: values.selectedRoleId,
    };

    await userService.updateUser(submission);

    history.push(`/administration/users`);
  }, errorCodeMappings);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Users', link: '/administration/users' },
        { name: 'Manage user' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Manage user</span>
        {user?.name}
      </h1>

      <LoadingSpinner loading={isLoading} text="Loading user">
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            userId: userId ?? '',
            selectedRoleId: user?.role ?? '',
          }}
          validationSchema={Yup.object({
            selectedRoleId: Yup.string().required('Choose role for the user'),
          })}
          onSubmit={handleSubmit}
          render={_ => {
            return (
              <Form id={formId}>
                <FormFieldset
                  id={`${formId}-details`}
                  legend="Detals"
                  legendSize="m"
                >
                  <SummaryList>
                    <SummaryListItem term="Name">{user?.name}</SummaryListItem>
                    <SummaryListItem term="Email">
                      {' '}
                      {user?.email}
                    </SummaryListItem>
                    <SummaryListItem term="Phone">-</SummaryListItem>
                  </SummaryList>
                </FormFieldset>
                <FormFieldset
                  id={`${formId}-role`}
                  legend="Role"
                  legendSize="m"
                  hint="The users role within the service."
                >
                  <FormFieldSelect
                    id={`${formId}-selectedRoleId`}
                    label="Role"
                    name="selectedRoleId"
                    options={roles?.map(role => ({
                      label: role.name,
                      value: role.id,
                    }))}
                  />
                </FormFieldset>

                {/* <FormFieldCheckboxGroup
                id={`${formId}-releaseaccess`}
                legend="Release access"
                legendSize="m"
                hint="The releases a user has access to (BAU Users have access to all releases)."
                name="releaseAccess"
                options={[]}
              /> */}

                <Button type="submit" className="govuk-!-margin-top-6">
                  Save
                </Button>
                <div className="govuk-!-margin-top-6">
                  <ButtonText onClick={cancelHandler}>Cancel</ButtonText>
                </div>
              </Form>
            );
          }}
        />
      </LoadingSpinner>
    </Page>
  );
};

export default ManageUserPage;
