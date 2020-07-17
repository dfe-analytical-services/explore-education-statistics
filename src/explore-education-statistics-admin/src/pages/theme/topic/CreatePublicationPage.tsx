import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import ThemeAndTopicContext from '@admin/components/ThemeAndTopicContext';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { dashboardRoute } from '@admin/routes/routes';
import contactService, { ContactDetails } from '@admin/services/contactService';
import { ExternalMethodology } from '@admin/services/dashboardService';
import methodologyService, {
  BasicMethodology,
} from '@admin/services/methodologyService';
import publicationService from '@admin/services/publicationService';
import { IdTitlePair } from '@admin/services/types/common';
import { Dictionary } from '@admin/types';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset, FormGroup } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import RelatedInformation from '@common/components/RelatedInformation';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { AssignMethodologyFormValues } from './publication/AssignMethodologyForm';

interface FormValues extends AssignMethodologyFormValues {
  publicationTitle: string;
  selectedContactId: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'publicationTitle',
    messages: {
      SLUG_NOT_UNIQUE: 'Choose a unique title',
    },
  }),
];

interface CreatePublicationModel {
  methodologies: BasicMethodology[];
  contacts: ContactDetails[];
  topic: IdTitlePair;
}

const CreatePublicationPage = ({
  history,
}: RouteComponentProps<{ topicId: string }>) => {
  const [model, setModel] = useState<CreatePublicationModel>();

  const { topic } = useContext(ThemeAndTopicContext).selectedThemeAndTopic;

  useEffect(() => {
    Promise.all([
      methodologyService.getMethodologies(),
      contactService.getContacts(),
    ]).then(([methodologies, contacts]) => {
      setModel({
        methodologies,
        contacts,
        topic,
      });
    });
  }, [topic]);

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    const methodology: Dictionary<string | undefined | ExternalMethodology> = {
      selectedMethodologyId: undefined,
      externalMethodology: undefined,
    };
    if (values.methodologyChoice === 'existing') {
      methodology.selectedMethodologyId = values.selectedMethodologyId as string;
    }
    if (values.methodologyChoice === 'external') {
      methodology.externalMethodology = values.externalMethodology;
    }
    await publicationService.createPublication({
      topicId: topic.id,
      ...values,
      ...methodology,
    });

    history.push(dashboardRoute.path);
  }, errorMappings);

  const cancelHandler = () => {
    history.push(dashboardRoute.path);
  };

  const getSelectedContact = (
    contactId: string,
    availableContacts: ContactDetails[],
  ) =>
    availableContacts.find(contact => contact.id === contactId) ||
    availableContacts[0];

  const formId = 'createPublicationForm';

  return (
    <Page
      wide
      breadcrumbs={[
        {
          name: 'Create new publication',
        },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl">
              {model && model.topic.title}
            </span>
            Create new publication
          </h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation/create-new-publication" target="blank">
                  Creating a new publication
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
            publicationTitle: '',
            selectedContactId: '',
            methodologyChoice: 'existing',
            selectedMethodologyId: '',
            externalMethodology: { title: '', url: 'https://' },
          }}
          validationSchema={Yup.object<FormValues>({
            publicationTitle: Yup.string().required(
              'Enter a publication title',
            ),
            selectedContactId: Yup.string().required(
              'Choose a publication and release contact',
            ),
            methodologyChoice: Yup.mixed().required('Choose a methodology'),
            selectedMethodologyId: Yup.string().when('methodologyChoice', {
              is: 'existing',
              then: Yup.string().required('Choose a methodology'),
              otherwise: Yup.string(),
            }),
            externalMethodology: Yup.object<{
              title: string;
              url: string;
            }>().when('methodologyChoice', {
              is: 'external',
              then: Yup.object().shape({
                title: Yup.string().required('Enter a link title'),
                url: Yup.string()
                  .required('Enter a URL')
                  .url('Enter a valid URL')
                  .test({
                    name: 'currentHostUrl',
                    message: 'URL cannot be for this website',
                    test: (value: string) =>
                      Boolean(value && !value.includes(window.location.host)),
                  }),
              }),
            }),
          })}
          onSubmit={handleSubmit}
        >
          {form => {
            return (
              <Form id={formId}>
                <FormFieldTextInput
                  id={`${formId}-publicationTitle`}
                  label="Enter publication title"
                  name="publicationTitle"
                />
                <FormFieldRadioGroup<FormValues>
                  id={`${formId}-methodologyChoice`}
                  legend="Choose a methodology for this publication"
                  legendSize="m"
                  name="methodologyChoice"
                  options={[
                    {
                      value: 'existing',
                      label: 'Choose an existing methodology',
                      conditional: (
                        <FormFieldSelect<FormValues>
                          id={`${formId}-selectedMethodologyId`}
                          name="selectedMethodologyId"
                          label="Select methodology"
                          options={[
                            { label: 'Choose a methodology', value: '' },
                            ...orderBy(
                              model.methodologies
                                .filter(
                                  methodology => methodology.status !== 'Draft',
                                )
                                .map(methodology => ({
                                  label: `${methodology.title} [${methodology.status}]`,
                                  value: methodology.id,
                                })),
                              'label',
                            ),
                          ]}
                          order={[]}
                        />
                      ),
                    },
                    {
                      value: 'external',
                      label: 'Link to an externally hosted methodology',
                      conditional: (
                        <FormGroup>
                          <FormFieldTextInput
                            label="Link title"
                            id="externalMethodology.title"
                            name="externalMethodology.title"
                          />
                          <FormFieldTextInput
                            label="URL"
                            id="externalMethodology.url"
                            name="externalMethodology.url"
                          />
                        </FormGroup>
                      ),
                    },
                    {
                      value: 'later',
                      label: 'Select a methodology later',
                    },
                  ]}
                  onChange={e => {
                    if (e.target.value === 'existing') {
                      return form.setValues({
                        ...form.values,
                        selectedMethodologyId: orderBy(
                          model.methodologies,
                          methodology => methodology.title,
                        )[0].id,
                        externalMethodology: {
                          title: '',
                          url: '',
                        },
                      });
                    }
                    if (e.target.value === 'external') {
                      return form.setValues({
                        ...form.values,
                        selectedMethodologyId: '',
                      });
                    }
                    return form.setValues({
                      ...form.values,
                      selectedMethodologyId: '',
                      externalMethodology: {
                        title: '',
                        url: '',
                      },
                    });
                  }}
                />
                <FormFieldset
                  className="govuk-!-margin-top-9"
                  id={`${formId}-selectedContactIdFieldset`}
                  legend="Choose the contact for this publication"
                  legendSize="m"
                  hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
                >
                  <FormFieldSelect<FormValues>
                    id={`${formId}-selectedContactId`}
                    label="Publication and release contact"
                    name="selectedContactId"
                    options={[
                      { label: 'Choose a contact', value: '' },
                      ...orderBy(
                        model.contacts.map(contact => ({
                          label: `${contact.contactName} - (${contact.teamName})`,
                          value: contact.id,
                        })),
                        'label',
                      ),
                    ]}
                    order={[]}
                  />
                </FormFieldset>
                {form.values.selectedContactId && (
                  <SummaryList>
                    <SummaryListItem term="Team">
                      {
                        getSelectedContact(
                          form.values.selectedContactId,
                          model.contacts,
                        ).teamName
                      }
                    </SummaryListItem>
                    <SummaryListItem term="Name">
                      {
                        getSelectedContact(
                          form.values.selectedContactId,
                          model.contacts,
                        ).contactName
                      }
                    </SummaryListItem>
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
                  Create publication
                </Button>
                <div className="govuk-!-margin-top-6">
                  <ButtonText onClick={cancelHandler}>
                    Cancel publication
                  </ButtonText>
                </div>
              </Form>
            );
          }}
        </Formik>
      )}
    </Page>
  );
};

export default CreatePublicationPage;
