import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('WizardStepSummary', () => {
  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    setCurrentStep: noop,
    isActive: true,
    isEnabled: true,
    isLoading: false,
    goToNextStep: noop,
    goToPreviousStep: noop,
  };

  test('renders correctly with default wizard props', () => {
    render(
      <WizardStepSummary {...wizardProps} goToButtonText="Edit this">
        <p>The step summary</p>
      </WizardStepSummary>,
    );

    expect(screen.getByText('The step summary')).toBeInTheDocument();
    expect(screen.queryByTestId('loadingSpinner')).not.toBeInTheDocument();

    const button = screen.getByRole('button', { name: /Edit this/ });
    expect(button).not.toBeDisabled();
    expect(button).toHaveTextContent('Edit this on step 1');
  });

  test('disables edit button if `loadingStep` is defined', () => {
    render(
      <WizardStepSummary
        {...wizardProps}
        goToButtonText="Edit this"
        loadingStep={2}
      >
        <p>The step summary</p>
      </WizardStepSummary>,
    );

    expect(screen.getByRole('button', { name: /Edit this/ })).toBeDisabled();
  });

  test('shows loading spinner if `isLoading` is true', () => {
    render(
      <WizardStepSummary {...wizardProps} goToButtonText="Edit this" isLoading>
        <p>The step summary</p>
      </WizardStepSummary>,
    );

    expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
  });
});
