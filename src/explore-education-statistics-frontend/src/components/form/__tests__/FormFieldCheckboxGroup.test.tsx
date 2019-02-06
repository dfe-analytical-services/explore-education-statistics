import { Formik, FormikProps } from 'formik';
import React from 'react';
import { fireEvent, render, wait } from 'react-testing-library';
import FormFieldCheckboxGroup from '../FormFieldCheckboxGroup';

describe('FormFieldCheckboxGroup', () => {
  interface FormValues {
    test: string[];
  }

  test('checking option checks it', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
            value={props.values.test}
          />
        )}
      />,
    );

    const checkbox = getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    fireEvent.click(checkbox);

    await wait();

    expect(checkbox.checked).toBe(true);
  });

  test('un-checking option un-checks it', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: ['1'],
        }}
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
            value={props.values.test}
          />
        )}
      />,
    );

    const checkbox = getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    fireEvent.click(checkbox);

    await wait();

    expect(checkbox.checked).toBe(false);
  });

  test('checking `Select all` option checks all values', () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
            value={props.values.test}
            selectAll
          />
        )}
      />,
    );

    fireEvent.click(getByLabelText('Select all'));

    expect((getByLabelText('Checkbox 1') as HTMLInputElement).checked).toBe(
      true,
    );
    expect((getByLabelText('Checkbox 2') as HTMLInputElement).checked).toBe(
      true,
    );
    expect((getByLabelText('Checkbox 3') as HTMLInputElement).checked).toBe(
      true,
    );
  });

  test('un-checking `Select all` option un-checks all values', () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: ['1', '2', '3'],
        }}
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            value={props.values.test}
            selectAll
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        )}
      />,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    fireEvent.click(getByLabelText('Select all'));

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
  });

  test('checking all options checks the `Select all` checkbox', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            value={props.values.test}
            selectAll
            options={[
              { id: 'checkbox-1', label: 'Checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Checkbox 3', value: '3' },
            ]}
          />
        )}
      />,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = getByLabelText('Checkbox 3') as HTMLInputElement;
    const selectAll = getByLabelText('Select all') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(selectAll.checked).toBe(false);

    fireEvent.click(checkbox1);
    fireEvent.click(checkbox2);
    fireEvent.click(checkbox3);

    await wait();

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);
    expect(selectAll.checked).toBe(true);
  });

  test('un-checking any options un-checks the `Select all` checkbox ', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: ['1', '2', '3'],
        }}
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            value={props.values.test}
            selectAll
            options={[
              { id: 'checkbox-1', label: 'Checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Checkbox 3', value: '3' },
            ]}
          />
        )}
      />,
    );

    const checkbox = getByLabelText('Checkbox 1') as HTMLInputElement;
    const selectAll = getByLabelText('Select all') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);
    expect(selectAll.checked).toBe(true);

    fireEvent.click(checkbox);

    await wait();

    expect(checkbox.checked).toBe(false);
    expect(selectAll.checked).toBe(false);
  });
});
