import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormGroup } from '@common/components/form';
import { useFormContext } from '@common/components/form/contexts/FormContext';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { useFormikContext } from 'formik';
import React, { MouseEventHandler } from 'react';
import { InjectedWizardProps } from './Wizard';

interface Props {
  goToPreviousStep: InjectedWizardProps['goToPreviousStep'];
  onPreviousStep?: MouseEventHandler;
  stepNumber: InjectedWizardProps['stepNumber'];
  submitText?: string;
  submittingText?: string;
  onSubmitClick?: MouseEventHandler<HTMLButtonElement>;
}

const WizardStepFormActions = ({
  goToPreviousStep,
  onPreviousStep,
  stepNumber,
  submitText = 'Next step',
  submittingText = 'Submitting',
  onSubmitClick = () => {},
}: Props) => {
  const { formId } = useFormContext();
  const form = useFormikContext();

  return (
    <FormGroup>
      <ButtonGroup>
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
          size="md"
          text="Page is loading"
        />
      </ButtonGroup>
    </FormGroup>
  );
};

export default WizardStepFormActions;
