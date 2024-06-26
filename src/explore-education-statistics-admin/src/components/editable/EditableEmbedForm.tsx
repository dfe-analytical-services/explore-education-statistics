import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Yup from '@common/validation/yup';
import { mapFieldErrors } from '@common/validation/serverValidations';
import { useConfig } from '@admin/contexts/ConfigContext';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

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
  const { permittedEmbedUrlDomains } = useConfig();

  const validationSchema = useMemo<
    ObjectSchema<EditableEmbedFormValues>
  >(() => {
    return Yup.object({
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
                permittedEmbedUrlDomains.some(domain =>
                  value.startsWith(domain),
                ),
            ),
        }),
    });
  }, [permittedEmbedUrlDomains]);

  return (
    <FormProvider
      errorMappings={errorMappings}
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      <Form id={formId} onSubmit={onSubmit}>
        <FormFieldTextInput<EditableEmbedFormValues>
          name="title"
          hint="This will show to users of assistive technology, it does not show on the release page"
          label="Title"
        />

        <FormFieldTextInput<EditableEmbedFormValues>
          name="url"
          hint="Embedded dashboards must be hosted on the DfE Shiny apps domain (https://department-for-education.shinyapps.io/)"
          label="URL"
        />
        <ButtonGroup>
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit">Save</Button>
        </ButtonGroup>
      </Form>
    </FormProvider>
  );
};

export default EditableEmbedForm;
