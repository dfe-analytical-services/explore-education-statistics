import useMounted from '@common/hooks/useMounted';

interface Props {
  currentStep: number;
  stepNumber: number;
  onReset: () => void;
}

/**
 * Reset the form state for this current step if
 * the user moves back to a previous step.
 */
function RHFResetFormOnPreviousStep({
  currentStep,
  stepNumber,
  onReset,
}: Props) {
  useMounted(() => {
    if (currentStep < stepNumber) {
      onReset();
    }
  });

  return null;
}

export default RHFResetFormOnPreviousStep;
