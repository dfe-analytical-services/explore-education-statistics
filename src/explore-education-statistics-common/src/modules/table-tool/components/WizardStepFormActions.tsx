import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormGroup } from '@common/components/form';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { MouseEventHandler } from 'react';
import { InjectedWizardProps } from './Wizard';

interface Props extends InjectedWizardProps {
  isSubmitting?: boolean;
  submitText?: string;
  submittingText?: string;
  onPreviousStep?: MouseEventHandler<HTMLButtonElement>;
  onSubmit?: MouseEventHandler<HTMLButtonElement>;
}

const WizardStepFormActions = ({
  goToPreviousStep,
  isSubmitting = false,
  loadingStep,
  stepNumber,
  submitText = 'Next step',
  submittingText = 'Submitting',
  onPreviousStep,
  onSubmit,
}: Props) => {
  const { formId } = useFormIdContext();
  const loading = typeof loadingStep !== 'undefined' || isSubmitting;
  const isLoadingNextStep = (loadingStep ?? stepNumber) > stepNumber;

  return (
    <FormGroup>
      <ButtonGroup>
        {stepNumber > 1 && (
          <Button
            disabled={loading}
            type="button"
            variant="secondary"
            onClick={async event => {
              if (onPreviousStep) {
                await onPreviousStep(event);
              }

              if (!event.isDefaultPrevented()) {
                await goToPreviousStep();
              }
            }}
          >
            Previous step
          </Button>
        )}

        <Button
          disabled={loading}
          id={`${formId}-submit`}
          type="submit"
          onClick={onSubmit}
        >
          {isSubmitting ? submittingText : submitText}
        </Button>

        <LoadingSpinner
          // We trigger another loading spinner that alerts
          // in the `WizardStepSummary` component, so we
          // don't need to this one to alert as well.
          alert={isLoadingNextStep}
          className="govuk-!-margin-left-2"
          inline
          hideText
          loading={loading}
          size="md"
          text={
            isLoadingNextStep ? 'Loading next step' : 'Loading previous step'
          }
        />
      </ButtonGroup>
    </FormGroup>
  );
};

export default WizardStepFormActions;
