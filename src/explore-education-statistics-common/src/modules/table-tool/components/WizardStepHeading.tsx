import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { InjectedWizardProps } from './Wizard';
import styles from './WizardStepHeading.module.scss';

interface Props extends InjectedWizardProps {
  children: ReactNode;
  fieldsetHeading?: boolean;
}

const WizardStepHeading = ({
  children,
  fieldsetHeading = false,
  isActive,
  isEnabled,
  stepNumber,
}: Props) => {
  return (
    <h2
      className={classNames({
        'govuk-heading-l dfe-flex dfe-align-items--center': isActive,
        'govuk-fieldset__heading': fieldsetHeading && isActive,
        [styles.stepEnabled]: isEnabled && !isActive,
      })}
    >
      <span
        className={classNames('govuk-tag', {
          'govuk-tag--turquoise govuk-!-margin-right-2': isActive,
          'govuk-tag govuk-tag--grey': !isActive,
        })}
      >
        {`Step ${stepNumber} `}
        {isActive && <span className="govuk-visually-hidden">(current) </span>}
      </span>
      <span className={classNames({ 'govuk-visually-hidden': !isActive })}>
        {children}
      </span>
    </h2>
  );
};

export default WizardStepHeading;
