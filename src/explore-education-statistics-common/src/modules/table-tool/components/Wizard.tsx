import useMountedRef from '@common/hooks/useMountedRef';
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
  isActive: boolean;
  isEnabled: boolean;
  isLoading: boolean;
  loadingStep?: number;
  /**
   * Move the wizard to a specific {@param step}.
   *
   * An optional {@param task} can be provided to
   * run before the step transition completes.
   */
  setCurrentStep(step: number, task?: () => Promise<void>): void;
  /**
   * Provide an optional {@param task} that should
   * run before the step transition completes.
   */
  goToNextStep(task?: () => Promise<void>): void;
  /**
   * Provide an optional {@param task} that should
   * run before the step transition completes.
   */
  goToPreviousStep(task?: () => Promise<void>): void;
}

interface Props {
  children: ReactElement | (ReactElement | undefined | boolean)[];
  currentStep?: number;
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
  currentStep,
  initialStep = 1,
  id,
  scrollOnMount = false,
  onStepChange,
}: Props) => {
  const mountedRef = useMountedRef();

  const [shouldScroll, setShouldScroll] = useState(scrollOnMount);
  const [activeStep, setActiveStepState] = useState(initialStep);

  const [loadingStep, setLoadingStep] = useState<number>();

  const filteredChildren = Children.toArray(children).filter(child =>
    isComponentType(child, WizardStep),
  ) as FunctionComponentElement<WizardStepProps & InjectedWizardProps>[];

  const lastStep = filteredChildren.length;

  const setActiveStep = async (
    nextStep: number,
    task?: () => Promise<void>,
  ) => {
    if (nextStep > lastStep || nextStep < 1) {
      return;
    }

    setShouldScroll(true);

    const current = activeStep;
    let next = nextStep;

    if (onStepChange) {
      next = await onStepChange(nextStep, activeStep);
    }

    setLoadingStep(next);

    const stepElement = filteredChildren[next - 1];

    if (next < current && stepElement?.props?.onBack) {
      await stepElement.props.onBack();
    }

    try {
      if (task) {
        await task();
      }

      setActiveStepState(next);
    } finally {
      if (mountedRef.current) {
        setLoadingStep(undefined);
      }
    }
  };

  useEffect(() => {
    if (currentStep) {
      setActiveStepState(currentStep);
    }
  }, [currentStep]);

  return (
    <ol className={styles.stepNav} id={id}>
      {filteredChildren.map((child, index) => {
        const stepNumber = index + 1;

        return cloneElement<WizardStepProps & InjectedWizardProps>(child, {
          stepNumber,
          currentStep: activeStep,
          loadingStep,
          shouldScroll,
          setCurrentStep: setActiveStep,
          id: child.props.id || `${id}-step-${stepNumber}`,
          isActive: stepNumber === activeStep,
          isEnabled: activeStep >= stepNumber,
          isLoading: loadingStep === stepNumber,
          async goToPreviousStep(task) {
            await setActiveStep(stepNumber - 1 < 1 ? 1 : stepNumber - 1, task);
          },
          async goToNextStep(task) {
            await setActiveStep(
              stepNumber + 1 > lastStep ? lastStep : stepNumber + 1,
              task,
            );
          },
        });
      })}
    </ol>
  );
};

export default Wizard;
