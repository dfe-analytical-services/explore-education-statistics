import useFormSubmit from '@admin/hooks/useFormSubmit';
import preReleaseUserService from '@admin/services/preReleaseUserService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

interface FormValues {
  email: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'email',
    messages: {
      USER_ALREADY_EXISTS: 'User already exists',
    },
  }),
];

interface Props {
  releaseId: string;
}

const formId = 'preReleaseUserAccessForm';

const PreReleaseUserAccessForm = ({ releaseId }: Props) => {
  const [isRemoving, toggleRemoving] = useToggle(false);

  const {
    value: users = [],
    isLoading,
    error,
    setState: setUsers,
  } = useAsyncRetry(() => preReleaseUserService.getUsers(releaseId), [
    releaseId,
  ]);

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    const nextUsers = await preReleaseUserService.inviteUser(
      releaseId,
      values.email,
    );

    setUsers({ value: nextUsers });

    actions.resetForm();
  }, errorMappings);

  if (error) {
    return <WarningMessage>Could not load pre-release users</WarningMessage>;
  }

  return (
    <LoadingSpinner loading={isLoading}>
      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          email: '',
        }}
        validationSchema={Yup.object<FormValues>({
          email: Yup.string()
            .required('Enter an email address')
            .email('Enter a valid email address'),
        })}
        onSubmit={handleSubmit}
      >
        {form => (
          <Form id={formId}>
            <FormFieldTextInput<FormValues>
              id={`${formId}-email`}
              label="Invite new user by email"
              name="email"
              className="govuk-!-width-one-third"
            />

            <ButtonGroup>
              <Button type="submit" disabled={form.isSubmitting || isRemoving}>
                {form.isSubmitting ? 'Inviting new user' : 'Invite new user'}
              </Button>
            </ButtonGroup>
          </Form>
        )}
      </Formik>

      {users.length > 0 ? (
        <table>
          <thead>
            <tr>
              <th>User email</th>
              <th>Invited?</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {users.map(user => (
              <tr key={user.email}>
                <td>{user.email}</td>
                <td>{user.invited ? 'Yes' : 'No'}</td>
                <td className="dfe-align--right">
                  <ButtonText
                    disabled={isRemoving}
                    onClick={async () => {
                      toggleRemoving.on();

                      const nextUsers = await preReleaseUserService.removeUser(
                        releaseId,
                        user.email,
                      );

                      setUsers({ value: nextUsers });
                      toggleRemoving.off();
                    }}
                  >
                    Remove
                  </ButtonText>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p className="govuk-inset-text">
          No pre-release users have been invited yet.
        </p>
      )}
    </LoadingSpinner>
  );
};

export default PreReleaseUserAccessForm;
