import flushPromises from '@common-test/flushPromises';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';
import WizardStepFormActions from '../WizardStepFormActions';

describe('WizardStepFormActions', () => {
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

  test('when submitting, `Next step` button changes to disabled `Submitting` button', async () => {
    render(<WizardStepFormActions {...wizardProps} isSubmitting />);

    expect(screen.getByRole('button', { name: 'Submitting' })).toBeDisabled();
  });

  test('when submitting, `Next step` button changes to custom submitting text', () => {
    render(
      <WizardStepFormActions
        {...wizardProps}
        isSubmitting
        submittingText="Processing..."
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Processing...' }),
    ).toBeDisabled();
  });

  test('does not render `Previous step` button when not on step 2', () => {
    const { rerender } = render(<WizardStepFormActions {...wizardProps} />);

    expect(screen.queryByText('Previous step')).not.toBeInTheDocument();

    rerender(<WizardStepFormActions {...wizardProps} stepNumber={2} />);

    expect(screen.getByText('Previous step')).toBeInTheDocument();
  });

  test('when submitting, `Previous step` button is disabled', () => {
    render(
      <WizardStepFormActions {...wizardProps} isSubmitting stepNumber={2} />,
    );

    const previousButton = screen.getByRole('button', {
      name: 'Previous step',
    });

    expect(previousButton).toBeDisabled();
  });

  test('clicking `Previous step` button calls `goToPreviousStep`', async () => {
    const goToPreviousStep = jest.fn();

    render(
      <WizardStepFormActions
        {...wizardProps}
        goToPreviousStep={goToPreviousStep}
        stepNumber={2}
      />,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    await flushPromises();
    expect(goToPreviousStep).toHaveBeenCalled();
  });

  test('clicking `Previous step` button does not call `goToPreviousStep` until `onPreviousStep` completes', async () => {
    jest.useFakeTimers();

    const handlePreviousStep = () => Promise.resolve();

    const goToPreviousStep = jest.fn();

    render(
      <WizardStepFormActions
        {...wizardProps}
        goToPreviousStep={goToPreviousStep}
        onPreviousStep={handlePreviousStep}
        stepNumber={2}
      />,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    expect(goToPreviousStep).not.toHaveBeenCalled();

    await flushPromises();

    expect(goToPreviousStep).toHaveBeenCalled();
  });

  test('preventing default `Previous step` button event does not call `goToPreviousStep` handler', async () => {
    const goToPreviousStep = jest.fn();

    render(
      <WizardStepFormActions
        {...wizardProps}
        goToPreviousStep={goToPreviousStep}
        onPreviousStep={event => event.preventDefault()}
        stepNumber={2}
      />,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    await flushPromises();
    expect(goToPreviousStep).not.toHaveBeenCalled();
  });
});
