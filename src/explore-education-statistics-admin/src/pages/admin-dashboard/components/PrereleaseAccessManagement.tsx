import { PrereleaseContactDetails } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldset from '@common/components/form/FormFieldset';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import React, { useEffect, useState } from 'react';

interface Model {
  preReleaseContactsForRelease: PrereleaseContactDetails[];
  inviting: boolean;
  removing: boolean;
}

interface FormValues {
  email: string;
}

interface Props {
  release: AdminDashboardRelease;
}

const PrereleaseAccessManagement = ({
  release,
  handleApiErrors,
}: Props & ErrorControlProps) => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    dashboardService
      .getPreReleaseContactsForRelease(release.id)
      .then(preReleaseContactsForRelease =>
        setModel({
          preReleaseContactsForRelease,
          inviting: false,
          removing: false,
        }),
      )
      .catch(handleApiErrors);
  }, [handleApiErrors, release.id]);

  const formId = `invitePrereleaseAccessUsers-${release.id}`;

  const errorCodeMappings = [
    errorCodeToFieldError(
      'USER_ALREADY_EXISTS',
      'userEmail',
      'User already exists',
    ),
  ];

  const inviteUserByEmail: (
    email: string,
    resetForm: () => void,
  ) => Promise<void> = async (email, resetForm) => {
    setModel({
      ...(model as Model),
      inviting: true,
    });

    await dashboardService
      .addPreReleaseContactToRelease(release.id, email)
      .then(updatedContacts => {
        resetForm();

        setModel({
          preReleaseContactsForRelease: updatedContacts,
          inviting: false,
          removing: false,
        });
      })
      .catch(handleApiErrors);
  };

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async (values, actions) => {
      await inviteUserByEmail(values.email, actions.resetForm);
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

  return (
    <>
      {model && (
        <>
          <Formik<FormValues>
            enableReinitialize
            initialValues={{
              email: '',
            }}
            validationSchema={Yup.object<FormValues>({
              email: Yup.string().email('Enter a valid email address'),
            })}
            onSubmit={submitFormHandler}
            render={() => {
              return (
                <Form id={formId}>
                  <FormFieldset
                    legend="Manage pre release access"
                    legendSize="s"
                    id={`pre-release-selection-${release.id}`}
                  >
                    <FormFieldTextInput
                      id={`${formId}-email`}
                      label="Invite a new user"
                      name="email"
                      disabled={model.inviting || model.removing}
                    />
                    <Button
                      type="submit"
                      className="govuk-!-margin-top-6 govuk-button--secondary"
                      disabled={model.inviting || model.removing}
                    >
                      {model.inviting && 'Inviting new user'}
                      {model.removing && 'Removing user'}
                      {!model.inviting && !model.removing && 'Invite new user'}
                    </Button>
                  </FormFieldset>
                </Form>
              );
            }}
          />

          <SummaryList>
            {model.preReleaseContactsForRelease.map(existingContact => (
              <SummaryListItem
                key={existingContact.email}
                term="Pre release access"
                actions={
                  <ButtonText
                    onClick={() => {
                      setModel({
                        ...model,
                        removing: true,
                      });

                      dashboardService
                        .removePreReleaseContactFromRelease(
                          release.id,
                          existingContact.email,
                        )
                        .then(updatedContacts =>
                          setModel({
                            ...model,
                            preReleaseContactsForRelease: updatedContacts,
                            removing: false,
                          }),
                        )
                        .catch(handleApiErrors);
                    }}
                  >
                    Remove
                  </ButtonText>
                }
              >
                {existingContact.email}
                {existingContact.invited && <> (invited)</>}
              </SummaryListItem>
            ))}
          </SummaryList>
        </>
      )}
    </>
  );
};

export default withErrorControl(PrereleaseAccessManagement);
