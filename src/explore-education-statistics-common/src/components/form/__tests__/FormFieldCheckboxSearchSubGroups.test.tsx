import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { fireEvent, render, screen, within } from '@testing-library/react';
import { Formik } from 'formik';
import React from 'react';
import FormFieldCheckboxSearchSubGroups from '../FormFieldCheckboxSearchSubGroups';

describe('FormFieldCheckboxSearchSubGroups', () => {
  interface FormValues {
    test: string[];
  }

  test('checking option checks it', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
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

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(true);
  });

  test('un-checking option un-checks it', () => {
    render(
      <Formik
        initialValues={{
          test: ['1'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
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

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(false);
  });

  test('clicking `Select all options` button checks all values for all groups', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [{ value: '1', label: 'Checkbox 1' }],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    fireEvent.click(
      screen.getByRole('button', {
        name: 'Select all 3 options',
      }),
    );

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    expect(
      screen.getByRole('button', {
        name: 'Unselect all 3 options',
      }),
    ).toBeInTheDocument();
  });

  test('clicking `Unselect all options` button un-checks all options for all groups', () => {
    render(
      <Formik
        initialValues={{
          test: ['1', '2', '3'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [{ value: '1', label: 'Checkbox 1' }],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    fireEvent.click(
      screen.getByRole('button', {
        name: 'Unselect all 3 options',
      }),
    );

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
  });

  test('checking all checkboxes for all groups renders `Unselect all options` button', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [{ value: '1', label: 'Checkbox 1' }],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    expect(
      screen.queryByRole('button', {
        name: 'Unselect all 3 options',
      }),
    ).not.toBeInTheDocument();

    fireEvent.click(screen.getByLabelText('Checkbox 1'));
    fireEvent.click(screen.getByLabelText('Checkbox 2'));
    fireEvent.click(screen.getByLabelText('Checkbox 3'));

    expect(
      screen.getByRole('button', {
        name: 'Unselect all 3 options',
      }),
    ).toBeInTheDocument();
  });

  test('clicking `Select all subgroup options` button for a group checks all values for that group', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [{ value: '1', label: 'Checkbox 1' }],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const group1 = within(screen.getByRole('group', { name: 'Group A' }));
    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox1 = group1.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    fireEvent.click(
      group2.getByRole('button', {
        name: 'Select all 2 subgroup options',
      }),
    );

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);
  });

  test('clicking `Unselect all` button for a group un-checks all values for that group', () => {
    render(
      <Formik
        initialValues={{
          test: ['2', '3'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [{ value: '1', label: 'Checkbox 1' }],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const group1 = within(screen.getByRole('group', { name: 'Group A' }));
    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox1 = group1.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    fireEvent.click(
      group2.getByRole('button', { name: 'Unselect all 2 subgroup options' }),
    );

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
  });

  test('checking all options for a group renders corresponding `Unselect all subgroup options` button', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [{ value: '1', label: 'Checkbox 1' }],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    expect(
      group2.getByRole('button', { name: 'Select all 2 subgroup options' }),
    ).toBeInTheDocument();
    expect(
      group2.queryByRole('button', { name: 'Unselect all 2 subgroup options' }),
    ).toBeNull();

    fireEvent.click(checkbox2);
    fireEvent.click(checkbox3);

    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    expect(
      group2.queryByRole('button', { name: 'Select all 2 subgroup options' }),
    ).toBeNull();
    expect(
      group2.getByRole('button', { name: 'Unselect all 2 subgroup options' }),
    ).not.toBeNull();
  });

  test('un-checking any options renders the corresponding `Unselect all options` button', () => {
    render(
      <Formik
        initialValues={{
          test: ['2', '3'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [{ value: '1', label: 'Checkbox 1' }],
              },
              {
                legend: 'Group B',
                options: [
                  { value: '2', label: 'Checkbox 2' },
                  { value: '3', label: 'Checkbox 3' },
                ],
              },
            ]}
          />
        )}
      </Formik>,
    );

    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    expect(
      group2.queryByRole('button', { name: 'Select all 2 subgroup options' }),
    ).toBeNull();
    expect(
      group2.getByRole('button', { name: 'Unselect all 2 subgroup options' }),
    ).toBeInTheDocument();

    fireEvent.click(checkbox2);

    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(true);

    expect(
      group2.getByRole('button', { name: 'Select all 2 subgroup options' }),
    ).toBeInTheDocument();
    expect(
      group2.queryByRole('name', { name: 'Unselect all 2 subgroup options' }),
    ).toBeNull();
  });

  describe('error messages', () => {
    test('does not display validation message when checkboxes are untouched', async () => {
      render(
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
            <FormFieldCheckboxSearchSubGroups<FormValues>
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

      expect(screen.queryByText('Select at least one option')).toBeNull();
    });

    test('displays validation message when no checkboxes are checked', async () => {
      render(
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
            <FormFieldCheckboxSearchSubGroups<FormValues>
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

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(screen.queryByText('Select at least one option')).toBeNull();

      fireEvent.click(checkbox);

      await waitFor(() => {
        expect(checkbox.checked).toBe(false);

        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false', () => {
      render(
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
            <FormFieldCheckboxSearchSubGroups<FormValues>
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

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(screen.queryByText('Select at least one option')).toBeNull();

      fireEvent.click(checkbox);

      expect(checkbox.checked).toBe(false);
      expect(screen.queryByText('Select at least one option')).toBeNull();
    });
  });

  test('providing a search term does not remove checkboxes that have already been checked', () => {
    jest.useFakeTimers();

    render(
      <Formik
        initialValues={{
          test: ['1'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchSubGroups<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            searchLabel="Search options"
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

    const searchInput = screen.getByLabelText(
      'Search options',
    ) as HTMLInputElement;

    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('checked');

    fireEvent.change(searchInput, {
      target: {
        value: '2',
      },
    });

    jest.runAllTimers();

    expect(screen.getAllByLabelText(/Checkbox/)).toHaveLength(2);
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('checked');
    expect(screen.getByLabelText('Checkbox 2')).not.toHaveAttribute('checked');
  });
});
