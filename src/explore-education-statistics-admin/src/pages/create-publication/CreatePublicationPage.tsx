import Link from "@admin/components/Link";
import Page from "@admin/components/Page";
import {ContactDetails, IdLabelPair} from "@admin/services/common/types";
import service from '@admin/services/edit-publication/service';
import Button from "@common/components/Button";
import {Form, FormFieldset, Formik} from "@common/components/form";
import FormFieldRadioGroup from "@common/components/form/FormFieldRadioGroup";
import FormFieldSelect from "@common/components/form/FormFieldSelect";
import FormFieldTextInput from "@common/components/form/FormFieldTextInput";
import SummaryList from "@common/components/SummaryList";
import SummaryListItem from "@common/components/SummaryListItem";
import Yup from "@common/lib/validation/yup";
import {FormikProps} from "formik";
import React, {useEffect, useState} from 'react';
import {RouteComponentProps} from 'react-router';

interface MatchProps {
  topicId: string;
}

interface FormValues {
  publicationTitle: string;
  methodologyChoice?: 'existing' | 'new';
  selectedMethodologyId?: string;
  selectedContactId: string;
}

const CreatePublicationPage = ({ match, history }: RouteComponentProps<MatchProps>) => {

  const { topicId } = match.params;

  const [methodologies, setMethodologies] = useState<IdLabelPair[]>();
  const [contacts, setContacts] = useState<ContactDetails[]>();

  useEffect(() => {
    const methodologyPromise = service.getMethodologies();
    const contactsPromise = service.getPublicationAndReleaseContacts();
    Promise.all([methodologyPromise, contactsPromise]).
      then(([methodologiesResult, contactsResult]) => {
        setMethodologies(methodologiesResult);
        setContacts(contactsResult);
      }
    );
  }, []);

  const getSelectedContact = (availableContacts: ContactDetails[], contactId: string) =>
    availableContacts.find(contact => contact.id === contactId) || availableContacts[0];

  const formId = 'createPublicationForm';

  return (
    <Page wide breadcrumbs={[{ name: 'Create new publication' }]}>
      <h1 className="govuk-heading-l">
        Create new publication
      </h1>
      {contacts && methodologies && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            publicationTitle: '',
            selectedContactId: contacts[0].id,
            methodologyChoice: undefined,
            selectedMethodologyId: undefined,
          }}
          validationSchema={Yup.object<FormValues>({
            publicationTitle: Yup.string().required(
              'Enter a publication title',
            ),
            selectedContactId: Yup.string().required(
              'Choose a publication and release contact'
            ),
            selectedMethodologyId: Yup.string().when('methodologyChoice', {
              is: 'existing',
              then: Yup.string().required('Select a methodology'),
              otherwise: Yup.string(),
            }),
          })}
          onSubmit={async (values: FormValues) => {
            service.createPublication({
              topicId,
              publicationTitle: values.publicationTitle,
              selectedContactId: values.selectedContactId,
              selectedMethodologyId: values.selectedMethodologyId
            }).then(() => history.push('/create-publication/confirm'));
          }}
          render={(form: FormikProps<FormValues>) => {
            return (
              <Form id={formId}>
                <FormFieldTextInput
                  id={`${formId}-publicationTitle`}
                  label='Enter publication title'
                  name='publicationTitle'
                />
                <FormFieldRadioGroup
                  id={`${formId}-methodologyChoice`}
                  legend='Choose a methodology for this publication'
                  name='methodologyChoice'
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
                    name='selectedMethodologyId'
                    label='Select methodology'
                    options={methodologies.map(methodology => ({
                      label: methodology.label,
                      value: methodology.id,
                    }))}
                  />
                )}
                <FormFieldset
                  id={`${formId}-selectedContactIdFieldset`}
                  legend='Choose the contact for this publication'
                  hint='They will be the main point of contact for data and methodology enquiries for this publication and its releases.'
                >
                  <FormFieldSelect
                    id={`${formId}-selectedContactId`}
                    label='Publication and release contact'
                    name='selectedContactId'
                    options={contacts.map(contact => ({
                      label: contact.contactName,
                      value: contact.id,
                    }))}
                  />
                </FormFieldset>
                {form.values.selectedContactId && (
                  <SummaryList>
                    <SummaryListItem term='Email'>
                      {getSelectedContact(contacts, form.values.selectedContactId).teamEmail}
                    </SummaryListItem>
                    <SummaryListItem term='Telephone'>
                      {getSelectedContact(contacts, form.values.selectedContactId).contactTelNo}
                    </SummaryListItem>
                  </SummaryList>
                )}
                <Button type="submit" className="govuk-!-margin-top-6">
                  Continue
                </Button>
                <div className="govuk-!-margin-top-6">
                  <Link to='/admin-dashboard'>
                    Cancel publication
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

export default CreatePublicationPage;
