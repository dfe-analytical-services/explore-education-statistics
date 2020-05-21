import useFormSubmit from '@admin/hooks/useFormSubmit';
import service from '@admin/services/edit-publication/service';
import { validateMandatoryDayMonthYearField } from '@admin/validation/validation';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { FormGroup } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import {
  dateToDayMonthYear,
  DayMonthYearValues,
} from '@common/utils/date/dayMonthYear';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

const errorCodeMappings = [
  errorCodeToFieldError('SLUG_NOT_UNIQUE', 'title', 'Choose a unique title'),
];

export interface MethodologySummaryFormValues {
  title: string;
  contactId: string;
  publishScheduled: DayMonthYearValues;
}

interface Props {
  id: string;
  initialValues?: MethodologySummaryFormValues;
  submitText: string;
  onCancel: () => void;
  onSubmit: (values: MethodologySummaryFormValues) => void;
}

const MethodologySummaryForm = ({
  id,
  initialValues,
  submitText,
  onCancel,
  onSubmit,
}: Props) => {
  const { value: contacts = [] } = useAsyncHandledRetry(
    service.getPublicationAndReleaseContacts,
    [],
  );

  const getSelectedContact = (contactId: string) =>
    contacts.find(contact => contact.id === contactId) ?? contacts[0];

  const handleSubmit = useFormSubmit<MethodologySummaryFormValues>(
    async values => {
      onSubmit(values);
    },
    errorCodeMappings,
  );

  return (
    <Formik<MethodologySummaryFormValues>
      enableReinitialize
      initialValues={
        initialValues ?? {
          title: '',
          contactId: '',
          publishScheduled: dateToDayMonthYear(new Date('')),
        }
      }
      validationSchema={Yup.object<MethodologySummaryFormValues>({
        title: Yup.string().required('Enter a methodology title'),
        contactId: Yup.string().required('Choose a methodology contact'),
        publishScheduled: validateMandatoryDayMonthYearField,
      })}
      onSubmit={handleSubmit}
    >
      {form => {
        return (
          <Form id={id}>
            <FormFieldTextInput<MethodologySummaryFormValues>
              id={`${id}-title`}
              label="Enter methodology title"
              name="title"
            />

            <FormFieldDayMonthYear<MethodologySummaryFormValues>
              id={`${id}-publishScheduled`}
              name="publishScheduled"
              legend="Schedule publish date"
              legendSize="s"
            />

            <FormFieldSelect<MethodologySummaryFormValues>
              id={`${id}-contactId`}
              hint="They will be the main point of contact for this methodology and its associated publications."
              label="Choose the contact for this methodology"
              name="contactId"
              placeholder="Select a contact"
              options={contacts.map(contact => ({
                label: contact.contactName,
                value: contact.id,
              }))}
            />

            {form.values.contactId && (
              <SummaryList>
                <SummaryListItem term="Email">
                  {getSelectedContact(form.values.contactId).teamEmail}
                </SummaryListItem>
                <SummaryListItem term="Telephone">
                  {getSelectedContact(form.values.contactId).contactTelNo}
                </SummaryListItem>
              </SummaryList>
            )}

            <ButtonGroup>
              <Button type="submit">{submitText}</Button>

              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </Form>
        );
      }}
    </Formik>
  );
};

export default MethodologySummaryForm;
