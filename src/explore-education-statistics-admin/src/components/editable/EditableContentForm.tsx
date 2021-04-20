import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { Element } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form } from '@common/components/form';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useCallback } from 'react';

interface FormValues {
  content: string;
}

export interface Props {
  label: string;
  content: string;
  id: string;
  hideLabel?: boolean;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onCancel: () => void;
  onSubmit: (content: string) => void;
}

const EditableContentForm = ({
  id,
  content,
  label,
  hideLabel = false,
  onImageUpload,
  onImageUploadCancel,
  onCancel,
  onSubmit,
}: Props) => {
  const validateElements = useCallback((elements: Element[]) => {
    let error: string | undefined;

    elements.some(element => {
      if (element.name === 'image' && !element.getAttribute('alt')) {
        error = 'All images must have alternative (alt) text';
        return true;
      }

      return false;
    });

    return error;
  }, []);

  return (
    <Formik<FormValues>
      initialValues={{
        content,
      }}
      validateOnChange={false}
      validationSchema={Yup.object({
        content: Yup.string().required('Enter content'),
      })}
      onSubmit={values => {
        onSubmit(values.content);
      }}
    >
      <Form id={`${id}-form`}>
        <FormFieldEditor<FormValues>
          name="content"
          label={label}
          hideLabel={hideLabel}
          focusOnInit
          validateElements={validateElements}
          onImageUpload={onImageUpload}
          onImageUploadCancel={onImageUploadCancel}
        />

        <ButtonGroup>
          <Button type="submit">Save</Button>
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
        </ButtonGroup>
      </Form>
    </Formik>
  );
};

export default EditableContentForm;
