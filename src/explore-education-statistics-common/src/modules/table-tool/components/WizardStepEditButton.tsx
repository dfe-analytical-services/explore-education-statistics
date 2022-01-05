import ButtonText from '@common/components/ButtonText';
import React from 'react';
import { InjectedWizardProps } from './Wizard';

interface Props {
  children: string;
}

const WizardStepEditButton = ({
  children,
  stepNumber,
  setCurrentStep,
}: Props & InjectedWizardProps) => {
  return (
    <ButtonText
      className="govuk-!-margin-top-2 govuk-!-margin-bottom-2"
      data-testid={`wizardStep-${stepNumber}-goToButton`}
      onClick={() => setCurrentStep(stepNumber)}
    >
      {children}{' '}
      <span className="govuk-visually-hidden">{`on step ${stepNumber}`}</span>
    </ButtonText>
  );
};

export default WizardStepEditButton;
