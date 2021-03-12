import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { InjectedWizardProps } from './Wizard';
import styles from './WizardStepHeading.module.scss';

interface Props {
  children: ReactNode;
  fieldsetHeading?: boolean;
  size?: 'xl' | 'l' | 'm' | 's';
}

const WizardStepHeading = ({
  children,
  currentStep,
  fieldsetHeading = false,
  isActive,
  size = 's',
  stepNumber,
  setCurrentStep,
}: Props & InjectedWizardProps) => {
  const stepEnabled = currentStep > stepNumber;

  return (
    <>
      {isActive ? (
        <h2
          className={classNames(`govuk-heading-l`, {
            'govuk-fieldset__heading': fieldsetHeading,
          })}
        >
          <span className="govuk-visually-hidden">{`Step ${stepNumber}: `}</span>
          {children}
        </h2>
      ) : (
        <>
          <h2
            className={classNames(
              `govuk-heading-${size}`,
              {
                [styles.stepEnabled]: stepEnabled,
              },
              'govuk-!-margin-bottom-0',
            )}
          >
            <span>{`Step ${stepNumber}: `}</span>
            {children}
          </h2>
          <button
            data-testid={`wizardStep-${stepNumber}-goToButton`}
            type="button"
            onClick={() => setCurrentStep(stepNumber)}
            className="govuk-button govuk-button--secondary govuk-!-margin-bottom-3 govuk-!-margin-top-1"
          >
            {stepEnabled && <>Edit this step</>}
          </button>
        </>
      )}
    </>
  );
};

export default WizardStepHeading;
