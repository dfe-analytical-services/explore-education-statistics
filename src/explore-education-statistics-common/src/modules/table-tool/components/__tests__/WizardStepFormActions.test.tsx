import flushPromises from '@common-test/flushPromises';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Form, Formik } from 'formik';
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
    render(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions {...wizardProps} />
        </Form>
      </Formik>,
    );

    const button = screen.getByRole('button', { name: 'Next step' });

    userEvent.click(button);

    expect(button).not.toHaveTextContent('Next step');
    expect(button).toHaveTextContent('Submitting');
    expect(button).toBeDisabled();
  });

  test('when submitting, `Next step` button changes to custom submitting text', () => {
    render(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions
            {...wizardProps}
            submittingText="Processing..."
          />
        </Form>
      </Formik>,
    );

    const button = screen.getByRole('button', { name: 'Next step' });

    userEvent.click(button);

    expect(button).not.toHaveTextContent('Next step');
    expect(button).toHaveTextContent('Processing...');
    expect(button).toBeDisabled();
  });

  test('does not render `Previous step` button when not on step 2', () => {
    const { rerender } = render(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions {...wizardProps} />
        </Form>
      </Formik>,
    );

    expect(screen.queryByText('Previous step')).not.toBeInTheDocument();

    rerender(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions {...wizardProps} stepNumber={2} />
        </Form>
      </Formik>,
    );

    expect(screen.getByText('Previous step')).toBeInTheDocument();
  });

  test('when submitting, `Previous step` button is disabled', () => {
    render(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions {...wizardProps} stepNumber={2} />
        </Form>
      </Formik>,
    );

    const previousButton = screen.getByRole('button', {
      name: 'Previous step',
    });

    expect(previousButton).not.toBeDisabled();

    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    expect(previousButton).toBeDisabled();
  });

  test('clicking `Previous step` button calls `goToPreviousStep`', async () => {
    const goToPreviousStep = jest.fn();

    render(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions
            {...wizardProps}
            goToPreviousStep={goToPreviousStep}
            stepNumber={2}
          />
        </Form>
      </Formik>,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    await flushPromises();
    expect(goToPreviousStep).toHaveBeenCalled();
  });

  test('clicking `Previous step` button does not call `goToPreviousStep` until `onPreviousStep` completes', async () => {
    jest.useFakeTimers();

    const handlePreviousStep = jest.fn(
      () => new Promise(resolve => setTimeout(resolve, 500)),
    );
    const goToPreviousStep = jest.fn();

    render(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions
            {...wizardProps}
            goToPreviousStep={goToPreviousStep}
            onPreviousStep={handlePreviousStep}
            stepNumber={2}
          />
        </Form>
      </Formik>,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    await flushPromises();
    expect(goToPreviousStep).not.toHaveBeenCalled();

    jest.advanceTimersByTime(500);

    await flushPromises();
    expect(goToPreviousStep).toHaveBeenCalled();

    jest.useRealTimers();
  });

  test('preventing default `Previous step` button event does not call `goToPreviousStep` handler', async () => {
    const goToPreviousStep = jest.fn();

    render(
      <Formik onSubmit={noop} initialValues={{}}>
        <Form id="test-form">
          <WizardStepFormActions
            {...wizardProps}
            goToPreviousStep={goToPreviousStep}
            onPreviousStep={event => event.preventDefault()}
            stepNumber={2}
          />
        </Form>
      </Formik>,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    await flushPromises();
    expect(goToPreviousStep).not.toHaveBeenCalled();
  });
});
