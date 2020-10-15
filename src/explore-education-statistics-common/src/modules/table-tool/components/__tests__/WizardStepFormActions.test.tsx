import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Form, Formik } from 'formik';
import React from 'react';
import WizardStepFormActions from '../WizardStepFormActions';

describe('WizardStepFormActions', () => {
  test('when submitting, `Next step` button changes to `Submitting`', async () => {
    render(
      <Formik onSubmit={() => {}} initialValues={{}}>
        <Form>
          <WizardStepFormActions
            formId="form"
            goToPreviousStep={() => {}}
            onPreviousStep={() => {}}
            stepNumber={1}
          />
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByText('Next step'));

    expect(screen.queryByText('Next step')).toBeNull();
    expect(screen.getByText('Submitting')).toHaveAttribute('disabled');
  });

  test('when submitting, `Next step` button changes to custom submitting text', () => {
    render(
      <Formik onSubmit={() => {}} initialValues={{}}>
        <Form>
          <WizardStepFormActions
            formId="form"
            goToPreviousStep={() => {}}
            onPreviousStep={() => {}}
            stepNumber={1}
            submittingText="Processing..."
          />
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByText('Next step'));

    expect(screen.queryByText('Next step')).toBeNull();
    expect(screen.getByText('Processing...')).toHaveAttribute('disabled');
  });

  test('does not render `Previous step` button when not on step 2', () => {
    const { rerender } = render(
      <Formik onSubmit={() => {}} initialValues={{}}>
        <Form>
          <WizardStepFormActions
            formId="form"
            goToPreviousStep={() => {}}
            onPreviousStep={() => {}}
            stepNumber={1}
          />
        </Form>
      </Formik>,
    );

    expect(screen.queryByText('Previous step')).toBeNull();

    rerender(
      <Formik onSubmit={() => {}} initialValues={{}}>
        <Form>
          <WizardStepFormActions
            formId="form"
            goToPreviousStep={() => {}}
            onPreviousStep={() => {}}
            stepNumber={2}
          />
        </Form>
      </Formik>,
    );

    expect(screen.queryByText('Previous step')).not.toBeNull();
  });

  test('clicking `Previous step` button calls `goToPreviousStep` handler', () => {
    const goToPreviousStep = jest.fn();

    render(
      <Formik onSubmit={() => {}} initialValues={{}}>
        <Form>
          <WizardStepFormActions
            formId="form"
            goToPreviousStep={goToPreviousStep}
            onPreviousStep={() => {}}
            stepNumber={2}
          />
        </Form>
      </Formik>,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    expect(goToPreviousStep).toHaveBeenCalled();
  });

  test('preventing default `Previous step` button event does not call `goToPreviousStep` handler', () => {
    const goToPreviousStep = jest.fn();

    render(
      <Formik onSubmit={() => {}} initialValues={{}}>
        <Form>
          <WizardStepFormActions
            formId="form"
            goToPreviousStep={goToPreviousStep}
            onPreviousStep={event => event.preventDefault()}
            stepNumber={2}
          />
        </Form>
      </Formik>,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    userEvent.click(screen.getByText('Previous step'));

    expect(goToPreviousStep).not.toHaveBeenCalled();
  });
});
