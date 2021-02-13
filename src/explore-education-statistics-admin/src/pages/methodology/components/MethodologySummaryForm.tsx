import contactService from '@admin/services/contactService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

export interface MethodologySummaryFormValues {
  title: string;
  contactId: string;
}

const errorMappings = [
  mapFieldErrors<MethodologySummaryFormValues>({
    target: 'title',
    messages: {
      SLUG_NOT_UNIQUE: 'Choose a unique title',
    },
  }),
];

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
    contactService.getContacts,
    [],
  );

  const getSelectedContact = (contactId: string) =>
    contacts.find(contact => contact.id === contactId) ?? contacts[0];

  const handleSubmit = useFormSubmit<MethodologySummaryFormValues>(
    async values => {
      onSubmit(values as MethodologySummaryFormValues);
    },
    errorMappings,
  );

  // TODO EES-899 - Save methodology contact in backend
  const isContactEnabled = false;

  return (
    <Formik<MethodologySummaryFormValues>
      enableReinitialize
      initialValues={
        initialValues ??
        ({
          title: '',
          contactId: '',
        } as MethodologySummaryFormValues)
      }
      validationSchema={Yup.object<MethodologySummaryFormValues>({
        title: Yup.string().required('Enter a methodology title'),
        contactId: isContactEnabled
          ? Yup.string().required('Choose a methodology contact')
          : Yup.string(),
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

            {isContactEnabled && (
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
            )}

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
