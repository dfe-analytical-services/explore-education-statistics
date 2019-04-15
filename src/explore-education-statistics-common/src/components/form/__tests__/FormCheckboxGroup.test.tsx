import React from 'react';
import { render } from 'react-testing-library';
import FormCheckboxGroup from '../FormCheckboxGroup';

describe('FormCheckboxGroup', () => {
  test('renders list of checkboxes in correct order', () => {
    const { container, getAllByLabelText } = render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const checkboxes = getAllByLabelText(/Test checkbox/);

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders checkboxes with some pre-checked', () => {
    const { container, getAllByLabelText } = render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
        value={['2']}
      />,
    );

    const checkboxes = getAllByLabelText(/Test checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0].checked).toBe(false);
    expect(checkboxes[1].checked).toBe(true);
    expect(checkboxes[2].checked).toBe(false);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with legend', () => {
    const { container, getByText } = render(
      <FormCheckboxGroup
        legend="Choose some checkboxes"
        id="test-checkboxes"
        name="test-checkboxes"
        options={[
          { id: 'radio-1', label: 'Test radio 1', value: '1' },
          { id: 'radio-2', label: 'Test radio 2', value: '2' },
          { id: 'radio-3', label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    expect(getByText('Choose some checkboxes')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders unchecked `Select all` option when `selectAll` is true', () => {
    const { container, getByLabelText } = render(
      <FormCheckboxGroup
        value={[]}
        id="test-checkboxes"
        name="test-checkboxes"
        selectAll
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const selectAllCheckbox = getByLabelText('Select all') as HTMLInputElement;

    expect(selectAllCheckbox.checked).toBe(false);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders checked `Select all` checkbox when all options are pre-checked', () => {
    const { getByLabelText } = render(
      <FormCheckboxGroup
        value={['1', '2', '3']}
        id="test-checkboxes"
        name="test-checkboxes"
        selectAll
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const selectAllCheckbox = getByLabelText('Select all') as HTMLInputElement;

    expect(selectAllCheckbox.checked).toBe(true);
  });

  test('does not render checked `Select all` checkbox when checked values do not match options', () => {
    const { getByLabelText } = render(
      <FormCheckboxGroup
        value={['4', '5', '6']}
        id="test-checkboxes"
        name="test-checkboxes"
        selectAll
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const selectAllCheckbox = getByLabelText('Select all') as HTMLInputElement;

    expect(selectAllCheckbox.checked).toBe(false);
  });

  test('does not render `Select all` checkbox when there is only one option', () => {
    const { queryByLabelText } = render(
      <FormCheckboxGroup
        value={[]}
        id="test-checkboxes"
        name="test-checkboxes"
        selectAll
        options={[{ id: 'checkbox-1', label: 'Test checkbox 1', value: '1' }]}
      />,
    );

    expect(queryByLabelText('Select all')).toBeNull();
    expect(queryByLabelText('Test checkbox 1')).not.toBeNull();
  });
});
