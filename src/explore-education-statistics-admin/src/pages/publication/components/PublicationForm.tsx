import FormFieldThemeTopicSelect from '@admin/components/form/FormFieldThemeTopicSelect';
import methodologyService from '@admin/services/methodologyService';
import { ExternalMethodology } from '@admin/services/publicationService';
import themeService from '@admin/services/themeService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormFieldset, FormGroup } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { OmitStrict } from '@common/types';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { ReactNode, useMemo } from 'react';

interface FormValues {
  title: string;
  topicId?: string;
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
  const {
    value: approvedMethodologies = [],
    isLoading: isMethodologiesLoading,
  } = useAsyncHandledRetry(async () => {
    const allMethodologies = await methodologyService.getMethodologies();
    return allMethodologies.filter(
      methodology => methodology.status !== 'Draft',
    );
  });

  const {
    value: themes = [],
    isLoading: isThemesLoading,
  } = useAsyncHandledRetry(themeService.getThemes);

  const initialMethodologyChoice = useMemo<
    FormValues['methodologyChoice']
  >(() => {
    if (initialValues?.methodologyId) {
      return 'existing';
    }

    if (initialValues?.externalMethodology) {
      return 'external';
    }

    return initialValues ? 'none' : 'existing';
  }, [initialValues]);

  const validationSchema = useMemo(() => {
    const schema = Yup.object<FormValues>({
      title: Yup.string().required('Enter a publication title'),
      methodologyChoice: Yup.mixed<FormValues['methodologyChoice']>()
        .oneOf(['external', 'existing', 'none'])
        .required('Choose a methodology'),
      methodologyId: Yup.string().when('methodologyChoice', {
        is: 'existing',
        then: Yup.string()
          .oneOf(
            approvedMethodologies.map(m => m.id),
            'Choose a methodology',
          )
          .required('Choose a methodology'),
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
                message: 'External methodology URL cannot be for this website',
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
    });

    if (initialValues?.topicId) {
      return schema.shape({
        topicId: Yup.string().required('Choose a topic'),
      });
    }

    return schema;
  }, [initialValues?.topicId, approvedMethodologies]);

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

  if (isThemesLoading || isMethodologiesLoading) {
    return <LoadingSpinner />;
  }

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
        methodologyChoice: initialMethodologyChoice,
      }}
      validationSchema={validationSchema}
      onSubmit={handleSubmit}
    >
      {form => (
        <Form id={id}>
          <FormFieldTextInput<FormValues>
            label="Publication title"
            name="title"
            className="govuk-!-width-two-thirds"
          />

          {initialValues?.topicId && (
            <FormFieldThemeTopicSelect<FormValues>
              name="topicId"
              legend="Choose a topic for this publication"
              legendSize="m"
              id={id}
              themes={themes}
            />
          )}

          <FormFieldRadioGroup<FormValues, FormValues['methodologyChoice']>
            legend="Choose a methodology for this publication"
            legendSize="m"
            name="methodologyChoice"
            options={[
              {
                value: 'existing',
                label: 'Choose an existing methodology',
                conditional: (
                  <FormFieldSelect<FormValues>
                    name="methodologyId"
                    label="Select methodology"
                    placeholder="Choose a methodology"
                    options={orderBy(
                      approvedMethodologies.map(methodology => ({
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
                      name="externalMethodology.title"
                      className="govuk-!-width-two-thirds"
                    />
                    <FormFieldTextInput
                      label="URL"
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
                      approvedMethodologies,
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
            id="contact"
            legend="Contact for this publication"
            legendSize="m"
            hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
          >
            <FormFieldTextInput<FormValues>
              name="teamName"
              label="Team name"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<FormValues>
              name="teamEmail"
              label="Team email address"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<FormValues>
              name="contactName"
              label="Contact name"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<FormValues>
              name="contactTelNo"
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
