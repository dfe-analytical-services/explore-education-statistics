import React from 'react';
import { render } from 'react-testing-library';
import FormCheckboxSubGroups from '../FormCheckboxSubGroups';

describe('FormCheckboxSubGroups', () => {
  test('renders groups of checkboxes in correct order', () => {
    const { container, getAllByLabelText, getAllByText } = render(
      <FormCheckboxSubGroups
        id="test-checkboxes"
        name="test"
        value={[]}
        legend="Test checkboxes"
        options={[
          {
            legend: 'Group A',
            options: [
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
            ],
          },
          {
            legend: 'Group B',
            options: [
              { label: 'Checkbox 3', value: '3' },
              { label: 'Checkbox 4', value: '4' },
            ],
          },
        ]}
      />,
    );

    const groups = getAllByText(/Group/);

    expect(groups[0]).toHaveTextContent('Group A');
    expect(groups[1]).toHaveTextContent('Group B');

    const checkboxes = getAllByLabelText(/Checkbox/);

    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');
    expect(checkboxes[3]).toHaveAttribute('value', '4');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders `Unselect all options` buttons when `selectAll` is true', () => {
    const { container, getAllByText } = render(
      <FormCheckboxSubGroups
        value={[]}
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        selectAll
        options={[
          {
            legend: 'Group A',
            options: [
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
            ],
          },
          {
            legend: 'Group B',
            options: [
              { label: 'Checkbox 3', value: '3' },
              { label: 'Checkbox 4', value: '4' },
              { label: 'Checkbox 5', value: '5' },
            ],
          },
        ]}
      />,
    );

    const selectAllCheckboxes = getAllByText(
      /Select all/,
    ) as HTMLButtonElement[];

    expect(selectAllCheckboxes[0]).toHaveTextContent('Select all 2 options');
    expect(selectAllCheckboxes[1]).toHaveTextContent('Select all 3 options');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders `Unselect all options` buttons when all options are pre-checked', () => {
    const { getAllByText } = render(
      <FormCheckboxSubGroups
        value={['1', '2', '3', '4', '5']}
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        selectAll
        options={[
          {
            legend: 'Group A',
            options: [
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
            ],
          },
          {
            legend: 'Group B',
            options: [
              { label: 'Checkbox 3', value: '3' },
              { label: 'Checkbox 4', value: '4' },
              { label: 'Checkbox 5', value: '5' },
            ],
          },
        ]}
      />,
    );

    const selectAllCheckboxes = getAllByText(
      /Unselect all/,
    ) as HTMLInputElement[];

    expect(selectAllCheckboxes[0]).toHaveTextContent('Unselect all 2 options');
    expect(selectAllCheckboxes[1]).toHaveTextContent('Unselect all 3 options');
  });

  test('does not render `Select all options`  when there is only one option', () => {
    const { queryAllByLabelText } = render(
      <FormCheckboxSubGroups
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        selectAll
        value={[]}
        options={[
          {
            legend: 'Group A',
            options: [{ label: 'Checkbox 1', value: '1' }],
          },
          {
            legend: 'Group B',
            options: [{ label: 'Checkbox 3', value: '3' }],
          },
        ]}
      />,
    );

    expect(queryAllByLabelText(/Select all/)).toHaveLength(0);
  });

  test('generates group IDs from group legends if none provided', () => {
    const { container } = render(
      <FormCheckboxSubGroups
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={[]}
        options={[
          {
            legend: 'Group A',
            options: [{ label: 'Checkbox 1', value: '1' }],
          },
          {
            legend: 'Group B',
            id: 'custom-group-id',
            options: [{ label: 'Checkbox 3', value: '3' }],
          },
        ]}
      />,
    );

    expect(container.querySelector('#test-checkboxes-groupA')).not.toBeNull();
    expect(container.querySelector('#test-checkboxes-groupB')).toBeNull();
    expect(container.querySelector('#custom-group-id')).not.toBeNull();
  });
});
