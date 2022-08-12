import PreReleaseInvitePlanModal from '@admin/pages/release/pre-release/components/PreReleaseInvitePlanModal';
import preReleaseUserService, {
  PreReleaseInvitePlan,
} from '@admin/services/preReleaseUserService';
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
import React, { useCallback, useState } from 'react';

interface FormValues {
  emails: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'emails',
    messages: {
      InvalidEmailAddress: 'Enter valid email addresses',
      NoInvitableEmails:
        'All of the email addresses have already been invited or accepted',
    },
  }),
];

interface Props {
  releaseId: string;
  isReleaseApproved?: boolean;
  isReleaseLive?: boolean;
}

const formId = 'preReleaseUserAccessForm';
const inviteLimit = 50;

const PreReleaseUserAccessForm = ({
  releaseId,
  isReleaseApproved = false,
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

  const [invitePlan, setInvitePlan] = useState<PreReleaseInvitePlan>();

  const isValidEmail = (input: string) => {
    return /^[A-Z0-9.'_%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$/i.test(input);
  };

  const splitAndTrimLines = (input: string) =>
    input
      .split(/\r\n|\r|\n/)
      .map(line => line.trim())
      .filter(line => line);

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    setInvitePlan(
      await preReleaseUserService.getInvitePlan(
        releaseId,
        splitAndTrimLines(values.emails),
      ),
    );
  }, errorMappings);

  const handleModalSubmit = useFormSubmit<FormValues>(
    async (values, actions) => {
      const newUsers = await preReleaseUserService.inviteUsers(
        releaseId,
        splitAndTrimLines(values.emails),
      );

      setUsers({
        value: [...users, ...newUsers],
      });

      actions.resetForm();
    },
    errorMappings,
  );

  const handleModalCancel = useCallback(() => {
    setInvitePlan(undefined);
  }, []);

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
              .required('Enter 1 or more email addresses')
              .test({
                name: 'number-of-lines',
                message: `Enter between 1 and ${inviteLimit} lines of email addresses`,
                test: (value: string) => {
                  if (value) {
                    const numOfLines = (value.match(/^\s*\S/gm) || '').length;
                    return numOfLines <= inviteLimit;
                  }
                  return true;
                },
              })
              .test({
                name: 'lines-contain-valid-emails',
                message: ({ value }) =>
                  `'${value}' is not a valid email address`,
                test(value?: string) {
                  if (value) {
                    const emails = splitAndTrimLines(value);
                    const indexOfFirstInvalid = emails.findIndex(
                      email => !isValidEmail(email),
                    );
                    if (indexOfFirstInvalid < 0) {
                      return true;
                    }
                    // eslint-disable-next-line react/no-this-in-sfc
                    return this.createError({
                      path: 'emails',
                      params: {
                        value: emails[indexOfFirstInvalid],
                      },
                    });
                  }
                  return true;
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
                hint={`Invite up to ${inviteLimit} users at a time. Enter each email address on a new line.`}
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

              {invitePlan && (
                <PreReleaseInvitePlanModal
                  isReleaseApproved={isReleaseApproved}
                  invitePlan={invitePlan}
                  onConfirm={async () => {
                    await handleModalSubmit(form.values, form);
                    setInvitePlan(undefined);
                  }}
                  onCancel={handleModalCancel}
                  onExit={handleModalCancel}
                />
              )}
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
