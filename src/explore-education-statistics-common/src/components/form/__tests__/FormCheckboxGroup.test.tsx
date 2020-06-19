import { render, screen } from '@testing-library/react';
import React from 'react';
import FormCheckboxGroup from '../FormCheckboxGroup';

describe('FormCheckboxGroup', () => {
  test('renders list of checkboxes in correct order', () => {
    const { container } = render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={[]}
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const checkboxes = screen.getAllByLabelText(/Test checkbox/);

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders list of checkboxes in reverse order', () => {
    render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={[]}
        orderDirection={['desc']}
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const checkboxes = screen.getAllByLabelText(/Test checkbox/);

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0]).toHaveAttribute('value', '3');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '1');
  });

  test('renders list of checkboxes in custom order', () => {
    render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={[]}
        order={['value']}
        orderDirection={['desc']}
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '2' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '3' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '1' },
        ]}
      />,
    );

    const radios = screen.getAllByLabelText(/Test checkbox/);

    expect(radios).toHaveLength(3);
    expect(radios[0]).toHaveAttribute('value', '3');
    expect(radios[1]).toHaveAttribute('value', '2');
    expect(radios[2]).toHaveAttribute('value', '1');
  });

  test('renders checkboxes with some pre-checked', () => {
    const { container } = render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={['2']}
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const checkboxes = screen.getAllByLabelText(
      /Test checkbox/,
    ) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0].checked).toBe(false);
    expect(checkboxes[1].checked).toBe(true);
    expect(checkboxes[2].checked).toBe(false);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with legend', () => {
    const { container } = render(
      <FormCheckboxGroup
        legend="Choose some checkboxes"
        id="test-checkboxes"
        name="test-checkboxes"
        value={[]}
        options={[{ id: 'checkbox-1', label: 'Test checkbox 1', value: '1' }]}
      />,
    );

    expect(screen.getByText('Choose some checkboxes')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with small size variants', () => {
    const { container } = render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        small
        value={[]}
        options={[{ id: 'checkbox-1', label: 'Test checkbox 1', value: '1' }]}
      />,
    );

    expect(container.querySelector('.govuk-checkboxes--small')).not.toBeNull();
    expect(container).toMatchSnapshot();
  });

  test('renders `Select all 3 options` button when `selectAll` is true', () => {
    render(
      <FormCheckboxGroup
        value={[]}
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        selectAll
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    expect(screen.getByText('Select all 3 options')).toBeInTheDocument();
    expect(screen.queryByText('Unselect all 3 options')).toBeNull();
  });

  test('renders `Unselect all 3 options` button when all options are pre-checked', () => {
    render(
      <FormCheckboxGroup
        value={['1', '2', '3']}
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        selectAll
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    expect(screen.queryByText('Unselect all 3 options')).not.toBeNull();
    expect(screen.queryByText('Select all 3 options')).toBeNull();
  });

  test('does not render `Unselect all 3 options` button when checked values do not match options', () => {
    render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        selectAll
        value={['4', '5', '6']}
        options={[
          { id: 'checkbox-1', label: 'Test checkbox 1', value: '1' },
          { id: 'checkbox-2', label: 'Test checkbox 2', value: '2' },
          { id: 'checkbox-3', label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    expect(screen.queryByText('Unselect all 3 options')).toBeNull();
    expect(screen.queryByText('Select all 3 options')).not.toBeNull();
  });

  test('does not render `Select all 3 options` button when there is only one option', () => {
    render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        selectAll
        value={[]}
        options={[{ id: 'checkbox-1', label: 'Test checkbox 1', value: '1' }]}
      />,
    );

    expect(screen.queryByLabelText('Select all 3 options')).toBeNull();
    expect(screen.queryByLabelText('Unselect all 3 options')).toBeNull();
    expect(screen.queryByLabelText('Test checkbox 1')).not.toBeNull();
  });

  test('renders option with conditional contents', () => {
    const { container } = render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={['2']}
        options={[
          {
            id: 'checkbox-1',
            label: 'Test checkbox 1',
            value: '1',
            conditional: <p>Conditional 1</p>,
          },
          {
            id: 'checkbox-2',
            label: 'Test checkbox 2',
            value: '2',
            conditional: <p>Conditional 2</p>,
          },
          {
            id: 'checkbox-3',
            label: 'Test checkbox 3',
            value: '3',
            conditional: <p>Conditional 3</p>,
          },
        ]}
      />,
    );

    const hiddenClass = 'govuk-checkboxes__conditional--hidden';

    expect(screen.getByText('Conditional 1').parentElement).toHaveClass(
      hiddenClass,
    );
    expect(screen.getByText('Conditional 2').parentElement).not.toHaveClass(
      hiddenClass,
    );
    expect(screen.getByText('Conditional 3').parentElement).toHaveClass(
      hiddenClass,
    );
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('generates option IDs from id and value if none specified', () => {
    render(
      <FormCheckboxGroup
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={[]}
        options={[
          { label: 'Test checkbox 1', value: 'opt1' },
          { label: 'Test checkbox 2', value: 'opt-2' },
          { label: 'Test checkbox 3', value: 'opt.3' },
          { label: 'Test checkbox 4', value: 'opt 4 is \n here' },
        ]}
      />,
    );

    expect(screen.getByLabelText('Test checkbox 1')).toHaveAttribute(
      'id',
      'test-checkboxes-opt1',
    );
    expect(screen.getByLabelText('Test checkbox 2')).toHaveAttribute(
      'id',
      'test-checkboxes-opt-2',
    );
    expect(screen.getByLabelText('Test checkbox 3')).toHaveAttribute(
      'id',
      'test-checkboxes-opt.3',
    );

    expect(screen.getByLabelText('Test checkbox 4')).toHaveAttribute(
      'id',
      'test-checkboxes-opt-4-is---here',
    );
  });
});
