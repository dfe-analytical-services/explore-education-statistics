import React from 'react';
import { fireEvent, render, screen, within } from '@testing-library/react';
import FormCheckboxSearchSubGroups from '../FormCheckboxSearchSubGroups';

describe('FormCheckboxSearchSubGroups', () => {
  test('renders groups of checkboxes in correct order', () => {
    const { container } = render(
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

    const groups = screen.getAllByText(/Group/);

    expect(groups[0]).toHaveTextContent('Group A');
    expect(groups[1]).toHaveTextContent('Group B');

    const checkboxes = screen.getAllByLabelText(
      /Checkbox/,
    ) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(4);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');
    expect(checkboxes[3]).toHaveAttribute('value', '4');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('generates group IDs if none provided', () => {
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

    expect(container.querySelector('#test-checkboxes-1')).not.toBeNull();
    expect(container.querySelector('#test-checkboxes-2')).toBeNull();
    expect(container.querySelector('#custom-group-id')).not.toBeNull();
  });

  test('renders `Select all options` button correctly', () => {
    render(
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
            options: [
              { label: 'Checkbox 2', value: '2' },
              { label: 'Checkbox 3', value: '3' },
            ],
          },
        ]}
      />,
    );

    const fieldset = within(
      screen.getByRole('group', { name: 'Test checkboxes' }),
    );

    expect(
      fieldset.getByRole('button', {
        name: 'Select all 3 options',
      }),
    ).toBeInTheDocument();
  });

  test('does not render any `Select all options` button if only a single option', () => {
    render(
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
        ]}
      />,
    );

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  test('renders `Select all subgroup options` buttons for subgroups correctly', () => {
    render(
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
            options: [
              { label: 'Checkbox 2', value: '2' },
              { label: 'Checkbox 3', value: '3' },
            ],
          },
        ]}
      />,
    );

    const group1 = within(screen.getByRole('group', { name: 'Group A' }));
    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    // If only single option, don't render the button
    expect(group1.queryByRole('button')).not.toBeInTheDocument();

    expect(
      group2.getByRole('button', {
        name: 'Select all 2 subgroup options',
      }),
    ).toBeInTheDocument();
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

  test('does not throw error if search term that is invalid regex is used', () => {
    jest.useFakeTimers();

    const { getByLabelText, queryAllByLabelText } = render(
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
        value: '[',
      },
    });

    jest.runAllTimers();

    const checkboxes = queryAllByLabelText(/Checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(0);
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
});
