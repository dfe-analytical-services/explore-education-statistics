import React, { ChangeEventHandler, Component } from 'react';
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
        checkedValues={{
          1: false,
          2: true,
          3: false,
        }}
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

    const checkboxes = getAllByLabelText(/Test checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0].checked).toBe(false);
    expect(checkboxes[1].checked).toBe(true);
    expect(checkboxes[2].checked).toBe(false);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('clicking an unchecked checkbox checks it', () => {
    const { getByLabelText } = render(
      <FormCheckboxGroup
        name="test-checkboxes"
        options={[{ id: 'checkbox-1', label: 'Test checkbox', value: '1' }]}
      />,
    );

    const checkbox = getByLabelText('Test checkbox') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(true);
  });

  test('clicking a pre-checked checkbox un-checks it', () => {
    class CheckboxWrapper extends Component {
      public state = {
        isChecked: true,
      };

      private handleChange = () => {
        this.setState({
          isChecked: !this.state.isChecked,
        });
      };

      public render() {
        return (
          <FormCheckboxGroup
            checkedValues={{
              1: this.state.isChecked,
            }}
            name="test-checkboxes"
            onChange={this.handleChange}
            options={[
              {
                id: 'checkbox-1',
                label: 'Test checkbox',
                value: '1',
              },
            ]}
          />
        )
      }
    }

    const { getByLabelText } = render(<CheckboxWrapper />);

    const checkbox = getByLabelText('Test checkbox') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(false);
  });

  test('clicking multiple checkboxes checks them all', () => {
    const { getByLabelText } = render(
      <FormCheckboxGroup
        name="test-checkboxes"
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const checkbox1 = getByLabelText('Test checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Test checkbox 2') as HTMLInputElement;
    const checkbox3 = getByLabelText('Test checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    fireEvent.click(checkbox1);
    fireEvent.click(checkbox2);

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(false);
  });

  test('renders correctly with legend', () => {
    const { container, getByText } = render(
      <FormCheckboxGroup
        legend="Choose some checkboxes"
        name="test-radios"
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
});
