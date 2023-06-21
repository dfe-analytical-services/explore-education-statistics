import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
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

    const group1 = within(screen.getByRole('group', { name: 'Group A' }));
    const group2 = within(screen.getByRole('group', { name: 'Group B' }));

    const group1Checkboxes = group1.getAllByRole('checkbox');
    const group2Checkboxes = group2.getAllByRole('checkbox');

    expect(group1Checkboxes).toHaveLength(2);
    expect(group1Checkboxes[0]).toHaveAttribute('value', '1');
    expect(group1Checkboxes[1]).toHaveAttribute('value', '2');

    expect(group2Checkboxes).toHaveLength(2);
    expect(group2Checkboxes[0]).toHaveAttribute('value', '3');
    expect(group2Checkboxes[1]).toHaveAttribute('value', '4');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders single group of checkboxes correctly', () => {
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
        ]}
      />,
    );

    expect(
      screen.queryByRole('group', { name: 'Group A' }),
    ).not.toBeInTheDocument();

    const fieldset = within(
      screen.getByRole('group', { name: 'Choose options' }),
    );

    const checkboxes = fieldset.getAllByRole('checkbox');

    expect(checkboxes).toHaveLength(2);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('generates group IDs if none provided', () => {
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
            options: [{ label: 'Checkbox 3', value: '3' }],
          },
        ]}
      />,
    );

    expect(screen.getByRole('group', { name: 'Group A' })).toHaveAttribute(
      'id',
      'test-checkboxes-options-1',
    );

    expect(screen.getByRole('group', { name: 'Group B' })).toHaveAttribute(
      'id',
      'test-checkboxes-custom-group-id',
    );
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
        name: /Select all 2 subgroup options/i,
      }),
    ).toBeInTheDocument();
  });

  test('does not render `Select all subgroup options` button if there is only one subgroup', () => {
    render(
      <FormCheckboxSearchSubGroups
        id="test-checkboxes"
        name="test-checkboxes"
        legend="Test checkboxes"
        value={[]}
        options={[
          {
            legend: 'Group A',
            options: [
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
            ],
          },
        ]}
      />,
    );

    const fieldset = within(
      screen.getByRole('group', { name: 'Test checkboxes' }),
    );

    expect(
      fieldset.queryByRole('button', {
        name: 'Select all 2 subgroup options',
      }),
    ).not.toBeInTheDocument();

    expect(
      fieldset.getByRole('button', {
        name: 'Select all 2 options',
      }),
    ).toBeInTheDocument();
  });

  test('providing a search term renders only relevant checkboxes', async () => {
    jest.useFakeTimers();

    render(
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

    userEvent.type(screen.getByLabelText('Search options'), '2');

    jest.runAllTimers();

    const checkboxes = screen.getAllByLabelText(
      /Checkbox/,
    ) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(1);
    expect(checkboxes[0]).toHaveAttribute('value', '2');
  });

  test('does not throw error if search term that is invalid regex is used', async () => {
    jest.useFakeTimers();

    render(
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

    userEvent.type(screen.getByLabelText('Search options'), '[');

    jest.runAllTimers();

    const checkboxes = screen.queryAllByLabelText(
      /Checkbox/,
    ) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(0);
  });

  test('providing a search term does not remove checkboxes that have already been checked', async () => {
    jest.useFakeTimers();

    render(
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

    const searchInput = screen.getByLabelText('Search options');

    userEvent.type(searchInput, '2');

    jest.runAllTimers();

    const checkboxes = screen.getAllByLabelText(
      /Checkbox/,
    ) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(2);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
  });

  test('renders search input if more than one option in a single group', () => {
    render(
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
        ]}
      />,
    );

    expect(screen.getByLabelText('Search options')).toBeInTheDocument();
  });

  test('does not render search input if only a single option', () => {
    render(
      <FormCheckboxSearchSubGroups
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={[]}
        options={[
          {
            legend: 'Group A',
            options: [{ label: 'Checkbox 1', value: '1' }],
          },
        ]}
      />,
    );

    expect(screen.queryByLabelText('Search options')).not.toBeInTheDocument();
  });
});
