import { Formik, FormikProps } from 'formik';
import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import WizardStepFormActions from '../WizardStepFormActions';

describe('WizardStepFormActions', () => {
  test('when submitting, `Next step` button changes to `Submitting`', async () => {
    const { queryByText } = render(
      <Formik
        onSubmit={() => {}}
        initialValues={{}}
        render={(form: FormikProps<{}>) => {
          return (
            <WizardStepFormActions
              form={{ ...form, isSubmitting: true }}
              formId="form"
              goToPreviousStep={() => {}}
              onPreviousStep={() => {}}
              stepNumber={1}
            />
          );
        }}
      />,
    );

    expect(queryByText('Next step')).toBeNull();
    expect(queryByText('Submitting')).toHaveAttribute('disabled');
  });

  test('when submitting, `Next step` button changes to custom submitting text', () => {
    const { queryByText } = render(
      <Formik
        onSubmit={() => {}}
        initialValues={{}}
        render={(form: FormikProps<{}>) => {
          return (
            <WizardStepFormActions
              form={{ ...form, isSubmitting: true }}
              formId="form"
              goToPreviousStep={() => {}}
              onPreviousStep={() => {}}
              stepNumber={1}
              submittingText="Processing..."
            />
          );
        }}
      />,
    );

    expect(queryByText('Next step')).toBeNull();
    expect(queryByText('Processing...')).toHaveAttribute('disabled');
  });

  test('does not render `Previous step` button when not on step 2', () => {
    const { queryByText, rerender } = render(
      <Formik
        onSubmit={() => {}}
        initialValues={{}}
        render={(form: FormikProps<{}>) => (
          <WizardStepFormActions
            form={form}
            formId="form"
            goToPreviousStep={() => {}}
            onPreviousStep={() => {}}
            stepNumber={1}
          />
        )}
      />,
    );

    expect(queryByText('Previous step')).toBeNull();

    rerender(
      <Formik
        onSubmit={() => {}}
        initialValues={{}}
        render={(form: FormikProps<{}>) => (
          <WizardStepFormActions
            form={form}
            formId="form"
            goToPreviousStep={() => {}}
            onPreviousStep={() => {}}
            stepNumber={2}
          />
        )}
      />,
    );

    expect(queryByText('Previous step')).not.toBeNull();
  });

  test('clicking `Previous step` button calls `goToPreviousStep` handler', () => {
    const goToPreviousStep = jest.fn();

    const { getByText } = render(
      <Formik
        onSubmit={() => {}}
        initialValues={{}}
        render={(form: FormikProps<{}>) => (
          <WizardStepFormActions
            form={form}
            formId="form"
            goToPreviousStep={goToPreviousStep}
            onPreviousStep={() => {}}
            stepNumber={2}
          />
        )}
      />,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    fireEvent.click(getByText('Previous step'));

    expect(goToPreviousStep).toHaveBeenCalled();
  });

  test('preventing default `Previous step` button event does not call `goToPreviousStep` handler', () => {
    const goToPreviousStep = jest.fn();

    const { getByText } = render(
      <Formik
        onSubmit={() => {}}
        initialValues={{}}
        render={(form: FormikProps<{}>) => (
          <WizardStepFormActions
            form={form}
            formId="form"
            goToPreviousStep={goToPreviousStep}
            onPreviousStep={event => event.preventDefault()}
            stepNumber={2}
          />
        )}
      />,
    );

    expect(goToPreviousStep).not.toHaveBeenCalled();

    fireEvent.click(getByText('Previous step'));

    expect(goToPreviousStep).not.toHaveBeenCalled();
  });
});
