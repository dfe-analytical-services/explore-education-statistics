import isComponentType from '@common/utils/type-guards/components/isComponentType';
import React, {
  Children,
  cloneElement,
  FunctionComponentElement,
  ReactElement,
  useEffect,
  useState,
} from 'react';
import styles from './Wizard.module.scss';
import WizardStep, { WizardStepProps } from './WizardStep';

export interface InjectedWizardProps {
  shouldScroll: boolean;
  stepNumber: number;
  currentStep: number;
  setCurrentStep(step: number): void;
  isActive: boolean;
  isEnabled: boolean;
  isLoading: boolean;
  goToNextStep(): void;
  goToPreviousStep(): void;
}

interface Props {
  children: ReactElement | (ReactElement | undefined | boolean)[];
  initialStep?: number;
  id: string;
  scrollOnMount?: boolean;
  onStepChange?: (
    nextStep: number,
    previousStep: number,
  ) => number | Promise<number>;
}

const Wizard = ({
  children,
  initialStep = 1,
  id,
  scrollOnMount = false,
  onStepChange,
}: Props) => {
  const [shouldScroll, setShouldScroll] = useState(scrollOnMount);
  const [currentStep, setCurrentStepState] = useState(initialStep);

  const [loading, setLoading] = useState<number>();

  const filteredChildren = Children.toArray(children).filter(child =>
    isComponentType(child, WizardStep),
  ) as FunctionComponentElement<WizardStepProps & InjectedWizardProps>[];

  const lastStep = filteredChildren.length;

  const setCurrentStep = async (nextStep: number) => {
    setShouldScroll(true);

    const current = currentStep;
    let next = nextStep;

    if (onStepChange) {
      next = await onStepChange(nextStep, currentStep);
    }

    setCurrentStepState(next);

    const stepElement = filteredChildren[next - 1];

    if (next < current && stepElement?.props?.onBack) {
      setLoading(next);

      await stepElement.props.onBack();

      setLoading(undefined);
    }
  };

  useEffect(() => {
    setCurrentStepState(initialStep);
  }, [initialStep]);

  return (
    <ol className={styles.stepNav} id={id}>
      {filteredChildren.map((child, index) => {
        const stepNumber = index + 1;

        return cloneElement<WizardStepProps & InjectedWizardProps>(child, {
          stepNumber,
          currentStep,
          shouldScroll,
          async setCurrentStep(nextStep: number) {
            if (nextStep <= lastStep && nextStep >= 1) {
              await setCurrentStep(nextStep);
            }
          },
          id: child.props.id || `${id}-step-${stepNumber}`,
          isActive: stepNumber === currentStep,
          isEnabled: currentStep >= stepNumber,
          isLoading: loading === stepNumber,
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
