import React from 'react';
import { InjectedWizardProps } from './Wizard';
import styles from './WizardStepEditButton.module.scss';

interface Props {
  editTitle: string;
}

const WizardStepEditButton = ({
  editTitle,
  stepNumber,
  setCurrentStep,
}: Props & InjectedWizardProps) => {
  return (
    <div className="govuk-!-margin-top-2">
      <button
        type="button"
        data-testid={`wizardStep-${stepNumber}-goToButton`}
        onClick={() => setCurrentStep(stepNumber)}
        className={styles.stepButton}
      >
        {editTitle}{' '}
        <span className="govuk-visually-hidden">{`Step ${stepNumber}`}</span>
      </button>
    </div>
  );
};

export default WizardStepEditButton;
