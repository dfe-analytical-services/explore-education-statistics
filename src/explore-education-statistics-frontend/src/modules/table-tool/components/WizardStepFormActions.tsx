import Button from '@common/components/Button';
import { FormGroup } from '@common/components/form';
import { FormikState } from 'formik';
import React, { MouseEventHandler } from 'react';
import { InjectedWizardProps } from './Wizard';

interface Props {
  form: FormikState<{}>;
  formId: string;
  goToPreviousStep: InjectedWizardProps['goToPreviousStep'];
  onPreviousStep?: MouseEventHandler;
  stepNumber: InjectedWizardProps['stepNumber'];
  submitText?: string;
  submittingText?: string;
  submitOnClick?: (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => void;
}

const WizardStepFormActions = ({
  form,
  formId,
  goToPreviousStep,
  onPreviousStep,
  stepNumber,
  submitText = 'Next step',
  submittingText = 'Submitting',
  submitOnClick = () => {},
}: Props) => {
  return (
    <FormGroup>
      <Button
        disabled={form.isSubmitting}
        id={`${formId}-submit`}
        type="submit"
        onClick={submitOnClick}
      >
        {form.isSubmitting ? submittingText : submitText}
      </Button>

      {stepNumber > 1 && (
        <Button
          type="button"
          variant="secondary"
          onClick={event => {
            if (onPreviousStep) {
              onPreviousStep(event);
            }

            if (!event.isDefaultPrevented()) {
              goToPreviousStep();
            }
          }}
        >
          Previous step
        </Button>
      )}
    </FormGroup>
  );
};

export default WizardStepFormActions;
