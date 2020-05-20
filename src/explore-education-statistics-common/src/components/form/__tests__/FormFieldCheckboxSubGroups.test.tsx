import Yup from '@common/validation/yup';
import { fireEvent, render, wait } from '@testing-library/react';
import { Formik } from 'formik';
import React from 'react';
import FormFieldCheckboxSubGroups from '../FormFieldCheckboxSubGroups';

describe('FormFieldCheckboxSubGroups', () => {
  interface FormValues {
    test: string[];
  }

  test('checking option checks it', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { value: '2', label: 'Checkbox 2' },
                ],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '3', label: 'Checkbox 3' },
                  { value: '4', label: 'Checkbox 4' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
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
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { value: '2', label: 'Checkbox 2' },
                ],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '3', label: 'Checkbox 3' },
                  { value: '4', label: 'Checkbox 4' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox = getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    fireEvent.click(checkbox);

    await wait();

    expect(checkbox.checked).toBe(false);
  });

  test('clicking `Select all 2 options` button checks all values for that group', () => {
    const { getByLabelText, getByText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { value: '2', label: 'Checkbox 2' },
                ],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '3', label: 'Checkbox 3' },
                  { value: '4', label: 'Checkbox 4' },
                  { value: '5', label: 'Checkbox 5' },
                ],
              },
            ]}
            selectAll
          />
        )}
      </Formik>,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = getByLabelText('Checkbox 3') as HTMLInputElement;
    const checkbox4 = getByLabelText('Checkbox 4') as HTMLInputElement;
    const checkbox5 = getByLabelText('Checkbox 5') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
    expect(checkbox4.checked).toBe(false);
    expect(checkbox5.checked).toBe(false);

    fireEvent.click(getByText('Select all 2 options'));

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(false);
    expect(checkbox4.checked).toBe(false);
    expect(checkbox5.checked).toBe(false);
  });

  test('clicking all `Select all` buttons checks all values for every group', () => {
    const { getByLabelText, getByText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '4', label: 'Checkbox 4' },
                  { value: '5', label: 'Checkbox 5' },
                ],
              },
            ]}
            selectAll
          />
        )}
      </Formik>,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = getByLabelText('Checkbox 3') as HTMLInputElement;
    const checkbox4 = getByLabelText('Checkbox 4') as HTMLInputElement;
    const checkbox5 = getByLabelText('Checkbox 5') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
    expect(checkbox4.checked).toBe(false);
    expect(checkbox5.checked).toBe(false);

    fireEvent.click(getByText('Select all 3 options'));
    fireEvent.click(getByText('Select all 2 options'));

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);
    expect(checkbox4.checked).toBe(true);
    expect(checkbox5.checked).toBe(true);
  });

  test('clicking `Unselect all 2 options` button un-checks all values for that group', () => {
    const { getByLabelText, getByText } = render(
      <Formik
        initialValues={{
          test: ['1', '2'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { value: '2', label: 'Checkbox 2' },
                ],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '3', label: 'Checkbox 3' },
                  { value: '4', label: 'Checkbox 4' },
                  { value: '5', label: 'Checkbox 5' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = getByLabelText('Checkbox 3') as HTMLInputElement;
    const checkbox4 = getByLabelText('Checkbox 4') as HTMLInputElement;
    const checkbox5 = getByLabelText('Checkbox 5') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(false);
    expect(checkbox4.checked).toBe(false);
    expect(checkbox5.checked).toBe(false);

    fireEvent.click(getByText('Unselect all 2 options'));

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
    expect(checkbox4.checked).toBe(false);
    expect(checkbox5.checked).toBe(false);
  });

  test('checking all options for a group renders the corresponding `Unselect all options` button', async () => {
    const { getByLabelText, queryByText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { value: '2', label: 'Checkbox 2' },
                ],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '3', label: 'Checkbox 3' },
                  { value: '4', label: 'Checkbox 4' },
                  { value: '5', label: 'Checkbox 5' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);

    expect(queryByText('Unselect all 2 options')).toBeNull();
    expect(queryByText('Select all 2 options')).not.toBeNull();

    fireEvent.click(checkbox1);
    fireEvent.click(checkbox2);

    await wait();

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);

    expect(queryByText('Unselect all 2 options')).not.toBeNull();
    expect(queryByText('Select all 2 options')).toBeNull();
  });

  test('un-checking any options renders the corresponding `Select all options` button', async () => {
    const { getByLabelText, queryByText } = render(
      <Formik
        initialValues={{
          test: ['1', '2'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { value: '2', label: 'Checkbox 2' },
                ],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '3', label: 'Checkbox 3' },
                  { value: '4', label: 'Checkbox 4' },
                  { value: '5', label: 'Checkbox 5' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);

    expect(queryByText('Unselect all 2 options')).not.toBeNull();
    expect(queryByText('Select all 2 options')).toBeNull();

    fireEvent.click(checkbox1);

    await wait();

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(true);

    expect(queryByText('Unselect all 2 options')).toBeNull();
    expect(queryByText('Select all 2 options')).not.toBeNull();
  });

  describe('error messages', () => {
    test('does not display validation message when checkboxes are untouched', async () => {
      const { queryByText } = render(
        <Formik
          initialValues={{
            test: [],
          }}
          onSubmit={() => undefined}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxSubGroups<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                {
                  legend: 'Group A',
                  options: [
                    { value: '1', label: 'Checkbox 1' },
                    { value: '2', label: 'Checkbox 2' },
                  ],
                },
                {
                  legend: 'Group B',
                  options: [
                    { value: '3', label: 'Checkbox 3' },
                    { value: '4', label: 'Checkbox 4' },
                  ],
                },
              ]}
            />
          )}
        </Formik>,
      );

      expect(queryByText('Select at least one option')).toBeNull();
    });

    test('displays validation message when no checkboxes are checked', async () => {
      const { getByLabelText, queryByText } = render(
        <Formik
          initialValues={{
            test: ['1'],
          }}
          initialTouched={{
            test: true,
          }}
          onSubmit={() => undefined}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxSubGroups<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                {
                  legend: 'Group A',
                  options: [
                    { value: '1', label: 'Checkbox 1' },
                    { value: '2', label: 'Checkbox 2' },
                  ],
                },
                {
                  legend: 'Group B',
                  options: [
                    { value: '3', label: 'Checkbox 3' },
                    { value: '4', label: 'Checkbox 4' },
                  ],
                },
              ]}
            />
          )}
        </Formik>,
      );

      const checkbox = getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(queryByText('Select at least one option')).toBeNull();

      fireEvent.click(checkbox);

      await wait();

      expect(checkbox.checked).toBe(false);
      expect(queryByText('Select at least one option')).not.toBeNull();
    });

    test('does not display validation message when `showError` is false', async () => {
      const { getByLabelText, queryByText } = render(
        <Formik
          initialValues={{
            test: ['1'],
          }}
          onSubmit={() => undefined}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxSubGroups<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              showError={false}
              options={[
                {
                  legend: 'Group A',
                  options: [
                    { value: '1', label: 'Checkbox 1' },
                    { value: '2', label: 'Checkbox 2' },
                  ],
                },
                {
                  legend: 'Group B',
                  options: [
                    { value: '3', label: 'Checkbox 3' },
                    { value: '4', label: 'Checkbox 4' },
                  ],
                },
              ]}
            />
          )}
        </Formik>,
      );

      const checkbox = getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(queryByText('Select at least one option')).toBeNull();

      fireEvent.click(checkbox);

      await wait();

      expect(checkbox.checked).toBe(false);
      expect(queryByText('Select at least one option')).toBeNull();
    });
  });
});
