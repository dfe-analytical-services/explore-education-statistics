import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import dashboardRoutes from '@admin/routes/dashboard/routes';
import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import service from '@admin/services/edit-publication/service';
import methodologyService from '@admin/services/methodology/service';
import Button from '@common/components/Button';
import { FormFieldset, Formik } from '@common/components/form';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import handleServerSideValidation, {
  errorCodeToFieldError,
} from '@common/components/form/util/serverValidationHandler';
import RelatedInformation from '@common/components/RelatedInformation';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { FormikProps } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { CreateMethodologyRequest } from '@admin/services/methodology/types';
import {
  DayMonthYearInputs,
  dateToDayMonthYear,
  dayMonthYearValuesToInputs,
  dayMonthYearInputsToDate,
} from '@common/services/publicationService';
import { validateMandatoryDayMonthYearField } from '@admin/validation/validation';

interface MatchProps {
  topicId: string;
}

interface FormValues {
  methodologyTitle: string;
  selectedContactId: string;
  scheduledPublishDate: DayMonthYearInputs;
}

const serverSideValidationHandler = handleServerSideValidation(
  errorCodeToFieldError(
    'SLUG_NOT_UNIQUE',
    'methodologyTitle',
    'Choose a unique title',
  ),
);

interface CreateMethodologyModel {
  contacts: ContactDetails[];
}

const CreateMethodologyPage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const [model, setModel] = useState<CreateMethodologyModel>();

  useEffect(() => {
    Promise.all([service.getPublicationAndReleaseContacts()]).then(
      ([contacts]) => {
        setModel({
          contacts,
        });
      },
    );
  }, []);

  const submitFormHandler = async (values: FormValues) => {
    const submission: CreateMethodologyRequest = {
      title: values.methodologyTitle,
      publishScheduled: dayMonthYearInputsToDate(values.scheduledPublishDate),
      contactId: values.selectedContactId,
    };

    const createdMethodology = await methodologyService.createMethodology(
      submission,
    );

    // TODO: redirect to the methodology summary
    history.push(`/methodology/${createdMethodology.id}`);
  };

  const cancelHandler = () => history.push('/methodology');

  const getSelectedContact = (
    contactId: string,
    availableContacts: ContactDetails[],
  ) =>
    availableContacts.find(contact => contact.id === contactId) ||
    availableContacts[0];

  const formId = 'createMethodologyForm';

  return (
    <Page
      wide
      breadcrumbs={[
        {
          name: 'Manage methodology',
          link: '/methodology',
        },
        {
          name: 'Create new methodology',
        },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">Create new methodology</h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="#" target="blank">
                  Creating new methodology{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      {model && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            methodologyTitle: '',
            selectedContactId: orderBy(
              model.contacts,
              contact => contact.contactName,
            )[0].id,
            scheduledPublishDate: dayMonthYearValuesToInputs(
              dateToDayMonthYear(new Date('')),
            ),
          }}
          validationSchema={Yup.object({
            methodologyTitle: Yup.string().required(
              'Enter a methodology title',
            ),
            selectedContactId: Yup.string().required(
              'Choose a methodology contact',
            ),
            scheduledPublishDate: validateMandatoryDayMonthYearField,
          })}
          onSubmit={submitFormHandler}
          render={(form: FormikProps<FormValues>) => {
            return (
              <Form
                id={formId}
                submitValidationHandler={serverSideValidationHandler}
              >
                <FormFieldTextInput
                  id={`${formId}-methodologyTitle`}
                  label="Enter methodology title"
                  name="methodologyTitle"
                />

                <FormFieldDayMonthYear<FormValues>
                  formId={formId}
                  fieldName="scheduledPublishDate"
                  fieldsetLegend="Schedule publish date"
                  day={form.values.scheduledPublishDate.day}
                  month={form.values.scheduledPublishDate.month}
                  year={form.values.scheduledPublishDate.year}
                />

                <FormFieldset
                  id={`${formId}-selectedContactIdFieldset`}
                  legend="Choose the contact for this methodology"
                  legendSize="m"
                  hint="They will be the main point of contact for this methodology and its associated publications."
                >
                  <FormFieldSelect
                    id={`${formId}-selectedContactId`}
                    label="Methodology contact"
                    name="selectedContactId"
                    options={model.contacts.map(contact => ({
                      label: contact.contactName,
                      value: contact.id,
                    }))}
                  />
                </FormFieldset>
                {form.values.selectedContactId && (
                  <SummaryList>
                    <SummaryListItem term="Email">
                      {
                        getSelectedContact(
                          form.values.selectedContactId,
                          model.contacts,
                        ).teamEmail
                      }
                    </SummaryListItem>
                    <SummaryListItem term="Telephone">
                      {
                        getSelectedContact(
                          form.values.selectedContactId,
                          model.contacts,
                        ).contactTelNo
                      }
                    </SummaryListItem>
                  </SummaryList>
                )}
                <Button type="submit" className="govuk-!-margin-top-6">
                  Create methodology
                </Button>
                <div className="govuk-!-margin-top-6">
                  <Link to="#" onClick={cancelHandler}>
                    Cancel
                  </Link>
                </div>
              </Form>
            );
          }}
        />
      )}
    </Page>
  );
};

export default CreateMethodologyPage;
