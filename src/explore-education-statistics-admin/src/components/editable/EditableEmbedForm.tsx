import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextInput } from '@common/components/form';
import useFormSubmit from '@common/hooks/useFormSubmit';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';
import { mapFieldErrors } from '@common/validation/serverValidations';
import { useConfig } from '@admin/contexts/ConfigContext';

export interface EditableEmbedFormValues {
  title: string;
  url: string;
}

const formId = 'embedBlockForm';

interface Props {
  initialValues?: EditableEmbedFormValues;
  onCancel: () => void;
  onSubmit: (values: EditableEmbedFormValues) => void;
}

const errorMappings = [
  mapFieldErrors<EditableEmbedFormValues>({
    target: 'url',
    messages: {
      EmbedBlockUrlNotPermitted: 'URL must be on a permitted domain',
    },
  }),
];

const EditableEmbedForm = ({
  initialValues = { title: '', url: '' },
  onCancel,
  onSubmit,
}: Props) => {
  const [previewValues, setPreviewValues] =
    useState<EditableEmbedFormValues>(initialValues);

  const { PermittedEmbedUrlDomains } = useConfig();

  return (
    <Formik<EditableEmbedFormValues>
      initialValues={initialValues}
      validationSchema={Yup.object<EditableEmbedFormValues>({
        title: Yup.string().required('Enter a title'),
        url: Yup.string()
          .required('Enter a URL')
          .url('Enter a valid URL')
          .test({
            name: 'allowedDomain',
            message: 'URL must be on a permitted domain',
            test: (value: string) =>
              Boolean(
                value &&
                  PermittedEmbedUrlDomains.some(domain =>
                    value.startsWith(domain),
                  ),
              ),
          }),
      })}
      onSubmit={useFormSubmit<EditableEmbedFormValues>(onSubmit, errorMappings)}
    >
      {form => (
        <Form id={formId}>
          <FormFieldTextInput<EditableEmbedFormValues>
            name="title"
            hint="This will show to users of assistive technology, it does not show on the release page"
            label="Title"
            onChange={event => {
              setPreviewValues({
                ...previewValues,
                title: event.target.value,
              });
            }}
          />

          <FormFieldTextInput<EditableEmbedFormValues>
            name="url"
            hint="Embedded dashboards must be hosted on the DfE Shiny apps domain (https://department-for-education.shinyapps.io/)"
            label="URL"
            onChange={event => {
              if (form.isValid) {
                setPreviewValues({
                  ...previewValues,
                  url: event.target.value,
                });
              }
            }}
          />
          <ButtonGroup>
            <Button variant="secondary" onClick={onCancel}>
              Cancel
            </Button>
            <Button type="submit">Save</Button>
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default EditableEmbedForm;
