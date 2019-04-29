import isComponentType from '@common/lib/type-guards/components/isComponentType';
import WizardStep, {
  WizardStepProps,
} from '@frontend/prototypes/table-tool/components/WizardStep';
import React, { Children, cloneElement, ReactElement, useState } from 'react';
import styles from './Wizard.module.scss';

export interface InjectedWizardProps {
  stepNumber: number;
  currentStep: number;
  setCurrentStep: (step: number) => void;
  goToNextStep: () => void;
  goToPreviousStep: () => void;
}

interface Props {
  children: ReactElement | ReactElement[];
  initialStep?: number;
  id: string;
}

const Wizard = ({ children, initialStep = 1, id }: Props) => {
  const [currentStep, setCurrentStep] = useState(initialStep);

  const filteredChildren = Children.toArray(children).filter(child =>
    isComponentType(child, WizardStep),
  );

  return (
    <div className={styles.container}>
      <div>
        <ol className={styles.stepNav}>
          {filteredChildren.map((child, index) => {
            const stepNumber = index + 1;

            return cloneElement<WizardStepProps | InjectedWizardProps>(child, {
              stepNumber,
              currentStep,
              setCurrentStep,
              id: child.props.id || `${id}-${stepNumber}`,
              goToPreviousStep: () => {
                setCurrentStep(stepNumber - 1 < 1 ? 1 : stepNumber - 1);
              },
              goToNextStep: () => {
                setCurrentStep(
                  stepNumber + 1 > filteredChildren.length
                    ? filteredChildren.length
                    : stepNumber + 1,
                );
              },
            });
          })}
        </ol>
      </div>
    </div>
  );
};

export default Wizard;
