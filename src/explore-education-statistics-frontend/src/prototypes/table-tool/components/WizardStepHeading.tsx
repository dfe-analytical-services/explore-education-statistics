import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { InjectedWizardProps } from './Wizard';
import styles from './WizardStepHeading.module.scss';

interface Props {
  children: ReactNode;
}

const WizardStepHeading = ({
  children,
  currentStep,
  isActive,
  stepNumber,
  setCurrentStep,
}: Props & InjectedWizardProps) => {
  const stepEnabled = currentStep > stepNumber;

  return (
    <>
      {isActive ? (
        <h2 className="govuk-fieldset__heading">
          <span className="govuk-visually-hidden">{`Step ${stepNumber}:`}</span>
          {children}
        </h2>
      ) : (
        <h2
          className={classNames({
            [styles.stepEnabled]: stepEnabled,
          })}
        >
          <button
            type="button"
            onClick={() => {
              if (stepEnabled) {
                setCurrentStep(stepNumber);
              }
            }}
            className={styles.stepButton}
          >
            <span className="govuk-visually-hidden">{`Step ${stepNumber}:`}</span>
            {children}

            {stepEnabled && (
              <span
                className={styles.toggleText}
                aria-hidden
                onClick={() => {
                  setCurrentStep(stepNumber);
                }}
              >
                Go to this step
              </span>
            )}
          </button>
        </h2>
      )}
    </>
  );
};

export default WizardStepHeading;
