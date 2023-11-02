import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  title: string;
  titleType: 'default' | 'alternative';
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
  defaultTitle: string;
  id: string;
  initialValues: FormValues;
  submitText: string;
  onCancel: () => void;
  onSubmit: (title: string) => Promise<void> | void;
}

const MethodologySummaryForm = ({
  defaultTitle,
  id,
  initialValues,
  submitText,
  onCancel,
  onSubmit,
}: Props) => {
  const handleSubmit = async (values: FormValues) => {
    await onSubmit(values.title);
  };

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      title: Yup.string().required('Enter a methodology title'),
      titleType: Yup.string().oneOf(['default', 'alternative']).required(),
    });
  }, []);

  return (
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      {form => {
        return (
          <RHFForm id={id} onSubmit={handleSubmit}>
            <RHFFormFieldRadioGroup<FormValues>
              legend="Methodology title"
              name="titleType"
              order={[]}
              onChange={event => {
                if (event.target.value === 'default') {
                  form.setValue('title', defaultTitle);
                }
              }}
              options={[
                {
                  label: 'Use publication title',
                  value: 'default',
                },
                {
                  label: 'Set an alternative title',
                  value: 'alternative',
                  conditional: (
                    <RHFFormFieldTextInput<FormValues>
                      label="Enter methodology title"
                      name="title"
                    />
                  ),
                },
              ]}
            />
            <ButtonGroup>
              <Button type="submit">{submitText}</Button>
              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
};

export default MethodologySummaryForm;
