import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import FormCheckboxGroup from '../FormCheckboxGroup';

describe('FormCheckboxGroup', () => {
  test('renders list of checkboxes in correct order', () => {
    const { container, getAllByLabelText } = render(
      <FormCheckboxGroup
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
        name="test-checkboxes"
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          {
            checked: true,
            id: 'checkbox-2',
            label: 'Test checkbox 2',
            value: '2',
          },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const checkboxes = getAllByLabelText(/Test checkbox/);

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0]).not.toHaveAttribute('checked');
    expect(checkboxes[1]).toHaveAttribute('checked');
    expect(checkboxes[2]).not.toHaveAttribute('checked');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('clicking a checkbox checks it', () => {
    const { getByLabelText } = render(
      <FormCheckboxGroup
        name="test-checkboxes"
        options={[
          { id: 'checkbox-1', label: 'Test checkbox', value: '1' },
        ]}
      />,
    );

    const checkbox = getByLabelText('Test checkbox') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(true)
  });

  test('clicking a checkbox un-checks it', () => {
    const { container, getByLabelText } = render(
      <FormCheckboxGroup
        name="test-checkboxes"
        options={[
          { id: 'checkbox-1', label: 'Test checkbox', value: '1', checked: true },
        ]}
      />,
    );

    const checkbox = getByLabelText('Test checkbox') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(false);
  });
});
