import { useConfirmContext } from '@common/context/ConfirmContext';
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
  size = 'm',
  stepNumber,
  setCurrentStep,
}: Props & InjectedWizardProps) => {
  const stepEnabled = currentStep > stepNumber;

  const { askConfirm } = useConfirmContext();

  return (
    <>
      {isActive ? (
        <h2
          className={classNames(`govuk-heading-${size}`, {
            'govuk-fieldset__heading': fieldsetHeading,
          })}
        >
          <span className="govuk-visually-hidden">{`Step ${stepNumber}:`}</span>
          {children}
        </h2>
      ) : (
        <h2
          className={classNames(`govuk-heading-${size}`, {
            [styles.stepEnabled]: stepEnabled,
          })}
        >
          <button
            type="button"
            onClick={() => {
              askConfirm(() => {
                setCurrentStep(stepNumber);
              });
            }}
            className={styles.stepButton}
          >
            <span className="govuk-visually-hidden">{`Step ${stepNumber}:`}</span>
            {children}

            {stepEnabled && (
              <span className={styles.toggleText} aria-hidden>
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
