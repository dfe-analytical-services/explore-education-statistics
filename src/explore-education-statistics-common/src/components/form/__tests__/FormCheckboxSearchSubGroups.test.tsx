import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import FormCheckboxSearchSubGroups from '../FormCheckboxSearchSubGroups';

describe('FormCheckboxSearchSubGroups', () => {
  test('renders groups of checkboxes in correct order', () => {
    const { container, getAllByText, getAllByLabelText } = render(
      <FormCheckboxSearchSubGroups
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        value={[]}
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

    const checkboxes = getAllByLabelText(/Checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(4);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');
    expect(checkboxes[3]).toHaveAttribute('value', '4');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('generates group IDs from group legends if none provided', () => {
    const { container } = render(
      <FormCheckboxSearchSubGroups
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

  test('providing a search term renders only relevant checkboxes', () => {
    jest.useFakeTimers();

    const { getByLabelText, getAllByLabelText } = render(
      <FormCheckboxSearchSubGroups
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={[]}
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

    const searchInput = getByLabelText('Search options');

    fireEvent.change(searchInput, {
      target: {
        value: '2',
      },
    });

    jest.runAllTimers();

    const checkboxes = getAllByLabelText(/Checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(1);
    expect(checkboxes[0]).toHaveAttribute('value', '2');
  });

  test('providing a search term does not remove checkboxes that have already been checked', () => {
    jest.useFakeTimers();

    const { getByLabelText, getAllByLabelText } = render(
      <FormCheckboxSearchSubGroups
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={['1']}
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

    const searchInput = getByLabelText('Search options');

    fireEvent.change(searchInput, {
      target: {
        value: '2',
      },
    });

    jest.runAllTimers();

    const checkboxes = getAllByLabelText(/Checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(2);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
  });

  test('selecting options increments the selection count', async () => {
    const { queryByText } = render(
      <FormCheckboxSearchSubGroups
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={['1']}
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

    expect(queryByText('1 selected')).not.toBeNull();
  });

  test('setting `hideCount` prop to true hides the selection count', () => {
    const { queryByText } = render(
      <FormCheckboxSearchSubGroups
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        hideCount
        value={['1']}
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

    expect(queryByText('1 selected')).toBeNull();
  });
});
