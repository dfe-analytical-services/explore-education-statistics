import PreReleaseInvitePlanModal from '@admin/pages/release/pre-release/components/PreReleaseInvitePlanModal';
import preReleaseUserService, {
  PreReleaseInvitePlan,
} from '@admin/services/preReleaseUserService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import Gate from '@common/components/Gate';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useCallback, useMemo, useState } from 'react';
import { ObjectSchema } from 'yup';

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
  releaseVersionId: string;
  isReleaseApproved?: boolean;
  isReleaseLive?: boolean;
}

const formId = 'preReleaseUserAccessForm';
const inviteLimit = 50;

export default function PreReleaseUserAccessForm({
  releaseVersionId,
  isReleaseApproved = false,
  isReleaseLive = false,
}: Props) {
  const [isRemoving, toggleRemoving] = useToggle(false);

  const {
    value: users = [],
    isLoading,
    error,
    setState: setUsers,
  } = useAsyncRetry(
    () => preReleaseUserService.getUsers(releaseVersionId),
    [releaseVersionId],
  );

  const [invitePlan, setInvitePlan] = useState<PreReleaseInvitePlan>();

  const splitAndTrimLines = (input: string) =>
    input
      .split(/\r\n|\r|\n/)
      .map(line => line.trim())
      .filter(line => line);

  const handleSubmit = async (values: FormValues) => {
    setInvitePlan(
      await preReleaseUserService.getInvitePlan(
        releaseVersionId,
        splitAndTrimLines(values.emails),
      ),
    );
  };

  const handleModalSubmit = async (emails: string) => {
    const newUsers = await preReleaseUserService.inviteUsers(
      releaseVersionId,
      splitAndTrimLines(emails),
    );

    setUsers({
      value: [...users, ...newUsers],
    });
  };

  const handleModalCancel = useCallback(() => {
    setInvitePlan(undefined);
  }, []);

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
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
          message: ({ value }) => `'${value}' is not a valid email address`,
          test(value?: string) {
            if (value) {
              const emails = splitAndTrimLines(value);
              const schema = Yup.string().email();
              const indexOfFirstInvalid = emails.findIndex(
                email => !schema.isValidSync(email),
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
    });
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
        <FormProvider
          errorMappings={errorMappings}
          initialValues={{
            emails: '',
          }}
          validationSchema={validationSchema}
        >
          {({ formState, getValues, reset }) => {
            return (
              <Form id={formId} onSubmit={handleSubmit}>
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
                    disabled={formState.isSubmitting || isRemoving}
                  >
                    {formState.isSubmitting
                      ? 'Inviting new users'
                      : 'Invite new users'}
                  </Button>
                </ButtonGroup>

                {invitePlan && (
                  <PreReleaseInvitePlanModal
                    isReleaseApproved={isReleaseApproved}
                    invitePlan={invitePlan}
                    onConfirm={async () => {
                      await handleModalSubmit(getValues('emails'));
                      setInvitePlan(undefined);
                      reset();
                    }}
                    onCancel={handleModalCancel}
                    onExit={handleModalCancel}
                  />
                )}
              </Form>
            );
          }}
        </FormProvider>
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
                    <td className="govuk-!-text-align-right">
                      <ButtonText
                        disabled={isRemoving}
                        onClick={async () => {
                          toggleRemoving.on();

                          await preReleaseUserService.removeUser(
                            releaseVersionId,
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
}
