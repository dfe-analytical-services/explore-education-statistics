import { InjectedWizardProps } from '@frontend/prototypes/table-tool/components/Wizard';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './WizardStep.module.scss';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  hint?: string;
  id?: string;
  title: string;
}

const WizardStep = ({
  children,
  hint,
  title,
  id,
  ...restProps
}: WizardStepProps) => {
  // Hide injected props from public API
  const injectedWizardProps = restProps as InjectedWizardProps;

  const { stepNumber, currentStep, setCurrentStep } = injectedWizardProps;

  const stepEnabled = currentStep > stepNumber;

  return (
    <li
      className={classNames(styles.step, {
        [styles.stepActive]: currentStep === stepNumber,
        [styles.stepEnabled]: stepEnabled,
      })}
      id={id}
    >
      <div className={styles.section}>
        <h2>
          <button
            aria-expanded={false}
            type="button"
            onClick={() => {
              if (stepEnabled) {
                setCurrentStep(stepNumber);
              }
            }}
            className={styles.stepButton}
          >
            <div className={styles.stepNumber} aria-hidden>
              <span className={styles.stepNumberInner}>{stepNumber}</span>
            </div>

            <span className={styles.headingTitle}>
              <span className="govuk-visually-hidden">{`${stepNumber}:`}</span>
              {title}
            </span>

            {hint && <span className="govuk-hint">{hint}</span>}

            {stepEnabled && (
              <span
                className={styles.stepToggleText}
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

        {currentStep === stepNumber && (
          <>
            <div
              className="govuk-grid-row govuk-!-margin-bottom-4"
              id={`${id}-content`}
            >
              <div className="govuk-grid-column-full">
                {typeof children === 'function'
                  ? children(injectedWizardProps)
                  : children}
              </div>
            </div>
          </>
        )}
      </div>
    </li>
  );
};

export default WizardStep;
