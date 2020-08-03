import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { dashboardRoute } from '@admin/routes/routes';
import { ExternalMethodology } from '@admin/services/dashboardService';
import methodologyService from '@admin/services/methodologyService';
import publicationService from '@admin/services/publicationService';
import topicService from '@admin/services/topicService';
import { Dictionary } from '@admin/types';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormFieldset, FormGroup } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import RelatedInformation from '@common/components/RelatedInformation';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface FormValues {
  title: string;
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
  methodologyChoice?: 'existing' | 'external' | 'later';
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'title',
    messages: {
      SLUG_NOT_UNIQUE: 'Choose a unique title',
    },
  }),
  mapFieldErrors<FormValues>({
    target: 'methodologyChoice',
    messages: {
      METHODOLOGY_DOES_NOT_EXIST:
        'There was a problem adding the selected methodology',
      METHODOLOGY_MUST_BE_APPROVED_OR_PUBLISHED:
        'Choose a methodology that is Live or ready to be published',
      METHODOLOGY_OR_EXTERNAL_METHODOLOGY_LINK_MUST_BE_DEFINED:
        'Either an existing methodology or an external methodology link must be provided',
      CANNOT_SPECIFY_METHODOLOGY_AND_EXTERNAL_METHODOLOGY:
        'Either an existing methodology or an external methodology link must be provided',
    },
  }),
];

const formId = 'createPublicationForm';

const PublicationCreatePage = ({
  history,
  match,
}: RouteComponentProps<{ topicId: string }>) => {
  const { topicId } = match.params;

  const { value: methodologies = [] } = useAsyncHandledRetry(
    methodologyService.getMethodologies,
  );

  const { value: topic } = useAsyncHandledRetry(
    () => topicService.getTopic(topicId),
    [topicId],
  );

  const handleSubmit = useFormSubmit(
    async ({
      teamName,
      teamEmail,
      contactName,
      contactTelNo,
      methodologyChoice,
      ...values
    }: FormValues) => {
      if (!topic) {
        return;
      }

      const methodology: Dictionary<
        string | undefined | ExternalMethodology
      > = {
        selectedMethodologyId: undefined,
        externalMethodology: undefined,
      };

      if (methodologyChoice === 'existing') {
        methodology.selectedMethodologyId = values.selectedMethodologyId as string;
      }

      if (methodologyChoice === 'external') {
        methodology.externalMethodology = values.externalMethodology;
      }

      await publicationService.createPublication({
        ...values,
        ...methodology,
        topicId: topic.id,
        contact: {
          teamName,
          teamEmail,
          contactName,
          contactTelNo,
        },
      });

      history.push(dashboardRoute.path);
    },
    errorMappings,
  );

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
          <PageTitle caption={topic?.title} title="Create new publication" />
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

      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          title: '',
          methodologyChoice: 'existing',
          selectedMethodologyId: '',
          externalMethodology: { title: '', url: 'https://' },
          teamName: '',
          teamEmail: '',
          contactName: '',
          contactTelNo: '',
        }}
        validationSchema={Yup.object<FormValues>({
          title: Yup.string().required('Enter a publication title'),
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
              title: Yup.string().required(
                'Enter a external methodology title',
              ),
              url: Yup.string()
                .required('Enter an external methodology URL')
                .url('Enter a valid external methodology URL')
                .test({
                  name: 'currentHostUrl',
                  message:
                    'External methodology URL cannot be for this website',
                  test: (value: string) =>
                    Boolean(value && !value.includes(window.location.host)),
                }),
            }),
          }),
          teamName: Yup.string().required('Enter a team name'),
          teamEmail: Yup.string()
            .required('Enter a team email address')
            .email('Enter a valid team email address'),
          contactName: Yup.string().required('Enter a contact name'),
          contactTelNo: Yup.string().required(
            'Enter a contact telephone number',
          ),
        })}
        onSubmit={handleSubmit}
      >
        {form => {
          return (
            <Form id={formId}>
              <FormFieldTextInput<FormValues>
                id={`${formId}-title`}
                label="Enter publication title"
                name="title"
                className="govuk-!-width-two-thirds"
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
                            methodologies
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
                          id={`${formId}-externalMethodologyTitle`}
                          name="externalMethodology.title"
                          className="govuk-!-width-two-thirds"
                        />
                        <FormFieldTextInput
                          label="URL"
                          id={`${formId}-externalMethodologyUrl`}
                          name="externalMethodology.url"
                          className="govuk-!-width-two-thirds"
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
                        methodologies,
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
                id={`${formId}-contact`}
                legend="Contact for this publication"
                legendSize="m"
                hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
              >
                <FormFieldTextInput<FormValues>
                  name="teamName"
                  id={`${formId}-teamName`}
                  label="Team name"
                  className="govuk-!-width-one-half"
                />

                <FormFieldTextInput<FormValues>
                  name="teamEmail"
                  id={`${formId}-teamEmail`}
                  label="Team email address"
                  className="govuk-!-width-one-half"
                />

                <FormFieldTextInput<FormValues>
                  name="contactName"
                  id={`${formId}-contactName`}
                  label="Contact name"
                  className="govuk-!-width-one-half"
                />

                <FormFieldTextInput<FormValues>
                  name="contactTelNo"
                  id={`${formId}-contactTelNo`}
                  label="Contact telephone number"
                  width={10}
                />
              </FormFieldset>

              <ButtonGroup>
                <Button type="submit">Create publication</Button>

                <Link to={dashboardRoute.path}>Cancel publication</Link>
              </ButtonGroup>
            </Form>
          );
        }}
      </Formik>
    </Page>
  );
};

export default PublicationCreatePage;
