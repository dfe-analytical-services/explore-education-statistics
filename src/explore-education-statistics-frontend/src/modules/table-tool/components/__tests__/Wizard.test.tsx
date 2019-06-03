import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import Wizard from '../Wizard';
import WizardStep from '../WizardStep';

describe('Wizard', () => {
  test('does not render children that are not WizardSteps', () => {
    const { queryByText } = render(
      <Wizard id="test-wizard">
        <WizardStep>Step 1</WizardStep>
        <p>Not a wizard step</p>
        <WizardStep>Step 2</WizardStep>
      </Wizard>,
    );

    expect(queryByText('Step 1')).not.toBeNull();
    expect(queryByText('Step 2')).not.toBeNull();
    expect(queryByText('Not a wizard step')).toBeNull();
  });

  test('renders correctly with default `initialStep of Step 1`', () => {
    const { container } = render(
      <Wizard id="test-wizard">
        <WizardStep>Step 1</WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');

    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('renders correctly with `initialStep` set to Step 2', () => {
    const { container } = render(
      <Wizard id="test-wizard" initialStep={2}>
        <WizardStep>Step 1</WizardStep>
        <WizardStep>Step 2</WizardStep>
        <WizardStep>Step 3</WizardStep>
      </Wizard>,
    );

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveClass('stepActive');
    expect(step3).not.toBeVisible();
  });

  test('calling `setCurrentStep` render prop moves wizard to that step', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(getByText('Go to step 3'));

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveClass('stepActive');
  });

  test('calling `setCurrentStep` with a step greater than the last step will not change the wizard', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(getByText('Go to step 4'));

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `setCurrentStep` with a step less than the first step will not change the wizard', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(getByText('Go to step -1'));

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `goToPreviousStep` render prop moves wizard to previous step', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveClass('stepActive');
    expect(step3).not.toBeVisible();

    fireEvent.click(getByText('Go to previous step'));

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `goToPreviousStep` on first step does not change wizard', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();

    fireEvent.click(getByText('Go to previous step'));

    expect(step1).toBeVisible();
    expect(step1).toHaveClass('stepActive');
    expect(step2).not.toBeVisible();
    expect(step3).not.toBeVisible();
  });

  test('calling `goToNextStep` render prop moves wizard to next step', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step2).toHaveClass('stepActive');
    expect(step3).not.toBeVisible();

    fireEvent.click(getByText('Go to next step'));

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveClass('stepActive');
  });

  test('calling `goToNextStep` on last step does not change wizard', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveClass('stepActive');

    fireEvent.click(getByText('Go to next step'));

    expect(step1).toBeVisible();
    expect(step2).toBeVisible();
    expect(step3).toBeVisible();
    expect(step3).toHaveClass('stepActive');
  });

  test('scrolls to and focuses first step when mounted', () => {
    const { container } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toHaveFocus();
    expect(step1).toHaveScrolledIntoView();

    expect(step2).not.toHaveScrolledIntoView();
    expect(step2).not.toHaveFocus();

    expect(step3).not.toHaveScrolledIntoView();
    expect(step3).not.toHaveFocus();
  });

  test('scrolls to and focuses step when step changes', () => {
    const { container, getByText } = render(
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

    const step1 = container.querySelector('#test-wizard-step-1') as HTMLElement;
    const step2 = container.querySelector('#test-wizard-step-2') as HTMLElement;
    const step3 = container.querySelector('#test-wizard-step-3') as HTMLElement;

    expect(step1).toHaveFocus();
    expect(step1).toHaveScrolledIntoView();

    expect(step2).not.toHaveFocus();
    expect(step2).not.toHaveScrolledIntoView();

    expect(step3).not.toHaveFocus();
    expect(step3).not.toHaveScrolledIntoView();

    fireEvent.click(getByText('Go to step 3'));

    expect(step1).not.toHaveFocus();
    expect(step1).toHaveScrolledIntoView();

    expect(step2).not.toHaveFocus();
    expect(step2).not.toHaveScrolledIntoView();

    expect(step3).toHaveFocus();
    expect(step3).toHaveScrolledIntoView();
  });
});
