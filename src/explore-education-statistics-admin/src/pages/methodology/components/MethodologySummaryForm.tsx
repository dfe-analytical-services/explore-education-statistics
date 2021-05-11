import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

export interface MethodologySummaryFormValues {
  title: string;
}

const errorMappings = [
  mapFieldErrors<MethodologySummaryFormValues>({
    target: 'title',
    messages: {
      SLUG_NOT_UNIQUE: 'Choose a unique title',
    },
  }),
];

interface Props {
  id: string;
  initialValues?: MethodologySummaryFormValues;
  submitText: string;
  onCancel: () => void;
  onSubmit: (values: MethodologySummaryFormValues) => void;
}

const MethodologySummaryForm = ({
  id,
  initialValues,
  submitText,
  onCancel,
  onSubmit,
}: Props) => {
  const handleSubmit = useFormSubmit<MethodologySummaryFormValues>(
    async values => {
      onSubmit(values as MethodologySummaryFormValues);
    },
    errorMappings,
  );

  return (
    <Formik<MethodologySummaryFormValues>
      enableReinitialize
      initialValues={
        initialValues ??
        ({
          title: '',
        } as MethodologySummaryFormValues)
      }
      validationSchema={Yup.object<MethodologySummaryFormValues>({
        title: Yup.string().required('Enter a methodology title'),
      })}
      onSubmit={handleSubmit}
    >
      <Form id={id}>
        <FormFieldTextInput<MethodologySummaryFormValues>
          label="Enter methodology title"
          name="title"
        />
        <ButtonGroup>
          <Button type="submit">{submitText}</Button>

          <ButtonText onClick={onCancel}>Cancel</ButtonText>
        </ButtonGroup>
      </Form>
    </Formik>
  );
};

export default MethodologySummaryForm;
