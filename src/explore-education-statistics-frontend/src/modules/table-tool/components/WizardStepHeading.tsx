import { InjectedWizardProps } from '@frontend/modules/table-tool/components/Wizard';
import styles from '@frontend/modules/table-tool/components/WizardStepHeading.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fieldsetHeading?: boolean;
}

const WizardStepHeading = ({
  children,
  currentStep,
  fieldsetHeading = false,
  isActive,
  stepNumber,
  setCurrentStep,
}: Props & InjectedWizardProps) => {
  const stepEnabled = currentStep > stepNumber;

  return (
    <>
      {isActive ? (
        <h2
          className={classNames({
            'govuk-fieldset__heading': fieldsetHeading,
          })}
        >
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
