import flushPromises from '@common-test/flushPromises';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 4' }));

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

    userEvent.click(screen.getByRole('button', { name: 'Go to step -1' }));

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `setCurrentStep` with a task will not transition the wizard until it completes', async () => {
    jest.useFakeTimers();

    const task = jest.fn(
      () => new Promise<void>(resolve => setTimeout(resolve, 500)),
    );

    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ setCurrentStep }) => (
            <button type="button" onClick={() => setCurrentStep(3, task)}>
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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

    // Still on same step
    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    // Task needs to complete first
    jest.advanceTimersByTime(500);
    await flushPromises();

    // Moved to next step
    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveAttribute('aria-current', 'step');

    jest.useRealTimers();
  });

  test('calling `goToPreviousStep` render prop moves wizard to previous step', () => {
    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>
          {({ goToPreviousStep }) => (
            <button type="button" onClick={() => goToPreviousStep()}>
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

    userEvent.click(
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
            <button type="button" onClick={() => goToPreviousStep()}>
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

    userEvent.click(
      screen.getByRole('button', { name: 'Go to previous step' }),
    );

    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `goToPreviousStep` with a task will not transition the wizard until it completes', async () => {
    jest.useFakeTimers();

    const task = jest.fn(
      () => new Promise<void>(resolve => setTimeout(resolve, 500)),
    );

    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>
          {({ goToPreviousStep }) => (
            <button type="button" onClick={() => goToPreviousStep(task)}>
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

    userEvent.click(
      screen.getByRole('button', { name: 'Go to previous step' }),
    );

    // Still on same step
    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveAttribute('aria-current', 'step');
    expect(step3).not.toBeVisible();

    // Task needs to complete first
    jest.advanceTimersByTime(500);
    await flushPromises();

    // Moved to next step
    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    jest.useRealTimers();
  });

  test('calling `goToPreviousStep` with a task sets correct loading render props', async () => {
    const task = jest.fn(() => Promise.resolve());

    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>
          {({ isLoading, loadingStep }) => (
            <>
              <p>{`Step 1 - isLoading: ${isLoading}`}</p>
              <p>{`Step 1 - loadingStep: ${loadingStep}`}</p>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ goToPreviousStep, isLoading, loadingStep }) => (
            <>
              <p>{`Step 2 - isLoading: ${isLoading}`}</p>
              <p>{`Step 2 - loadingStep: ${loadingStep}`}</p>

              <button type="button" onClick={() => goToPreviousStep(task)}>
                Go to prev step
              </button>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isLoading, loadingStep }) => (
            <>
              <p>{`Step 3 - isLoading: ${isLoading}`}</p>
              <p>{`Step 3 - loadingStep: ${loadingStep}`}</p>
            </>
          )}
        </WizardStep>
      </Wizard>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Go to prev step' }));

    expect(screen.getByText('Step 1 - isLoading: true')).toBeInTheDocument();
    expect(screen.getByText('Step 1 - loadingStep: 1')).toBeInTheDocument();

    expect(screen.getByText('Step 2 - isLoading: false')).toBeInTheDocument();
    expect(screen.getByText('Step 2 - loadingStep: 1')).toBeInTheDocument();

    expect(screen.getByText('Step 3 - isLoading: false')).toBeInTheDocument();
    expect(screen.getByText('Step 3 - loadingStep: 1')).toBeInTheDocument();
  });

  test('calling `goToNextStep` render prop moves wizard to next step', () => {
    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>
          {({ goToNextStep }) => (
            <button type="button" onClick={() => goToNextStep()}>
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

    userEvent.click(screen.getByRole('button', { name: 'Go to next step' }));

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
            <button type="button" onClick={() => goToNextStep()}>
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

    userEvent.click(screen.getByRole('button', { name: 'Go to next step' }));

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveAttribute('aria-current', 'step');
  });

  test('calling `goToNextStep` with a task will not transition the wizard until it completes', async () => {
    jest.useFakeTimers();

    const task = jest.fn(
      () => new Promise<void>(resolve => setTimeout(resolve, 500)),
    );

    render(
      <Wizard id="test-wizard">
        <WizardStep>
          {({ goToNextStep }) => (
            <button type="button" onClick={() => goToNextStep(task)}>
              Go to next step
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

    userEvent.click(screen.getByRole('button', { name: 'Go to next step' }));

    // Still on same step
    expect(step1).toBeVisible();
    expect(step1).toHaveAttribute('aria-current', 'step');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    // Task needs to complete first
    jest.advanceTimersByTime(500);
    await flushPromises();

    // Moved to next step
    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveAttribute('aria-current', 'step');
    expect(step3).not.toBeVisible();

    jest.useRealTimers();
  });

  test('calling `goToNextStep` with a task sets correct loading render props', async () => {
    const task = jest.fn(() => Promise.resolve());

    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>
          {({ isLoading, loadingStep }) => (
            <>
              <p>{`Step 1 - isLoading: ${isLoading}`}</p>
              <p>{`Step 1 - loadingStep: ${loadingStep}`}</p>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isLoading, loadingStep, goToNextStep }) => (
            <>
              <p>{`Step 2 - isLoading: ${isLoading}`}</p>
              <p>{`Step 2 - loadingStep: ${loadingStep}`}</p>

              <button type="button" onClick={() => goToNextStep(task)}>
                Go to next step
              </button>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isLoading, loadingStep }) => (
            <>
              <p>{`Step 3 - isLoading: ${isLoading}`}</p>
              <p>{`Step 3 - loadingStep: ${loadingStep}`}</p>
            </>
          )}
        </WizardStep>
      </Wizard>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Go to next step' }));

    expect(screen.getByText('Step 1 - isLoading: false')).toBeInTheDocument();
    expect(screen.getByText('Step 1 - loadingStep: 3')).toBeInTheDocument();

    expect(screen.getByText('Step 2 - isLoading: false')).toBeInTheDocument();
    expect(screen.getByText('Step 2 - loadingStep: 3')).toBeInTheDocument();

    expect(screen.getByText('Step 3 - isLoading: true')).toBeInTheDocument();
    expect(screen.getByText('Step 3 - loadingStep: 3')).toBeInTheDocument();
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

  test('does not scroll and focus first step when `scrollOnMount` is true', () => {
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

    expect(step1).not.toHaveFocus();
    expect(step1).not.toHaveScrolledIntoView();

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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 3' }));

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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 1' }));

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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 2' }));

    await waitFor(() => {
      expect(onBack).not.toHaveBeenCalled();
    });
  });

  test('moving back does not occur until `onBack` has completed', async () => {
    jest.useFakeTimers();

    const handleBack = jest.fn(
      () => new Promise(resolve => setTimeout(resolve, 500)),
    );

    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep onBack={handleBack}>Step 1</WizardStep>
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

    userEvent.click(screen.getByRole('button', { name: 'Go to step 1' }));

    await waitFor(() => {
      expect(handleBack).toHaveBeenCalledTimes(1);
    });

    const step1 = screen.getByTestId('wizardStep-1');
    const step2 = screen.getByTestId('wizardStep-2');

    expect(step1).not.toHaveAttribute('aria-current');
    expect(step2).toHaveAttribute('aria-current', 'step');

    jest.advanceTimersByTime(500);

    await waitFor(() => {
      expect(step1).toHaveAttribute('aria-current', 'step');
      expect(step2).not.toHaveAttribute('aria-current');
    });

    expect(handleBack).toHaveBeenCalledTimes(1);

    jest.useRealTimers();
  });

  test('renders correct loading states whilst `onBack` is running', async () => {
    jest.useFakeTimers();

    const handleBack = jest.fn(
      () => new Promise(resolve => setTimeout(resolve, 500)),
    );

    render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep onBack={handleBack}>
          {({ isLoading, loadingStep }) => (
            <>
              <p>{`Step 1 - isLoading: ${isLoading}`}</p>
              <p>{`Step 1 - loadingStep: ${loadingStep}`}</p>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ setCurrentStep, isLoading, loadingStep }) => (
            <>
              <p>{`Step 2 - isLoading: ${isLoading}`}</p>
              <p>{`Step 2 - loadingStep: ${loadingStep}`}</p>

              <button type="button" onClick={() => setCurrentStep(1)}>
                Go to step 1
              </button>
            </>
          )}
        </WizardStep>
        <WizardStep>
          {({ isLoading, loadingStep }) => (
            <>
              <p>{`Step 3 - isLoading: ${isLoading}`}</p>
              <p>{`Step 3 - loadingStep: ${loadingStep}`}</p>
            </>
          )}
        </WizardStep>
      </Wizard>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Go to step 1' }));

    await waitFor(() => {
      expect(handleBack).toHaveBeenCalledTimes(1);
    });

    expect(screen.getByText('Step 1 - isLoading: true')).toBeInTheDocument();
    expect(screen.getByText('Step 1 - loadingStep: 1')).toBeInTheDocument();

    expect(screen.getByText('Step 2 - isLoading: false')).toBeInTheDocument();
    expect(screen.getByText('Step 2 - loadingStep: 1')).toBeInTheDocument();

    expect(screen.getByText('Step 3 - isLoading: false')).toBeInTheDocument();
    expect(screen.getByText('Step 3 - loadingStep: 1')).toBeInTheDocument();

    jest.useRealTimers();
  });
});
