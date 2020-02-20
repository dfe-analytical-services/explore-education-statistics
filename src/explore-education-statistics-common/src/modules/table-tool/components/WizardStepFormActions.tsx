import { FormikState } from 'formik';
import React, { MouseEventHandler } from 'react';
import Button from '@common/components/Button';
import { FormGroup } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { InjectedWizardProps } from './Wizard';

interface Props {
  form: FormikState<{}>;
  formId: string;
  goToPreviousStep: InjectedWizardProps['goToPreviousStep'];
  onPreviousStep?: MouseEventHandler;
  stepNumber: InjectedWizardProps['stepNumber'];
  submitText?: string;
  submittingText?: string;
  onSubmitClick?: MouseEventHandler<HTMLButtonElement>;
}

const WizardStepFormActions = ({
  form,
  formId,
  goToPreviousStep,
  onPreviousStep,
  stepNumber,
  submitText = 'Next step',
  submittingText = 'Submitting',
  onSubmitClick = () => {},
}: Props) => {
  return (
    <FormGroup>
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

      <Button
        disabled={form.isSubmitting}
        id={`${formId}-submit`}
        type="submit"
        onClick={onSubmitClick}
      >
        {form.isSubmitting ? submittingText : submitText}
      </Button>

      <LoadingSpinner
        alert
        inline
        hideText
        loading={form.isSubmitting}
        text="Page is loading"
      />
    </FormGroup>
  );
};

export default WizardStepFormActions;
