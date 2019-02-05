import React, { Component, ReactNode } from 'react';
import { fireEvent, render } from 'react-testing-library';
import { CheckboxChangeEventHandler } from '../FormCheckbox';
import FormCheckboxGroup from '../FormCheckboxGroup';

describe('FormCheckboxGroup', () => {
  // Stateful container component to wrap our checkbox groups with
  class CheckboxWrapper extends Component<
    {
      children: (
        state: string[],
        handleChange: CheckboxChangeEventHandler,
      ) => ReactNode;
      initialState: string[];
    },
    {
      values: string[];
    }
  > {
    public state = {
      values: [...this.props.initialState],
    };

    private handleChange: CheckboxChangeEventHandler = event => {
      const values: string[] = [...this.state.values];

      if (event.target.checked) {
        values.push(event.target.value);
      } else {
        values.splice(values.indexOf(event.target.value), 1);
      }

      this.setState({ values });
    };

    public render() {
      return this.props.children(this.state.values, this.handleChange);
    }
  }

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

  test('clicking an unchecked checkbox checks it', () => {
    const { getByLabelText } = render(
      <CheckboxWrapper initialState={[]}>
        {(state, handleChange) => (
          <FormCheckboxGroup
            value={state}
            onChange={handleChange}
            options={[{ id: 'checkbox-1', label: 'Test checkbox', value: '1' }]}
            id="test-checkboxes"
            name="test-checkboxes"
          />
        )}
      </CheckboxWrapper>,
    );

    const checkbox = getByLabelText('Test checkbox') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(true);
  });

  test('clicking a pre-checked checkbox un-checks it', () => {
    const { getByLabelText } = render(
      <CheckboxWrapper initialState={['1']}>
        {(state, handleChange) => (
          <FormCheckboxGroup
            value={state}
            onChange={handleChange}
            options={[{ id: 'checkbox-1', label: 'Test checkbox', value: '1' }]}
            id="test-checkboxes"
            name="test-checkboxes"
          />
        )}
      </CheckboxWrapper>,
    );

    const checkbox = getByLabelText('Test checkbox') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    fireEvent.click(checkbox);

    expect(checkbox.checked).toBe(false);
  });

  test('clicking multiple checkboxes checks them all', () => {
    const { getByLabelText } = render(
      <CheckboxWrapper initialState={[]}>
        {(state, handleChange) => (
          <FormCheckboxGroup
            value={state}
            id="test-checkboxes"
            name="test-checkboxes"
            onChange={handleChange}
            options={[
              { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
            ]}
          />
        )}
      </CheckboxWrapper>,
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

  test('renders with `Select all` checkbox when `onAllChange` is set', () => {
    const noop = () => null;

    const { container, getByLabelText } = render(
      <FormCheckboxGroup
        value={[]}
        id="test-checkboxes"
        name="test-checkboxes"
        onAllChange={noop}
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    expect(getByLabelText('Select all')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('checking all options checks the `Select all` checkbox', () => {
    const noop = () => null;

    const { getByLabelText } = render(
      <CheckboxWrapper initialState={['2', '3']}>
        {(state, handleChange) => (
          <FormCheckboxGroup
            value={state}
            id="test-checkboxes"
            name="test-checkboxes"
            onAllChange={noop}
            onChange={handleChange}
            options={[
              { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
            ]}
          />
        )}
      </CheckboxWrapper>,
    );

    const checkbox1 = getByLabelText('Test checkbox 1') as HTMLInputElement;
    const selectAll = getByLabelText('Select all') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(selectAll.checked).toBe(false);

    fireEvent.click(checkbox1);

    expect(checkbox1.checked).toBe(true);
    expect(selectAll.checked).toBe(true);
  });

  test('un-checking any options un-checks the `Select all` checkbox ', () => {
    const noop = () => null;

    const { getByLabelText } = render(
      <CheckboxWrapper initialState={['1', '2', '3']}>
        {(state, handleChange) => (
          <FormCheckboxGroup
            value={state}
            id="test-checkboxes"
            name="test-checkboxes"
            onAllChange={noop}
            onChange={handleChange}
            options={[
              { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
            ]}
          />
        )}
      </CheckboxWrapper>,
    );

    const checkbox1 = getByLabelText('Test checkbox 1') as HTMLInputElement;
    const selectAll = getByLabelText('Select all') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(selectAll.checked).toBe(true);

    fireEvent.click(checkbox1);

    expect(checkbox1.checked).toBe(false);
    expect(selectAll.checked).toBe(false);
  });
});
