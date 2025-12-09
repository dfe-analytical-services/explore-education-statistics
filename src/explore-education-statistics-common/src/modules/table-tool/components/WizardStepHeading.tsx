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
  stepHeadingTag: Heading = 'h2',
  stepNumber,
}: Props) => {
  return (
    <Heading
      aria-current={isActive ? 'step' : undefined}
      className={classNames('govuk-heading-l', {
        'govuk-heading-l dfe-flex dfe-align-items--center': isActive,
        'govuk-fieldset__heading': fieldsetHeading && isActive,
        [styles.stepEnabled]: isEnabled && !isActive,
      })}
    >
      <span
        className={classNames('govuk-tag', 'dfe-white-space--nowrap', {
          'govuk-tag--turquoise govuk-!-margin-right-2': isActive,
          'govuk-tag govuk-tag--grey': !isActive,
        })}
      >
        {`Step ${stepNumber} `}
      </span>
      <span className={classNames({ 'govuk-visually-hidden': !isActive })}>
        {children}
      </span>
    </Heading>
  );
};

export default WizardStepHeading;
