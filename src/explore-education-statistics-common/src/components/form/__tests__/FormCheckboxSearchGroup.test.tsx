import React from 'react';
import { fireEvent, render } from 'react-testing-library';
import FormCheckboxSearchGroup from '../FormCheckboxSearchGroup';

jest.mock('lodash/debounce');

describe('FormCheckboxSearchGroup', () => {
  test('renders list of checkboxes in correct order', () => {
    const { container, getAllByLabelText } = render(
      <FormCheckboxSearchGroup
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        value={[]}
        options={[
          { label: 'Test checkbox 1', value: '1' },
          { label: 'Test checkbox 2', value: '2' },
          { label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const checkboxes = getAllByLabelText(/Test checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(3);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('providing a search term renders only relevant checkboxes', () => {
    jest.useFakeTimers();

    const { getByLabelText, getAllByLabelText } = render(
      <FormCheckboxSearchGroup
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={[]}
        options={[
          { label: 'Test checkbox 1', value: '1' },
          { label: 'Test checkbox 2', value: '2' },
          { label: 'Test checkbox 3', value: '3' },
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

    const checkboxes = getAllByLabelText(/Test checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(1);
    expect(checkboxes[0]).toHaveAttribute('value', '2');
  });

  test('does not throw error if search term that is invalid regex is used', () => {
    jest.useFakeTimers();

    const { getByLabelText, queryAllByLabelText } = render(
      <FormCheckboxSearchGroup
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={[]}
        options={[
          { label: 'Test checkbox 1', value: '1' },
          { label: 'Test checkbox 2', value: '2' },
          { label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    const searchInput = getByLabelText('Search options');

    fireEvent.change(searchInput, {
      target: {
        value: '[',
      },
    });

    expect(() => {
      jest.runAllTimers();
    }).not.toThrow();

    const checkboxes = queryAllByLabelText(
      /Test checkbox/,
    ) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(0);
  });

  test('providing a search term does not remove checkboxes that have already been checked', () => {
    jest.useFakeTimers();

    const { getByLabelText, getAllByLabelText } = render(
      <FormCheckboxSearchGroup
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={['1']}
        options={[
          { label: 'Test checkbox 1', value: '1' },
          { label: 'Test checkbox 2', value: '2' },
          { label: 'Test checkbox 3', value: '3' },
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

    const checkboxes = getAllByLabelText(/Test checkbox/) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(2);
    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
  });

  test('selecting options increments the selection count', async () => {
    const { queryByText } = render(
      <FormCheckboxSearchGroup
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        value={['1']}
        options={[
          { label: 'Test checkbox 1', value: '1' },
          { label: 'Test checkbox 2', value: '2' },
          { label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    expect(queryByText('1 selected')).not.toBeNull();
  });

  test('setting `hideCount` prop to true hides the selection count', () => {
    const { queryByText } = render(
      <FormCheckboxSearchGroup
        name="testCheckboxes"
        id="test-checkboxes"
        legend="Choose options"
        searchLabel="Search options"
        hideCount
        value={['1']}
        options={[
          { label: 'Test checkbox 1', value: '1' },
          { label: 'Test checkbox 2', value: '2' },
          { label: 'Test checkbox 3', value: '3' },
        ]}
      />,
    );

    expect(queryByText('1 selected')).toBeNull();
  });
});
