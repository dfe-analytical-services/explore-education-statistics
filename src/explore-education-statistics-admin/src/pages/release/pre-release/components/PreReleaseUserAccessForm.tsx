import preReleaseUserService from '@admin/services/preReleaseUserService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldTextArea } from '@common/components/form';
import Gate from '@common/components/Gate';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

interface FormValues {
  emails: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'emails',
    messages: {
      INVALID_EMAIL_ADDRESS:
        'Please enter valid @education.gov.uk email addresses',
    },
  }),
];

interface Props {
  releaseId: string;
  isReleaseLive?: boolean;
}

const formId = 'preReleaseUserAccessForm';
const inviteLimit = 50;

const PreReleaseUserAccessForm = ({
  releaseId,
  isReleaseLive = false,
}: Props) => {
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
    const newUsers = await preReleaseUserService.inviteUsers(
      releaseId,
      values.emails,
    );

    setUsers({
      value: [...users, ...newUsers],
    });

    actions.resetForm();
  }, errorMappings);

  if (error) {
    return <WarningMessage>Could not load pre-release users</WarningMessage>;
  }

  return (
    <LoadingSpinner loading={isLoading}>
      <Gate
        condition={!isReleaseLive}
        fallback={
          <WarningMessage>
            This release has been published and can no longer be updated.
          </WarningMessage>
        }
      >
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            emails: '',
          }}
          validationSchema={Yup.object<FormValues>({
            emails: Yup.string()
              .trim()
              .required('Please enter 1 or more email addresses')
              .test({
                name: 'number of lines',
                message: `Please enter between 1 and ${inviteLimit} lines`,
                test: (value: string) => {
                  if (value) {
                    const numOfLines = (value.match(/\n/g) || '').length + 1;
                    return numOfLines <= inviteLimit;
                  }
                  return true;
                },
              })
              .test({
                name: 'email format',
                message: 'Please enter valid @education.gov.uk email addresses',
                test: (value: string) => {
                  if (value) {
                    if (
                      value ===
                      'simulate-delivered@notifications.service.gov.uk'
                    ) {
                      return true;
                    }
                    const emails = value.split(/\r\n|\r|\n/);
                    return emails.every(email => {
                      const emailSegments = email.split('@');
                      return (
                        emailSegments.length === 2 &&
                        emailSegments[1] === 'education.gov.uk'
                      );
                    });
                  }
                  return false;
                },
              }),
          })}
          onSubmit={handleSubmit}
        >
          {form => (
            <Form id={formId}>
              <FormFieldTextArea<FormValues>
                label="Invite new users by email"
                name="emails"
                className="govuk-!-width-one-third"
                hint={`Invite up to ${inviteLimit} email addresses at a time. Enter each email address on a new line.`}
                rows={15}
              />

              <ButtonGroup>
                <Button
                  type="submit"
                  disabled={form.isSubmitting || isRemoving}
                >
                  {form.isSubmitting
                    ? 'Inviting new users'
                    : 'Invite new users'}
                </Button>
              </ButtonGroup>
            </Form>
          )}
        </Formik>
      </Gate>

      {users.length > 0 ? (
        <>
          <InsetText>
            <p>
              These people will have access to a preview of the release 24 hours
              before the scheduled publish date.
            </p>
          </InsetText>

          <table>
            <thead>
              <tr>
                <th>User email</th>
                {!isReleaseLive && <th />}
              </tr>
            </thead>
            <tbody>
              {users.map(user => (
                <tr key={user.email}>
                  <td>{user.email}</td>

                  {!isReleaseLive && (
                    <td className="dfe-align--right">
                      <ButtonText
                        disabled={isRemoving}
                        onClick={async () => {
                          toggleRemoving.on();

                          await preReleaseUserService.removeUser(
                            releaseId,
                            user.email,
                          );

                          setUsers({
                            value: users.filter(u => u.email !== user.email),
                          });
                          toggleRemoving.off();
                        }}
                      >
                        Remove
                      </ButtonText>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </>
      ) : (
        <InsetText>No pre-release users have been invited.</InsetText>
      )}
    </LoadingSpinner>
  );
};

export default PreReleaseUserAccessForm;
