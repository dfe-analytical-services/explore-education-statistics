import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';

// eslint-disable-next-line import/prefer-default-export
export const testWizardStepProps: InjectedWizardProps = {
  currentStep: 1,
  isActive: true,
  isEnabled: true,
  isLoading: false,
  stepNumber: 1,
  setCurrentStep: (step, task) => task?.(),
  goToNextStep: task => task?.(),
  goToPreviousStep: task => task?.(),
  shouldScroll: false,
};
