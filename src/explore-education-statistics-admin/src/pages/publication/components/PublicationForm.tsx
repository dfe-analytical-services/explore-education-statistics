import FormFieldThemeTopicSelect from '@admin/components/form/FormFieldThemeTopicSelect';
import publicationService from '@admin/services/publicationService';
import themeService from '@admin/services/themeService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import {
  FormFieldSelect,
  FormFieldset,
  FormFieldTextArea,
} from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { ReactNode, useMemo } from 'react';

export interface FormValues {
  title: string;
  summary: string;
  topicId?: string;
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo?: string;
  supersededById?: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'title',
    messages: {
      SlugNotUnique: 'Choose a unique title',
    },
  }),
];

interface Props {
  cancelButton?: ReactNode;
  id?: string;
  initialValues?: FormValues;
  publicationId?: string;
  showSupersededBy?: boolean;
  showTitleInput?: boolean;
  onSubmit: (values: FormValues) => void;
}

const PublicationForm = ({
  cancelButton,
  id = 'publicationForm',
  initialValues,
  publicationId,
  showSupersededBy = false,
  showTitleInput = false,
  onSubmit,
}: Props) => {
  const { value, isLoading: isThemesLoading } = useAsyncHandledRetry(
    async () => {
      const themes = await themeService.getThemes();
      if (!showSupersededBy) {
        return { themes };
      }

      const allPublications =
        await publicationService.getPublicationSummaries();
      const publications = allPublications.filter(
        publication => publication.id !== publicationId,
      );

      return {
        themes,
        publications,
      };
    },
  );

  const { themes, publications } = value ?? {};

  const validationSchema = useMemo(() => {
    const schema = Yup.object<FormValues>({
      title: Yup.string().required('Enter a publication title'),
      summary: Yup.string()
        .required('Enter a publication summary')
        .max(160, 'Summary must be 160 characters or less'),
      teamName: Yup.string().required('Enter a team name'),
      teamEmail: Yup.string()
        .required('Enter a team email address')
        .email('Enter a valid team email address'),
      contactName: Yup.string().required('Enter a contact name'),
      contactTelNo: Yup.string()
        .trim()
        .matches(
          /^[0-9 \t]*$/,
          'The telephone number should only contain numeric characters',
        )
        .matches(
          /^(?!^0[ \t]*3[ \t]*7[ \t]*0[ \t]*0[ \t]*0[ \t]*0[ \t]*2[ \t]*2[ \t]*8[ \t]*8$)/,
          'The DfE enquiries number is not suitable for use on statistics publications',
        )
        .min(8, 'The telephone number must be at least 8 characters long'),
      supersededById: Yup.string(),
    });

    if (initialValues?.topicId) {
      return schema.shape({
        topicId: Yup.string().required('Choose a topic'),
      });
    }
    return schema;
  }, [initialValues?.topicId]);

  const handleSubmit = useFormSubmit(async (values: FormValues) => {
    await onSubmit(values);
  }, errorMappings);

  if (isThemesLoading) {
    return <LoadingSpinner />;
  }

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        ...(initialValues ?? {
          title: '',
          summary: '',
          teamName: '',
          teamEmail: '',
          contactName: '',
          contactTelNo: '',
        }),
      }}
      validationSchema={validationSchema}
      onSubmit={handleSubmit}
    >
      {() => (
        <Form id={id}>
          {showTitleInput && (
            <FormFieldTextInput<FormValues>
              label="Publication title"
              name="title"
              className="govuk-!-width-two-thirds"
            />
          )}

          <FormFieldTextArea<FormValues>
            label="Publication summary"
            name="summary"
            className="govuk-!-width-one-half"
            maxLength={160}
          />

          {themes && initialValues?.topicId && (
            <FormFieldThemeTopicSelect<FormValues>
              name="topicId"
              legend="Choose a topic for this publication"
              legendSize="m"
              id={id}
              themes={themes}
            />
          )}

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
              label="Contact telephone number (optional)"
              width={10}
            />
          </FormFieldset>

          {publications && showSupersededBy && (
            <FormFieldset
              id="supersede"
              legend="Archive this publication"
              legendSize="m"
            >
              <FormFieldSelect<FormValues>
                label="Superseding publication"
                hint="If superseded by a publication with a live release, this will archive the current publication immediately"
                name="supersededById"
                options={publications.map(publication => ({
                  label: publication.title,
                  value: publication.id,
                }))}
                placeholder="None selected"
              />
            </FormFieldset>
          )}

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
