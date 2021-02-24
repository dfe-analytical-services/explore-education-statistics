import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form } from '@common/components/form';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

interface FormValues {
  content: string;
}

export interface Props {
  label: string;
  content: string;
  id: string;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onCancel: () => void;
  onSubmit: (content: string) => void;
}

const EditableContentForm = ({
  id,
  content,
  label,
  onImageUpload,
  onImageUploadCancel,
  onCancel,
  onSubmit,
}: Props) => {
  return (
    <Formik<FormValues>
      initialValues={{
        content,
      }}
      validationSchema={Yup.object<FormValues>({
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
          focusOnInit
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
