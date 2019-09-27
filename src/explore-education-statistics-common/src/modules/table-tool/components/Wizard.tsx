import isComponentType from '@common/lib/type-guards/components/isComponentType';
import React, { Children, cloneElement, ReactElement, useState } from 'react';
import styles from './Wizard.module.scss';
import WizardStep, { WizardStepProps } from './WizardStep';

export interface InjectedWizardProps {
  stepNumber: number;
  currentStep: number;
  setCurrentStep(step: number): void;
  isActive: boolean;
  goToNextStep(): void;
  goToPreviousStep(): void;
}

interface Props {
  children: ReactElement | ReactElement[];
  initialStep?: number;
  id: string;
  onStepChange?: (
    nextStep: number,
    previousStep: number,
  ) => void | number | Promise<number> | Promise<void>;
}

const Wizard = ({ children, initialStep = 1, id, onStepChange }: Props) => {
  const [currentStep, setCurrentStepState] = useState(initialStep);

  const filteredChildren = Children.toArray(children).filter(child =>
    isComponentType(child, WizardStep),
  );

  const lastStep = filteredChildren.length;

  const setCurrentStep = async (nextStep: number) => {
    if (onStepChange) {
      const next = await onStepChange(nextStep, currentStep);

      setCurrentStepState(next || nextStep);
    } else {
      setCurrentStepState(nextStep);
    }
  };

  return (
    <ol className={styles.stepNav}>
      {filteredChildren.map((child, index) => {
        const stepNumber = index + 1;

        return cloneElement<WizardStepProps | InjectedWizardProps>(child, {
          stepNumber,
          currentStep,
          async setCurrentStep(nextStep: number) {
            if (nextStep <= lastStep && nextStep >= 1) {
              await setCurrentStep(nextStep);
            }
          },
          id: child.props.id || `${id}-step-${stepNumber}`,
          isActive: stepNumber === currentStep,
          async goToPreviousStep() {
            await setCurrentStep(stepNumber - 1 < 1 ? 1 : stepNumber - 1);
          },
          async goToNextStep() {
            await setCurrentStep(
              stepNumber + 1 > lastStep ? lastStep : stepNumber + 1,
            );
          },
        });
      })}
    </ol>
  );
};

export default Wizard;
