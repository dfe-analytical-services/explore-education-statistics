import FormFieldCheckboxSearchSubGroups from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FormFieldCheckboxSearchSubGroups', () => {
  test('renders with correct default ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxSearchSubGroups
          name="test"
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
      </FormProvider>,
    );

    const fieldset = screen.getByRole('group', { name: 'Test checkboxes' });
    const groupA = screen.getByRole('group', { name: 'Group A' });
    const groupB = screen.getByRole('group', { name: 'Group B' });

    expect(fieldset).toHaveAttribute('id', 'test');
    expect(groupA).toHaveAttribute('id', 'test-options-1');
    expect(groupB).toHaveAttribute('id', 'test-options-2');

    expect(within(fieldset).getByRole('textbox')).toHaveAttribute(
      'id',
      'test-search',
    );

    expect(within(fieldset).getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'test-options-1-1',
    );
    expect(within(groupA).getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'test-options-1-2',
    );
    expect(within(groupB).getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'test-options-2-3',
    );
    expect(within(groupB).getByLabelText('Checkbox 4')).toHaveAttribute(
      'id',
      'test-options-2-4',
    );
  });

  test('renders with correct default ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const fieldset = screen.getByRole('group', { name: 'Test checkboxes' });
    const groupA = screen.getByRole('group', { name: 'Group A' });
    const groupB = screen.getByRole('group', { name: 'Group B' });

    expect(fieldset).toHaveAttribute('id', 'testForm-test');
    expect(groupA).toHaveAttribute('id', 'testForm-test-options-1');
    expect(groupB).toHaveAttribute('id', 'testForm-test-options-2');

    expect(within(fieldset).getByRole('textbox')).toHaveAttribute(
      'id',
      'testForm-test-search',
    );

    expect(within(fieldset).getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'testForm-test-options-1-1',
    );
    expect(within(groupA).getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'testForm-test-options-1-2',
    );
    expect(within(groupB).getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'testForm-test-options-2-3',
    );
    expect(within(groupB).getByLabelText('Checkbox 4')).toHaveAttribute(
      'id',
      'testForm-test-options-2-4',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            id="customId"
            name="test"
            legend="Test checkboxes"
            options={[
              {
                legend: 'Group A',
                options: [
                  { value: '1', label: 'Checkbox 1' },
                  { id: 'customOption2', value: '2', label: 'Checkbox 2' },
                ],
              },
              {
                legend: 'Group B',
                id: 'customGroup',
                options: [
                  { value: '3', label: 'Checkbox 3' },
                  { id: 'customOption4', value: '4', label: 'Checkbox 4' },
                ],
              },
            ]}
          />
        </Form>
      </FormProvider>,
    );
    const fieldset = screen.getByRole('group', { name: 'Test checkboxes' });
    const groupA = screen.getByRole('group', { name: 'Group A' });
    const groupB = screen.getByRole('group', { name: 'Group B' });

    expect(fieldset).toHaveAttribute('id', 'testForm-customId');
    expect(groupA).toHaveAttribute('id', 'testForm-customId-options-1');
    expect(groupB).toHaveAttribute('id', 'testForm-customId-customGroup');

    expect(within(fieldset).getByRole('textbox')).toHaveAttribute(
      'id',
      'testForm-customId-search',
    );

    expect(within(fieldset).getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'testForm-customId-options-1-1',
    );
    expect(within(groupA).getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'testForm-customId-options-1-customOption2',
    );
    expect(within(groupB).getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'testForm-customId-customGroup-3',
    );
    expect(within(groupB).getByLabelText('Checkbox 4')).toHaveAttribute(
      'id',
      'testForm-customId-customGroup-customOption4',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxSearchSubGroups
          id="customId"
          name="test"
          legend="Test checkboxes"
          options={[
            {
              legend: 'Group A',
              options: [
                { value: '1', label: 'Checkbox 1' },
                { id: 'customOption2', value: '2', label: 'Checkbox 2' },
              ],
            },
            {
              legend: 'Group B',
              id: 'customGroup',
              options: [
                { value: '3', label: 'Checkbox 3' },
                { id: 'customOption4', value: '4', label: 'Checkbox 4' },
              ],
            },
          ]}
        />
      </FormProvider>,
    );

    const fieldset = screen.getByRole('group', { name: 'Test checkboxes' });
    const groupA = screen.getByRole('group', { name: 'Group A' });
    const groupB = screen.getByRole('group', { name: 'Group B' });

    expect(fieldset).toHaveAttribute('id', 'customId');
    expect(groupA).toHaveAttribute('id', 'customId-options-1');
    expect(groupB).toHaveAttribute('id', 'customId-customGroup');

    expect(within(fieldset).getByRole('textbox')).toHaveAttribute(
      'id',
      'customId-search',
    );

    expect(within(fieldset).getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'customId-options-1-1',
    );
    expect(within(groupA).getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'customId-options-1-customOption2',
    );
    expect(within(groupB).getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'customId-customGroup-3',
    );
    expect(within(groupB).getByLabelText('Checkbox 4')).toHaveAttribute(
      'id',
      'customId-customGroup-customOption4',
    );
  });

  test('checking option checks it', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    await userEvent.click(checkbox);

    expect(checkbox.checked).toBe(true);
  });

  test('un-checking option un-checks it', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1'],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
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
        </Form>
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    await userEvent.click(checkbox);

    expect(checkbox.checked).toBe(false);
  });

  test('clicking `Select all options` button checks all values for all groups', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    await userEvent.click(
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

  test('clicking `Unselect all options` button un-checks all options for all groups', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1', '2', '3'],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    await userEvent.click(
      screen.getByRole('button', {
        name: 'Unselect all 3 options',
      }),
    );

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
  });

  test('checking all checkboxes for all groups renders `Unselect all options` button', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    expect(
      screen.queryByRole('button', {
        name: 'Unselect all 3 options',
      }),
    ).not.toBeInTheDocument();

    await userEvent.click(screen.getByLabelText('Checkbox 1'));
    await userEvent.click(screen.getByLabelText('Checkbox 2'));
    await userEvent.click(screen.getByLabelText('Checkbox 3'));

    expect(
      screen.getByRole('button', {
        name: 'Unselect all 3 options',
      }),
    ).toBeInTheDocument();
  });

  test('clicking `Select all subgroup options` button for a group checks all values for that group', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const group1 = within(screen.getByRole('group', { name: 'Group A' }));
    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox1 = group1.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    await userEvent.click(
      group2.getByRole('button', {
        name: /Select all 2 subgroup options/i,
      }),
    );

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);
  });

  test('clicking `Unselect all` button for a group un-checks all values for that group', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['2', '3'],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const group1 = within(screen.getByRole('group', { name: 'Group A' }));
    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox1 = group1.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    await userEvent.click(
      group2.getByRole('button', { name: /Unselect all 2 subgroup options/i }),
    );

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
  });

  test('checking all options for a group renders corresponding `Unselect all subgroup options` button', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    expect(
      group2.getByRole('button', { name: /Select all 2 subgroup options/i }),
    ).toBeInTheDocument();
    expect(
      group2.queryByRole('button', { name: 'Unselect all 2 subgroup options' }),
    ).not.toBeInTheDocument();

    await userEvent.click(checkbox2);
    await userEvent.click(checkbox3);

    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    expect(
      group2.queryByRole('button', { name: 'Select all 2 subgroup options' }),
    ).not.toBeInTheDocument();
    expect(
      group2.getByRole('button', { name: /Unselect all 2 subgroup options/i }),
    ).toBeInTheDocument();
  });

  test('un-checking any options renders the corresponding `Unselect all options` button', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['2', '3'],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const checkbox2 = group2.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = group2.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    expect(
      group2.queryByRole('button', { name: 'Select all 2 subgroup options' }),
    ).not.toBeInTheDocument();
    expect(
      group2.getByRole('button', { name: /Unselect all 2 subgroup options/i }),
    ).toBeInTheDocument();

    await userEvent.click(checkbox2);

    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(true);

    expect(
      group2.getByRole('button', { name: /Select all 2 subgroup options/i }),
    ).toBeInTheDocument();
    expect(
      group2.queryByRole('name', { name: 'Unselect all 2 subgroup options' }),
    ).not.toBeInTheDocument();
  });

  describe('error messages', () => {
    test('does not display validation message when checkboxes are untouched', async () => {
      render(
        <FormProvider
          initialValues={{
            test: [],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <Form id="testForm" onSubmit={Promise.resolve}>
            <FormFieldCheckboxSearchSubGroups
              name="test"
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
          </Form>
        </FormProvider>,
      );

      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();
    });

    test('displays validation message when no checkboxes are checked', async () => {
      render(
        <FormProvider
          initialValues={{
            test: ['1'],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <Form id="testForm" onSubmit={Promise.resolve}>
            <FormFieldCheckboxSearchSubGroups
              name="test"
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
            <button type="submit">Submit</button>
          </Form>
        </FormProvider>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(screen.queryByText('Select at least one option')).toBeNull();

      await userEvent.click(checkbox);
      expect(checkbox.checked).toBe(false);

      await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

      await waitFor(() => {
        expect(
          screen.queryAllByText('Select at least one option')[0],
        ).toHaveAttribute('href', '#testForm-test');
      });
    });

    test('does not display validation message when `showError` is false', async () => {
      render(
        <FormProvider
          initialValues={{
            test: ['1'],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <Form id="testForm" onSubmit={Promise.resolve}>
            <FormFieldCheckboxSearchSubGroups
              name="test"
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
              showError={false}
            />
          </Form>
        </FormProvider>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      await userEvent.click(checkbox);

      expect(checkbox.checked).toBe(false);
      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();
    });
  });

  test('providing a search term does not remove checkboxes that have already been checked', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1'],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchSubGroups
            name="test"
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
        </Form>
      </FormProvider>,
    );

    const searchInput = screen.getByLabelText(
      'Search options',
    ) as HTMLInputElement;

    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('checked');

    await userEvent.type(searchInput, '2');

    await waitFor(() =>
      expect(screen.queryByLabelText('Checkbox 3')).not.toBeInTheDocument(),
    );

    expect(screen.getAllByLabelText(/Checkbox/)).toHaveLength(2);
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('checked');
    expect(screen.getByLabelText('Checkbox 2')).not.toHaveAttribute('checked');
  });
});
