import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import Wizard from '../Wizard';
import WizardStep from '../WizardStep';

describe('Wizard', () => {
  test('does not render children that are not WizardSteps', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>Step 1</WizardStep>
        <p>Not a wizard step</p>
        <WizardStep>Step 2</WizardStep>
      </Wizard>,
    );

    expect(screen.getByText('Step 1')).toBeInTheDocument();
    expect(screen.getByText('Step 2')).toBeInTheDocument();
    expect(screen.queryByText('Not a wizard step')).toBeNull();
  });

  test('renders correctly with default `initialStep`', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>Step 1</WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');

    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('renders each step with correct state props with default `initialStep`', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ isActive, isEnabled }) => (
            <>
              <p>{`Step 1 - isActive: ${isActive}`}</p>
              <p>{`Step 1 - isEnabled: ${isEnabled}`}</p>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isActive, isEnabled }) => (
            <>
              <p>{`Step 2 - isActive: ${isActive}`}</p>
              <p>{`Step 2 - isEnabled: ${isEnabled}`}</p>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isActive, isEnabled }) => (
            <>
              <p>{`Step 3 - isActive: ${isActive}`}</p>
              <p>{`Step 3 - isEnabled: ${isEnabled}`}</p>
            </>
          )}
        </WizardStep>
      </Wizard>,
    );

    expect(screen.getByText('Step 1 - isActive: true')).toBeInTheDocument();
    expect(screen.getByText('Step 1 - isEnabled: true')).toBeInTheDocument();

    expect(screen.getByText('Step 2 - isActive: false')).toBeInTheDocument();
    expect(screen.getByText('Step 2 - isEnabled: false')).toBeInTheDocument();

    expect(screen.getByText('Step 3 - isActive: false')).toBeInTheDocument();
    expect(screen.getByText('Step 3 - isEnabled: false')).toBeInTheDocument();
  });

  test('renders correctly with `initialStep` set to Step 2', () => {
    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveAttribute('aria-current', 'step');
    expect(step3).not.toBeVisible();
  });

  test('renders each step with correct state props when `initialStep` set to Step 2', () => {
    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>
          {({ isActive, isEnabled }) => (
            <>
              <p>{`Step 1 - isActive: ${isActive}`}</p>
              <p>{`Step 1 - isEnabled: ${isEnabled}`}</p>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isActive, isEnabled }) => (
            <>
              <p>{`Step 2 - isActive: ${isActive}`}</p>
              <p>{`Step 2 - isEnabled: ${isEnabled}`}</p>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isActive, isEnabled }) => (
            <>
              <p>{`Step 3 - isActive: ${isActive}`}</p>
              <p>{`Step 3 - isEnabled: ${isEnabled}`}</p>
            </>
          )}
        </WizardStep>
      </Wizard>,
    );

    expect(screen.getByText('Step 1 - isActive: false')).toBeInTheDocument();
    expect(screen.getByText('Step 1 - isEnabled: true')).toBeInTheDocument();

    expect(screen.getByText('Step 2 - isActive: true')).toBeInTheDocument();
    expect(screen.getByText('Step 2 - isEnabled: true')).toBeInTheDocument();

    expect(screen.getByText('Step 3 - isActive: false')).toBeInTheDocument();
    expect(screen.getByText('Step 3 - isEnabled: false')).toBeInTheDocument();
  });

  test('calling `setCurrentStep` render prop moves wizard to that step', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ setCurrentStep }) => (
            <button type="button" onClick={() => setCurrentStep(3)}>
              Go to step 3
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveAttribute('aria-current', 'step');
  });

  test('calling `setCurrentStep` with a step greater than the last step will not change the wizard', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ setCurrentStep }) => {
            return (
              <button type="button" onClick={() => setCurrentStep(4)}>
                Go to step 4
              </button>
            );
          }}
        </WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(screen.getByRole('button', { name: 'Go to step 4' }));

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `setCurrentStep` with a step less than the first step will not change the wizard', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ setCurrentStep }) => {
            return (
              <button type="button" onClick={() => setCurrentStep(-1)}>
                Go to step -1
              </button>
            );
          }}
        </WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(screen.getByRole('button', { name: 'Go to step -1' }));

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `goToPreviousStep` render prop moves wizard to previous step', () => {
    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>
          {({ goToPreviousStep }) => (
            <button type="button" onClick={goToPreviousStep}>
              Go to previous step
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveAttribute('aria-current', 'step');
    expect(step3).not.toBeVisible();

    fireEvent.click(
      screen.getByRole('button', { name: 'Go to previous step' }),
    );

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `goToPreviousStep` on first step does not change wizard', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ goToPreviousStep }) => (
            <button type="button" onClick={goToPreviousStep}>
              Go to previous step
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(
      screen.getByRole('button', { name: 'Go to previous step' }),
    );

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `goToNextStep` render prop moves wizard to next step', () => {
    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>
          {({ goToNextStep }) => (
            <button type="button" onClick={goToNextStep}>
              Go to next step
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveAttribute('aria-current', 'step');
    expect(step3).not.toBeVisible();

    fireEvent.click(screen.getByRole('button', { name: 'Go to next step' }));

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveAttribute('aria-current', 'step');
  });

  test('calling `goToNextStep` on last step does not change wizard', () => {
    render(
      <Wizard id="test-wizard" initialStep={3}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>
          {({ goToNextStep }) => (
            <button type="button" onClick={goToNextStep}>
              Go to next step
            </button>
          )}
        </WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveAttribute('aria-current', 'step');

    fireEvent.click(screen.getByRole('button', { name: 'Go to next step' }));

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveAttribute('aria-current', 'step');
  });

  test('does not scroll or focus first step when mounted by default', () => {
    render(
      <Wizard id="test-wizard">
        <WizardStep>Step 1</WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).not.toHaveFocus();
    expect(step1).not.toHaveScrolledIntoView();

    expect(step2).not.toHaveScrolledIntoView();
    expect(step2).not.toHaveFocus();

    expect(step3).not.toHaveScrolledIntoView();
    expect(step3).not.toHaveFocus();
  });

  test('scrolls to and focuses first step when `scrollOnMount` is true', () => {
    jest.useFakeTimers();

    render(
      <Wizard id="test-wizard" scrollOnMount>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    jest.runAllTimers();

    expect(step1).toHaveFocus();
    expect(step1).toHaveScrolledIntoView();

    expect(step2).not.toHaveScrolledIntoView();
    expect(step2).not.toHaveFocus();

    expect(step3).not.toHaveScrolledIntoView();
    expect(step3).not.toHaveFocus();
  });

  test('scrolls to and focuses step when step changes', () => {
    jest.useFakeTimers();

    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ setCurrentStep }) => (
            <button type="button" onClick={() => setCurrentStep(3)}>
              Go to step 3
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    fireEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

    jest.runAllTimers();

    expect(step1).not.toHaveFocus();
    expect(step1).not.toHaveScrolledIntoView();

    expect(step2).not.toHaveFocus();
    expect(step2).not.toHaveScrolledIntoView();

    expect(step3).toHaveFocus();
    expect(step3).toHaveScrolledIntoView();
  });

  test('calls `onStepChange` handler when step changes', () => {
    const onStepChange = jest.fn();

    render(
      <Wizard id="test-wizard" onStepChange={onStepChange}>
        <WizardStep>
          {({ setCurrentStep }) => (
            <button type="button" onClick={() => setCurrentStep(3)}>
              Go to step 3
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    expect(onStepChange).not.toHaveBeenCalled();

    fireEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

    expect(onStepChange).toHaveBeenCalledWith(3, 1);
  });

  test('can change to a different step by returning a number from `onStepChange` handler', async () => {
    render(
      <Wizard id="test-wizard" onStepChange={() => 2}>
        <WizardStep>
          {({ setCurrentStep }) => (
            <button type="button" onClick={() => setCurrentStep(3)}>
              Go to step 3
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');
    const step3 = screen.getByTestId('wizardStep-3');

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

    await waitFor(() => {
      expect(step1).toBeVisible();
      expect(step2).toBeVisible();
      expect(step2).toHaveAttribute('aria-current', 'step');
      expect(step3).not.toBeVisible();
    });
  });

  test('moving back calls the step `onBack` handler', async () => {
    const onBack = jest.fn();

    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep onBack={onBack}>Step 1</WizardStep>
        <WizardStep>
          {({ setCurrentStep }) => (
            <button type="button" onClick={() => setCurrentStep(1)}>
              Go to step 1
            </button>
          )}
        </WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Go to step 1' }));

    await waitFor(() => {
      expect(onBack).toHaveBeenCalledTimes(1);
    });
  });

  test('moving forward does not call the step `onBack` handler', async () => {
    const onBack = jest.fn();

    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ setCurrentStep }) => (
            <button type="button" onClick={() => setCurrentStep(2)}>
              Go to step 2
            </button>
          )}
        </WizardStep>
        <WizardStep onBack={onBack}>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    fireEvent.click(screen.getByRole('button', { name: 'Go to step 2' }));

    await waitFor(() => {
      expect(onBack).not.toHaveBeenCalled();
    });
  });
});
