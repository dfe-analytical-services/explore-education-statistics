import {
  FieldValues,
  FormProvider as RHFormProvider,
  useForm,
  UseFormProps,
  UseFormReturn,
} from 'react-hook-form';
import React, { ReactNode, useEffect, useRef } from 'react';
import { Schema } from 'yup';
import { yupResolver } from '@hookform/resolvers/yup';
import isEqual from 'lodash/isEqual';

interface FormProviderProps<TFormValues extends FieldValues> {
  children: ReactNode | ((form: UseFormReturn<TFormValues>) => ReactNode);
  enableReinitialize?: boolean;
  initialValues?: UseFormProps<TFormValues>['defaultValues'];
  validationSchema?: Schema<TFormValues>;
}

export default function FormProvider<TFormValues extends FieldValues>({
  children,
  enableReinitialize,
  initialValues,
  validationSchema,
}: FormProviderProps<TFormValues>) {
  const form = useForm<TFormValues>({
    defaultValues: initialValues,
    resolver: validationSchema ? yupResolver(validationSchema) : undefined,
  });

  const previousInitialValues = useRef(initialValues);

  /**
   * RHF caches default values and doesn't have a built in option
   * to reinitialise forms.
   * This use effect provides the equivalent functionality as
   * Formik's `enableReinitialize` by checking for changes to the
   * initialValues and reseting the form with the new values if they've changed.
   */
  useEffect(() => {
    if (
      enableReinitialize &&
      !isEqual(previousInitialValues.current, initialValues)
    ) {
      form.reset(initialValues as TFormValues);
      previousInitialValues.current = initialValues;
    }
  }, [enableReinitialize, form, initialValues]);

  return (
    <RHFormProvider {...form}>
      {typeof children === 'function' ? children(form) : children}
    </RHFormProvider>
  );
}
