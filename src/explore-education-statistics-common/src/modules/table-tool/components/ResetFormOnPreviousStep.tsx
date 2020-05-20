import useMounted from '@common/hooks/useMounted';
import { useFormikContext } from 'formik';

interface Props {
  currentStep: number;
  stepNumber: number;
}

/**
 * Reset the form state for this current step if
 * the user moves back to a previous step.
 */
function ResetFormOnPreviousStep({ currentStep, stepNumber }: Props) {
  const formik = useFormikContext();

  useMounted(() => {
    if (currentStep < stepNumber) {
      formik.resetForm();
    }
  });

  return null;
}

export default ResetFormOnPreviousStep;
