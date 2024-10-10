import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import {
  Form,
  FormFieldCheckbox,
  FormFieldset,
  FormFieldTextInput,
} from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  agreeTerms: boolean;
  label: string;
}

interface Props {
  onCancel: () => void;
  onSubmit: (label: string) => void;
}

export default function ApiDataSetPreviewTokenCreateForm({
  onCancel,
  onSubmit,
}: Props) {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      label: Yup.string().required('Enter a token name'),
      agreeTerms: Yup.boolean()
        .required('The terms of usage must be agreed')
        .oneOf([true], 'The terms of usage must be agreed'),
    });
  }, []);
  return (
    <FormProvider validationSchema={validationSchema}>
      {({ formState }) => {
        return (
          <Form
            id="apiDataSetTokenCreateForm"
            onSubmit={values => onSubmit(values.label)}
          >
            <FormFieldTextInput<FormValues>
              name="label"
              label="Token name"
              hint="Add a name so you can easily reference this token"
            />
            <FormFieldset
              error={formState.errors?.agreeTerms?.message}
              id="terms"
              legend="Terms of usage"
              legendSize="s"
            >
              <FormFieldCheckbox
                name="agreeTerms"
                label="I agree to only share this token with individuals that have been granted production access to the unpublished data"
              />
            </FormFieldset>

            <ButtonGroup>
              <Button type="submit" ariaDisabled={formState.isSubmitting}>
                Continue
              </Button>
              <ButtonText
                ariaDisabled={formState.isSubmitting}
                onClick={onCancel}
              >
                Cancel
              </ButtonText>
              <LoadingSpinner
                alert
                hideText
                inline
                loading={formState.isSubmitting}
                size="sm"
                text="Creating new preview token"
              />
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}
