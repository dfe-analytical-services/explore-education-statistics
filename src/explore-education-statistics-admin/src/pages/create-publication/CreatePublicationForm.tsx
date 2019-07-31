import Link from '@admin/components/Link';
import { ContactDetails, IdLabelPair } from '@admin/services/common/types';
import Button from '@common/components/Button';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { FormikProps } from 'formik';
import React from 'react';

interface FormValues {
  publicationTitle: string;
  methodologyChoice?: 'existing' | 'new';
  selectedMethodologyId?: string;
  selectedContactId: string;
}

interface Props {
  contacts: ContactDetails[];
  methodologies: IdLabelPair[];
  currentValues?: FormValues;
  onSubmitHandler: (values: FormValues) => void;
  onCancelHandler: () => void;
}

const CreatePublicationForm = ({
  contacts,
  methodologies,
  currentValues,
  onSubmitHandler,
  onCancelHandler,
}: Props) => {
  const getSelectedContact = (contactId: string) =>
    contacts.find(contact => contact.id === contactId) || contacts[0];

  const formId = 'createPublicationForm';

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        publicationTitle: currentValues ? currentValues.publicationTitle : '',
        selectedContactId: currentValues
          ? currentValues.selectedContactId
          : contacts[0].id,
        methodologyChoice: currentValues
          ? currentValues.methodologyChoice
          : undefined,
        selectedMethodologyId:
          currentValues && currentValues.selectedMethodologyId
            ? currentValues.selectedMethodologyId
            : methodologies[0].id,
      }}
      validationSchema={Yup.object<FormValues>({
        publicationTitle: Yup.string().required('Enter a publication title'),
        selectedContactId: Yup.string().required(
          'Choose a publication and release contact',
        ),
        methodologyChoice: Yup.mixed().required('Choose a methodology'),
        selectedMethodologyId: Yup.string().when('methodologyChoice', {
          is: 'existing',
          then: Yup.string().required('Choose a methodology'),
          otherwise: Yup.string(),
        }),
      })}
      onSubmit={(values: FormValues) => {
        const valuesToStore: FormValues = {
          ...values,
          selectedMethodologyId:
            values.methodologyChoice === 'existing'
              ? values.selectedMethodologyId
              : undefined,
        };
        onSubmitHandler(valuesToStore);
      }}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId}>
            <FormFieldTextInput
              id={`${formId}-publicationTitle`}
              label="Enter publication title"
              name="publicationTitle"
            />
            <FormFieldRadioGroup
              id={`${formId}-methodologyChoice`}
              legend="Choose a methodology for this publication"
              name="methodologyChoice"
              options={[
                {
                  value: 'existing',
                  label: 'Add existing methodology',
                },
                {
                  value: 'new',
                  label: 'Create new methodology',
                },
              ]}
            />
            {form.values.methodologyChoice === 'existing' && (
              <FormFieldSelect
                id={`${formId}-selectedMethodologyId`}
                name="selectedMethodologyId"
                label="Select methodology"
                options={methodologies.map(methodology => ({
                  label: methodology.label,
                  value: methodology.id,
                }))}
              />
            )}
            <FormFieldset
              id={`${formId}-selectedContactIdFieldset`}
              legend="Choose the contact for this publication"
              hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
            >
              <FormFieldSelect
                id={`${formId}-selectedContactId`}
                label="Publication and release contact"
                name="selectedContactId"
                options={contacts.map(contact => ({
                  label: contact.contactName,
                  value: contact.id,
                }))}
              />
            </FormFieldset>
            {form.values.selectedContactId && (
              <SummaryList>
                <SummaryListItem term="Email">
                  {getSelectedContact(form.values.selectedContactId).teamEmail}
                </SummaryListItem>
                <SummaryListItem term="Telephone">
                  {
                    getSelectedContact(form.values.selectedContactId)
                      .contactTelNo
                  }
                </SummaryListItem>
              </SummaryList>
            )}
            <Button type="submit" className="govuk-!-margin-top-6">
              Continue
            </Button>
            <div className="govuk-!-margin-top-6">
              <Link to="#" onClick={onCancelHandler}>
                Cancel publication
              </Link>
            </div>
          </Form>
        );
      }}
    />
  );
};

export default CreatePublicationForm;
