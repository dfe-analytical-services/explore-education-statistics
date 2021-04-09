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
            `govuk-!-margin-top-6`,
            {
              'govuk-fieldset__heading': fieldsetHeading,
            },
          )}
        >
          <span className="govuk-tag govuk-tag--turquoise govuk-!-margin-right-2">{`Step  ${stepNumber}`}</span>
          <span>{children}</span>
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
          <div className="govuk-!-margin-top-2">
            <a
              href="#"
              data-testid={`wizardStep-${stepNumber}-goToButton`}
              onClick={() => setCurrentStep(stepNumber)}
              className="govuk-link"
            >
              {stepEnabled && (
                <>
                  {editTitle}{' '}
                  <span className="govuk-visually-hidden">{`Step ${stepNumber}`}</span>
                </>
              )}
            </a>
          </div>
        </>
      )}
    </>
  );
};

export default WizardStepHeading;
