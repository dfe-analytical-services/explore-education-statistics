import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import ThemeAndTopicContext from '@admin/components/ThemeAndTopicContext';
import { ErrorControlState } from '@admin/contexts/ErrorControlContext';
import withErrorControl from '@admin/hocs/withErrorControl';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import appRouteList from '@admin/routes/dashboard/routes';
import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import { ExternalMethodology } from '@admin/services/dashboard/types';
import service from '@admin/services/edit-publication/service';
import { Dictionary } from '@admin/types';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset, FormGroup, Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import RelatedInformation from '@common/components/RelatedInformation';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { FormikProps } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { AssignMethodologyFormValues } from './publication/AssignMethodologyForm';

const errorCodeMappings = [
  errorCodeToFieldError(
    'SLUG_NOT_UNIQUE',
    'publicationTitle',
    'Choose a unique title',
  ),
];

interface FormValues extends AssignMethodologyFormValues {
  publicationTitle: string;
  selectedContactId: string;
}

interface CreatePublicationModel {
  methodologies: IdTitlePair[];
  contacts: ContactDetails[];
  topic: IdTitlePair;
}

const CreatePublicationPage = ({
  history,
  handleApiErrors,
}: RouteComponentProps<{ topicId: string }> & ErrorControlState) => {
  const [model, setModel] = useState<CreatePublicationModel>();

  const { topic } = useContext(ThemeAndTopicContext).selectedThemeAndTopic;

  useEffect(() => {
    Promise.all([
      service.getMethodologies(),
      service.getPublicationAndReleaseContacts(),
    ])
      .then(([methodologies, contacts]) => {
        setModel({
          methodologies,
          contacts,
          topic,
        });
      })
      .catch(handleApiErrors);
  }, [topic, handleApiErrors]);

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
    await service.createPublication({
      topicId: topic.id,
      ...values,
      ...methodology,
    });

    history.push(appRouteList.adminDashboard.path as string);
  }, errorCodeMappings);

  const cancelHandler = () => {
    history.push(appRouteList.adminDashboard.path as string);
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
            selectedContactId: orderBy(
              model.contacts,
              contact => contact.contactName,
            )[0].id,
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
                url: Yup.string().url('Enter a valid URL'),
              }),
            }),
          })}
          onSubmit={handleSubmit}
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
                  legendSize="m"
                  name="methodologyChoice"
                  options={[
                    {
                      value: 'existing',
                      label: 'Choose an existing methodology',
                      conditional: (
                        <FormFieldSelect
                          id={`${formId}-selectedMethodologyId`}
                          name="selectedMethodologyId"
                          label="Select methodology"
                          options={model.methodologies.map(methodology => ({
                            label: methodology.title,
                            value: methodology.id,
                          }))}
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
                  <FormFieldSelect
                    id={`${formId}-selectedContactId`}
                    label="Publication and release contact"
                    name="selectedContactId"
                    options={model.contacts.map(contact => ({
                      label: `${contact.contactName} - (${contact.teamName})`,
                      value: contact.id,
                    }))}
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
        />
      )}
    </Page>
  );
};

export default withErrorControl(CreatePublicationPage);
