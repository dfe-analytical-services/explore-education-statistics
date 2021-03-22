import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { InjectedWizardProps } from './Wizard';
import styles from './WizardStepHeading.module.scss';

interface Props {
  children: ReactNode;
  editTitle?: string;
  fieldsetHeading?: boolean;
  size?: 'xl' | 'l' | 'm' | 's';
}

const WizardStepHeading = ({
  children,
  currentStep,
  editTitle = '',
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
          className={classNames(
            `govuk-heading-l`,
            `dfe-flex`,
            `dfe-align-items--center`,
            {
              'govuk-fieldset__heading': fieldsetHeading,
            },
          )}
        >
          <span>{children}</span>
          <span className="govuk-tag govuk-tag--turquoise govuk-!-margin-left-3">{`Step  ${stepNumber} of 6`}</span>
        </h2>
      ) : (
        <>
          <h2
            className={classNames(`govuk-heading-${size}`, {
              [styles.stepEnabled]: stepEnabled,
            })}
          >
            <span className="govuk-tag govuk-tag--grey">{`Step ${stepNumber} `}</span>
            <span className="govuk-visually-hidden">{children}</span>
          </h2>
          <a
            href="#"
            data-testid={`wizardStep-${stepNumber}-goToButton`}
            type="button"
            onClick={() => setCurrentStep(stepNumber)}
            className="govuk-link govuk-!-margin-top-1"
          >
            {stepEnabled && (
              <>
                {editTitle}{' '}
                <span className="govuk-visually-hidden">{`Step ${stepNumber}`}</span>
              </>
            )}
          </a>
        </>
      )}
    </>
  );
};

export default WizardStepHeading;
