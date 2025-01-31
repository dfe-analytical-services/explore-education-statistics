import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import FormRadioSearchGroup from '@common/components/form//FormRadioSearchGroup';
import render from '@common-test/render';

jest.mock('lodash/debounce');

describe('FormRadioSearchGroup', () => {
  test('renders list of radio buttons in correct order', () => {
    const { container } = render(
      <FormRadioSearchGroup
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const radios = screen.getAllByRole('radio');
    expect(radios).toHaveLength(3);
    expect(radios[0]).toHaveAttribute('value', '1');
    expect(radios[1]).toHaveAttribute('value', '2');
    expect(radios[2]).toHaveAttribute('value', '3');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('providing a search term renders only relevant radio buttons', async () => {
    const { user } = render(
      <FormRadioSearchGroup
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        searchLabel="Search options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const searchInput = screen.getByLabelText('Search options');

    await user.type(searchInput, '2');

    await waitFor(() => {
      expect(screen.queryByLabelText('Test radio 3')).not.toBeInTheDocument();
    });

    const radios = screen.getAllByRole('radio');
    expect(radios).toHaveLength(1);
    expect(radios[0]).toHaveAttribute('value', '2');
  });

  test('does not throw error if search term that is invalid regex is used', async () => {
    const { user } = render(
      <FormRadioSearchGroup
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        searchLabel="Search options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const searchInput = screen.getByLabelText('Search options');

    await user.type(searchInput, '[[');

    await waitFor(() => {
      expect(screen.queryAllByLabelText(/Test radio/)).toHaveLength(0);
    });
  });

  test('providing a search term does not remove radio buttons that have already been checked', async () => {
    const { user } = render(
      <FormRadioSearchGroup
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        searchLabel="Search options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const radio = screen.getByLabelText('Test radio 1');

    await user.click(radio);

    const searchInput = screen.getByLabelText('Search options');

    await user.type(searchInput, '2');

    await waitFor(() => {
      expect(screen.queryByLabelText('Test radio 3')).not.toBeInTheDocument();
    });

    const radios = screen.getAllByRole('radio');
    expect(radios).toHaveLength(2);
    expect(radios[0]).toHaveAttribute('value', '1');
    expect(radios[1]).toHaveAttribute('value', '2');
  });

  test('shows a message if no search results are found', async () => {
    const { user } = render(
      <FormRadioSearchGroup
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        searchLabel="Search options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const searchInput = screen.getByLabelText('Search options');

    await user.type(searchInput, 'Not there');

    expect(await screen.findByText('No results found')).toBeInTheDocument();

    const radios = screen.queryAllByRole('radio');
    expect(radios).toHaveLength(0);
  });

  test('`alwaysShow` values are shown when they do no match the search term', async () => {
    const { user } = render(
      <FormRadioSearchGroup
        alwaysShowOptions={['2']}
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        searchLabel="Search options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    await user.type(screen.getByLabelText('Search options'), '3');

    await waitFor(() => {
      expect(screen.queryByLabelText('Test radio 1')).not.toBeInTheDocument();
    });
    const radios = screen.getAllByRole('radio');
    expect(radios).toHaveLength(2);
    expect(radios[0]).toHaveAttribute('value', '2');
    expect(radios[1]).toHaveAttribute('value', '3');
  });

  test('shows a message when there are no results other than the `alwaysShow` values', async () => {
    const { user } = render(
      <FormRadioSearchGroup
        alwaysShowOptions={['2']}
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        searchLabel="Search options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    await user.type(screen.getByLabelText('Search options'), 'Not there');
    expect(await screen.findByText('No results found')).toBeInTheDocument();

    expect(screen.getByLabelText('Test radio 2')).toBeInTheDocument();
    expect(screen.getAllByRole('radio')).toHaveLength(1);
  });

  test('`alwaysShow` values are shown when there is not a search term', async () => {
    render(
      <FormRadioSearchGroup
        alwaysShowOptions={['2']}
        name="testRadios"
        id="test-radios"
        legend="Choose options"
        searchLabel="Search options"
        options={[
          { label: 'Test radio 1', value: '1' },
          { label: 'Test radio 2', value: '2' },
          { label: 'Test radio 3', value: '3' },
        ]}
      />,
    );

    const radios = screen.getAllByRole('radio');
    expect(radios).toHaveLength(3);
    expect(radios[0]).toHaveAttribute('value', '1');
    expect(radios[1]).toHaveAttribute('value', '2');
    expect(radios[2]).toHaveAttribute('value', '3');
  });
});
