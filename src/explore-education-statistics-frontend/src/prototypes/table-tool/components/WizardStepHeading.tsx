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
        <h2
          className={classNames('govuk-fieldset__heading', styles.stepActive)}
        >
          <span className={styles.number} aria-hidden>
            <span className={styles.numberInner}>{stepNumber}</span>
          </span>

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
            <span className={styles.number} aria-hidden>
              <span className={styles.numberInner}>{stepNumber}</span>
            </span>

            <span>
              <span className="govuk-visually-hidden">{`Step ${stepNumber}:`}</span>
              {children}
            </span>

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
