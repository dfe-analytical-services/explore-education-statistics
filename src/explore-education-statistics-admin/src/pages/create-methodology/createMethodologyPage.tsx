import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import dashboardRoutes from '@admin/routes/dashboard/routes';
import { ContactDetails, IdTitlePair } from '@admin/services/common/types';
import service from '@admin/services/edit-publication/service';
import Button from '@common/components/Button';
import { FormFieldset, Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
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
import { Topic } from '@admin/services/edit-publication/types';

interface MatchProps {
  topicId: string;
}

interface FormValues {
  publicationTitle: string;
  methodologyChoice?: 'existing' | 'new';
  selectedMethodologyId?: string;
  selectedContactId: string;
}

const serverSideValidationHandler = handleServerSideValidation(
  errorCodeToFieldError(
    'SLUG_NOT_UNIQUE',
    'publicationTitle',
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
  const { topicId } = match.params;

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
    await service.createPublication({
      topicId,
      ...values,
    });

    history.push(dashboardRoutes.adminDashboard);
  };

  const cancelHandler = () => {
    history.push(dashboardRoutes.adminDashboard);
  };

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
            publicationTitle: '',
            selectedContactId: orderBy(
              model.contacts,
              contact => contact.contactName,
            )[0].id,
            methodologyChoice: undefined,
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
          })}
          onSubmit={submitFormHandler}
          render={(form: FormikProps<FormValues>) => {
            return (
              <Form
                id={formId}
                submitValidationHandler={serverSideValidationHandler}
              >
                <FormFieldTextInput
                  id={`${formId}-publicationTitle`}
                  label="Enter methodology title"
                  name="methodologyTitle"
                />

                <FormFieldset
                  id={`${formId}-selectedContactIdFieldset`}
                  legend="Choose the contact for this publication"
                  legendSize="m"
                  hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
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
