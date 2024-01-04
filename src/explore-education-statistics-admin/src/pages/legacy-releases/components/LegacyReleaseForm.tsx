import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldNumberInput from '@common/components/form/rhf/RHFFormFieldNumberInput';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import Yup from '@common/validation/yup';
import React, { ReactNode } from 'react';

interface FormValues {
  description: string;
  url: string;
  order?: number;
}

const formId = 'legacyReleaseForm';

interface Props {
  cancelButton?: ReactNode;
  initialValues?: FormValues;
  onSubmit: (values: FormValues) => void;
}

const LegacyReleaseForm = ({
  cancelButton,
  initialValues = {
    description: '',
    url: '',
  },
  onSubmit,
}: Props) => {
  return (
    <FormProvider
      initialValues={initialValues}
      validationSchema={Yup.object<FormValues>({
        description: Yup.string().required('Enter a description'),
        url: Yup.string().required('Enter a URL').url('Enter a valid URL'),
        order:
          typeof initialValues?.order === 'number'
            ? Yup.number()
                .required('Enter an order')
                .min(0, 'Enter an order greater than 0')
            : Yup.number(),
      })}
    >
      {({ formState }) => {
        return (
          <RHFForm id={formId} onSubmit={onSubmit}>
            <RHFFormFieldTextInput<FormValues>
              name="description"
              label="Description"
              className="govuk-!-width-two-thirds"
            />

            <RHFFormFieldTextInput<FormValues>
              name="url"
              label="URL"
              className="govuk-!-width-two-thirds"
            />

            {typeof initialValues?.order !== 'undefined' && (
              <RHFFormFieldNumberInput<FormValues>
                name="order"
                label="Order"
                width={2}
              />
            )}

            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                Save legacy release
              </Button>
              {cancelButton}
            </ButtonGroup>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
};

export default LegacyReleaseForm;
