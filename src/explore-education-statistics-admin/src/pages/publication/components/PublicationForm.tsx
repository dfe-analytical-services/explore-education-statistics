import Link from '@admin/components/Link';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { dashboardRoute } from '@admin/routes/routes';
import { ExternalMethodology } from '@admin/services/dashboardService';
import methodologyService from '@admin/services/methodologyService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormFieldset, FormGroup } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { OmitStrict } from '@common/types';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { ReactNode } from 'react';

interface FormValues {
  title: string;
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
  methodologyChoice: 'existing' | 'external' | 'none';
  methodologyId?: string;
  externalMethodology?: ExternalMethodology;
}

export type PublicationFormValues = OmitStrict<FormValues, 'methodologyChoice'>;

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'title',
    messages: {
      SLUG_NOT_UNIQUE: 'Choose a unique title',
    },
  }),
  mapFieldErrors<FormValues>({
    target: 'methodologyId',
    messages: {
      METHODOLOGY_DOES_NOT_EXIST:
        'There was a problem adding the selected methodology',
      METHODOLOGY_MUST_BE_APPROVED_OR_PUBLISHED:
        'Choose a methodology that is Live or ready to be published',
    },
  }),
  mapFieldErrors<FormValues>({
    target: 'methodologyChoice',
    messages: {
      METHODOLOGY_OR_EXTERNAL_METHODOLOGY_LINK_MUST_BE_DEFINED:
        'Either an existing methodology or an external methodology link must be provided',
      CANNOT_SPECIFY_METHODOLOGY_AND_EXTERNAL_METHODOLOGY:
        'Either an existing methodology or an external methodology link must be provided',
    },
  }),
];

interface Props {
  cancelButton?: ReactNode;
  id?: string;
  initialValues?: PublicationFormValues;
  onSubmit: (values: PublicationFormValues) => void;
}

const PublicationForm = ({
  cancelButton,
  id = 'publicationForm',
  initialValues,
  onSubmit,
}: Props) => {
  const { value: methodologies = [] } = useAsyncHandledRetry(
    methodologyService.getMethodologies,
  );

  const handleSubmit = useFormSubmit(
    async ({
      methodologyChoice,
      methodologyId,
      externalMethodology,
      ...values
    }: FormValues) => {
      switch (methodologyChoice) {
        case 'existing':
          await onSubmit({ ...values, methodologyId });
          break;
        case 'external':
          await onSubmit({ ...values, externalMethodology });
          break;
        default:
          await onSubmit(values);
      }
    },
    errorMappings,
  );

  const initialMethodologyChoice = (): FormValues['methodologyChoice'] => {
    if (initialValues?.methodologyId) {
      return 'existing';
    }

    if (initialValues?.externalMethodology) {
      return 'external';
    }

    return initialValues ? 'none' : 'existing';
  };

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        ...(initialValues ?? {
          title: '',
          methodologyId: '',
          externalMethodology: {
            title: '',
            url: 'https://',
          },
          teamName: '',
          teamEmail: '',
          contactName: '',
          contactTelNo: '',
        }),
        methodologyChoice: initialMethodologyChoice(),
      }}
      validationSchema={Yup.object<FormValues>({
        title: Yup.string().required('Enter a publication title'),
        methodologyChoice: Yup.mixed<FormValues['methodologyChoice']>()
          .oneOf(['external', 'existing', 'none'])
          .required('Choose a methodology'),
        methodologyId: Yup.string().when('methodologyChoice', {
          is: 'existing',
          then: Yup.string().required('Choose a methodology'),
          otherwise: Yup.string(),
        }),
        externalMethodology: Yup.object<ExternalMethodology>().when(
          'methodologyChoice',
          {
            is: 'external',
            then: Yup.object().shape({
              title: Yup.string().required(
                'Enter an external methodology link title',
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
          },
        ),
        teamName: Yup.string().required('Enter a team name'),
        teamEmail: Yup.string()
          .required('Enter a team email address')
          .email('Enter a valid team email address'),
        contactName: Yup.string().required('Enter a contact name'),
        contactTelNo: Yup.string().required('Enter a contact telephone number'),
      })}
      onSubmit={handleSubmit}
    >
      {form => (
        <Form id={id}>
          <FormFieldTextInput<FormValues>
            id={`${id}-title`}
            label="Publication title"
            name="title"
            className="govuk-!-width-two-thirds"
          />

          <FormFieldRadioGroup<FormValues, FormValues['methodologyChoice']>
            id={`${id}-methodologyChoice`}
            legend="Choose a methodology for this publication"
            legendSize="m"
            name="methodologyChoice"
            options={[
              {
                value: 'existing',
                label: 'Choose an existing methodology',
                conditional: (
                  <FormFieldSelect<FormValues>
                    id={`${id}-methodologyId`}
                    name="methodologyId"
                    label="Select methodology"
                    placeholder="Choose a methodology"
                    options={orderBy(
                      methodologies
                        .filter(methodology => methodology.status !== 'Draft')
                        .map(methodology => ({
                          label: `${methodology.title} [${methodology.status}]`,
                          value: methodology.id,
                        })),
                      'label',
                    )}
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
                      id={`${id}-externalMethodologyTitle`}
                      name="externalMethodology.title"
                      className="govuk-!-width-two-thirds"
                    />
                    <FormFieldTextInput
                      label="URL"
                      id={`${id}-externalMethodologyUrl`}
                      name="externalMethodology.url"
                      className="govuk-!-width-two-thirds"
                    />
                  </FormGroup>
                ),
              },
              {
                value: 'none',
                label: 'No methodology',
              },
            ]}
            onChange={e => {
              switch (e.target.value) {
                case 'existing':
                  form.setValues({
                    ...form.values,
                    methodologyId: orderBy(
                      methodologies,
                      methodology => methodology.title,
                    )[0]?.id,
                    externalMethodology: {
                      title: '',
                      url: '',
                    },
                  });
                  break;
                case 'external':
                  form.setValues({
                    ...form.values,
                    methodologyId: '',
                  });
                  break;
                default:
                  form.setValues({
                    ...form.values,
                    methodologyId: '',
                    externalMethodology: {
                      title: '',
                      url: '',
                    },
                  });
                  break;
              }
            }}
          />
          <FormFieldset
            id={`${id}-contact`}
            legend="Contact for this publication"
            legendSize="m"
            hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
          >
            <FormFieldTextInput<FormValues>
              name="teamName"
              id={`${id}-teamName`}
              label="Team name"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<FormValues>
              name="teamEmail"
              id={`${id}-teamEmail`}
              label="Team email address"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<FormValues>
              name="contactName"
              id={`${id}-contactName`}
              label="Contact name"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<FormValues>
              name="contactTelNo"
              id={`${id}-contactTelNo`}
              label="Contact telephone number"
              width={10}
            />
          </FormFieldset>

          <ButtonGroup>
            <Button type="submit">Save publication</Button>
            {cancelButton}
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default PublicationForm;
