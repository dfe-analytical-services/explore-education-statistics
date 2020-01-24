import Link from '@admin/components/Link';
import { PrereleaseContactDetails } from '@admin/services/common/types';
import dashboardService from '@admin/services/dashboard/service';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import { Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldset from '@common/components/form/FormFieldset';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import React, { useEffect, useState } from 'react';

interface Model {
  availablePreReleaseContacts: PrereleaseContactDetails[];
  preReleaseContactsForRelease: PrereleaseContactDetails[];
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
    Promise.all([
      dashboardService.getAvailablePreReleaseContacts(),
      dashboardService.getPreReleaseContactsForRelease(release.id),
    ])
      .then(([availablePreReleaseContacts, preReleaseContactsForRelease]) =>
        setModel({
          availablePreReleaseContacts,
          preReleaseContactsForRelease,
        }),
      )
      .catch(handleApiErrors);
  }, [handleApiErrors, release.id]);

  const formId = 'invitePrereleaseAccessUsers';

  const errorCodeMappings = [
    errorCodeToFieldError(
      'USER_ALREADY_EXISTS',
      'userEmail',
      'User already exists',
    ),
  ];

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async values => {
      await dashboardService
        .addPreReleaseContactToRelease(release.id, values.email)
        .then(updatedContacts =>
          setModel({
            availablePreReleaseContacts:
              (model && model.availablePreReleaseContacts) || [],
            preReleaseContactsForRelease: updatedContacts,
          }),
        )
        .catch(handleApiErrors);
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
              email: Yup.string().required('Enter an email address'),
            })}
            onSubmit={submitFormHandler}
            render={() => {
              return (
                <Form id={formId}>
                  <FormFieldset
                    legend="Manage pre release access"
                    legendSize="s"
                    id="pre-release-selection"
                  >
                    <FormFieldSelect
                      id="preReleaseAccessContact"
                      name="preReleaseAccessContact"
                      label="Select user"
                      options={[
                        {
                          label: 'Please select',
                          value: '',
                        },
                        ...model.availablePreReleaseContacts
                          .filter(
                            contact =>
                              !model.preReleaseContactsForRelease.find(
                                c => c.email === contact.email,
                              ),
                          )
                          .map(contact => ({
                            label: contact.email,
                            value: contact.email,
                          })),
                      ]}
                      order={[]}
                      className="govuk-!-width-one-third"
                      onChange={event => {
                        dashboardService
                          .addPreReleaseContactToRelease(
                            release.id,
                            event.target.value,
                          )
                          .then(updatedContacts =>
                            setModel({
                              ...model,
                              preReleaseContactsForRelease: updatedContacts,
                            }),
                          )
                          .catch(handleApiErrors);
                      }}
                    />
                    <FormFieldTextInput
                      id={`${formId}-email`}
                      label="or invite a new user"
                      name="email"
                    />
                    <Button
                      type="submit"
                      className="govuk-!-margin-top-6 govuk-button--secondary"
                    >
                      Invite new user
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
                  <Link
                    to="#"
                    onClick={() => {
                      dashboardService
                        .removePreReleaseContactFromRelease(
                          release.id,
                          existingContact.email,
                        )
                        .then(updatedContacts =>
                          setModel({
                            ...model,
                            preReleaseContactsForRelease: updatedContacts,
                          }),
                        )
                        .catch(handleApiErrors);
                    }}
                  >
                    Remove
                  </Link>
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
