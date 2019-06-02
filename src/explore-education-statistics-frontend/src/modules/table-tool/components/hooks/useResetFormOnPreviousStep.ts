import { Formik } from '@common/components/form';
import { FormikValues } from 'formik';
import { RefObject, useEffect } from 'react';

/**
 * Reset the form state for this current step if
 * the user moves back to a previous step.
 */
export default function useResetFormOnPreviousStep<
  FormValues extends FormikValues
>(
  formikRef: RefObject<Formik<FormValues>>,
  currentStep: number,
  stepNumber: number,
  callback?: () => void,
) {
  useEffect(() => {
    if (currentStep < stepNumber) {
      if (formikRef.current) {
        formikRef.current.resetForm();
      }

      if (callback) {
        callback();
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formikRef, currentStep, stepNumber]);
}
