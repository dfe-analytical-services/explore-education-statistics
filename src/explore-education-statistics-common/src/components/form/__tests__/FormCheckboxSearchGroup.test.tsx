import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import FormCheckboxSearchGroup from '../FormCheckboxSearchGroup';

jest.mock('lodash/debounce');

describe('FormCheckboxSearchGroup', () => {
  afterEach(() => {
    jest.useRealTimers();
  });

  describe('without searchOnly', () => {
    test('renders list of checkboxes in correct order', () => {
      const { container } = render(
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

      const checkboxes = screen.getAllByLabelText(
        /Test checkbox/,
      ) as HTMLInputElement[];

      expect(checkboxes).toHaveLength(3);
      expect(checkboxes[0]).toHaveAttribute('value', '1');
      expect(checkboxes[1]).toHaveAttribute('value', '2');
      expect(checkboxes[2]).toHaveAttribute('value', '3');

      expect(container.innerHTML).toMatchSnapshot();
    });

    test('providing a search term renders only relevant checkboxes', async () => {
      jest.useFakeTimers();

      render(
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

      const searchInput = screen.getByLabelText('Search options');

      await userEvent.type(searchInput, '2');

      jest.runAllTimers();

      const checkboxes = screen.getAllByLabelText(
        /Test checkbox/,
      ) as HTMLInputElement[];

      expect(checkboxes).toHaveLength(1);
      expect(checkboxes[0]).toHaveAttribute('value', '2');
    });

    test('providing a search term does not remove checkboxes that have already been checked', async () => {
      jest.useFakeTimers();

      render(
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

      const searchInput = screen.getByLabelText('Search options');

      await userEvent.type(searchInput, '2');

      jest.runAllTimers();

      const checkboxes = screen.getAllByLabelText(
        /Test checkbox/,
      ) as HTMLInputElement[];

      expect(checkboxes).toHaveLength(2);
      expect(checkboxes[0]).toHaveAttribute('value', '1');
      expect(checkboxes[1]).toHaveAttribute('value', '2');
    });

    test('renders the checkboxes when the number of options is above the maxSearchResults', () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          maxSearchResults={2}
          value={[]}
          options={[
            { label: 'Test checkbox 1', value: '1' },
            { label: 'Test checkbox 2', value: '2' },
            { label: 'Test checkbox 3', value: '3' },
          ]}
        />,
      );

      const checkboxes = screen.getAllByLabelText(
        /Test checkbox/,
      ) as HTMLInputElement[];

      expect(checkboxes).toHaveLength(3);
      expect(checkboxes[0]).toHaveAttribute('value', '1');
      expect(checkboxes[1]).toHaveAttribute('value', '2');
      expect(checkboxes[2]).toHaveAttribute('value', '3');
    });

    test('renders the no options message when there are no options', () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          value={[]}
          options={[]}
        />,
      );

      expect(screen.getByText('No options available.')).toBeInTheDocument();
    });
  });

  describe('with searchOnly', () => {
    const testSearchOnlyOptions = [
      {
        label: 'Test checkbox 1',
        value: '1',
        hint: 'URN: 000001; Local authority: LA 1',
      },
      {
        label: 'Test checkbox 2',
        value: '2',
        hint: 'URN: 000002; Local authority: LA 1',
      },
      {
        label: 'Test checkbox 3',
        value: '3',
        hint: 'URN: 000003; Local authority: LA 1',
      },
      {
        label: 'Test 4',
        value: '4',
        hint: 'URN: 000004; Local authority: LA 1',
      },
    ];
    test('does not render the checkboxes by default', () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          searchLabel="Search options"
          searchOnly
          value={[]}
          options={testSearchOnlyOptions}
        />,
      );

      expect(screen.queryAllByRole('checkbox')).toHaveLength(0);
    });

    test('renders checkboxes for selected options', () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          searchLabel="Search options"
          searchOnly
          value={['2']}
          options={testSearchOnlyOptions}
        />,
      );

      const checkboxes = screen.getAllByRole('checkbox');
      expect(checkboxes).toHaveLength(1);
      expect(checkboxes[0]).toHaveAttribute('value', '2');
      expect(checkboxes[0]).toEqual(screen.getByLabelText('Test checkbox 2'));
      expect(checkboxes[0]).toBeChecked();
    });

    test('renders checkboxes that match the search term', async () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          searchLabel="Search options"
          searchOnly
          value={[]}
          options={testSearchOnlyOptions}
        />,
      );

      const searchInput = screen.getByLabelText('Search options');
      userEvent.type(searchInput, 'checkbox');

      await waitFor(() => {
        expect(screen.getByText('3 options found')).toBeInTheDocument();
      });

      const checkboxes = screen.getAllByRole('checkbox');
      expect(checkboxes).toHaveLength(3);
      expect(checkboxes[0]).toHaveAttribute('value', '1');
      expect(checkboxes[0]).toEqual(screen.getByLabelText('Test checkbox 1'));
      expect(checkboxes[0]).not.toBeChecked();

      expect(checkboxes[1]).toHaveAttribute('value', '2');
      expect(checkboxes[1]).toEqual(screen.getByLabelText('Test checkbox 2'));
      expect(checkboxes[1]).not.toBeChecked();

      expect(checkboxes[2]).toHaveAttribute('value', '3');
      expect(checkboxes[2]).toEqual(screen.getByLabelText('Test checkbox 3'));
      expect(checkboxes[1]).not.toBeChecked();
    });

    test('renders checkboxes where the URN in the hint matches the search term', async () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          searchLabel="Search options"
          searchOnly
          value={[]}
          options={testSearchOnlyOptions}
        />,
      );

      const searchInput = screen.getByLabelText('Search options');
      userEvent.type(searchInput, '000002');

      await waitFor(() => {
        expect(screen.getByText('1 option found')).toBeInTheDocument();
      });

      const checkboxes = screen.getAllByRole('checkbox');

      expect(checkboxes[0]).toHaveAttribute('value', '2');
      expect(checkboxes[0]).toEqual(screen.getByLabelText('Test checkbox 2'));
      expect(checkboxes[0]).not.toBeChecked();
    });

    test('does not search when the search term has fewer than 3 characters', async () => {
      jest.useFakeTimers();
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          searchLabel="Search options"
          searchOnly
          value={[]}
          options={testSearchOnlyOptions}
        />,
      );

      const searchInput = screen.getByLabelText('Search options');
      await userEvent.type(searchInput, 'ch');

      jest.runAllTimers();

      expect(screen.queryAllByRole('checkbox')).toHaveLength(0);
    });

    test('renders the too many results message and no checkboxes when there are more than the maximum results', async () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          maxSearchResults={2}
          searchHelpText="This is the help text"
          searchLabel="Search options"
          searchOnly
          value={[]}
          options={testSearchOnlyOptions}
        />,
      );

      const searchInput = screen.getByLabelText('Search options');
      userEvent.type(searchInput, 'checkbox');

      await waitFor(() => {
        expect(
          screen.getByText(
            '3 results found. Please refine your search to view options.',
          ),
        ).toBeInTheDocument();
      });

      expect(screen.queryAllByRole('checkbox')).toHaveLength(0);
    });

    test('renders the search help when text there are no options', () => {
      render(
        <FormCheckboxSearchGroup
          name="testCheckboxes"
          id="test-checkboxes"
          legend="Choose options"
          maxSearchResults={2}
          searchHelpText="This is the help text"
          searchLabel="Search options"
          searchOnly
          value={[]}
          options={[]}
        />,
      );

      expect(screen.getByText('This is the help text')).toBeInTheDocument();
    });
  });

  test('does not throw error if search term that is invalid regex is used', async () => {
    jest.useFakeTimers();

    render(
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

    const searchInput = screen.getByLabelText('Search options');

    await userEvent.type(searchInput, '[');

    expect(() => {
      jest.runAllTimers();
    }).not.toThrow();

    const checkboxes = screen.queryAllByLabelText(
      /Test checkbox/,
    ) as HTMLInputElement[];

    expect(checkboxes).toHaveLength(0);
  });
});
