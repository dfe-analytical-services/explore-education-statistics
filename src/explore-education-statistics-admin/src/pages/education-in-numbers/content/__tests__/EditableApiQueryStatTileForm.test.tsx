import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import EditableApiQueryStatTileForm from '../components/EditableApiQueryStatTileForm';
import testApiQueryTile from './__data__/testApiQueryTile';

describe('EditableApiQueryStatTileForm', () => {
  test('renders correctly without initial values', async () => {
    render(
      <EditableApiQueryStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).not.toHaveValue();
    expect(screen.getByLabelText('DataSetId')).not.toHaveValue();
    expect(screen.getByLabelText('Version')).not.toHaveValue();
    expect(screen.getByLabelText('Query')).not.toHaveValue();

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly with initial values', async () => {
    render(
      <EditableApiQueryStatTileForm
        apiQueryStatTile={testApiQueryTile}
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).toHaveValue('Tile 2 title');
    expect(screen.getByLabelText('DataSetId')).toHaveValue(
      'b8e0cbc4-e1f8-4b32-9d0f-8d0c5d3f0a11',
    );
    expect(screen.getByLabelText('Version')).toHaveValue('1.0.1');
    expect(screen.getByLabelText('Query')).toHaveValue(
      '{ "indicators": ["tile-2-indicator"] }',
    );

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('does not render the server-derived fields', async () => {
    render(
      <EditableApiQueryStatTileForm
        apiQueryStatTile={testApiQueryTile}
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText('Statistic')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Indicator unit')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Decimal places')).not.toBeInTheDocument();
  });

  test('submitting form calls onSubmit handler with updated values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableApiQueryStatTileForm
        apiQueryStatTile={testApiQueryTile}
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.clear(screen.getByLabelText('Title'));
    await user.type(screen.getByLabelText('Title'), 'New title');

    await user.clear(screen.getByLabelText('DataSetId'));
    await user.type(
      screen.getByLabelText('DataSetId'),
      'ff5b1c17-6a1f-4a4e-9a3f-1c2b3d4e5f60',
    );

    await user.clear(screen.getByLabelText('Version'));
    await user.type(screen.getByLabelText('Version'), '2.0.0');

    await user.clear(screen.getByLabelText('Query'));
    // Pasted rather than typed as user-event parses `{` and `[` as key
    // descriptors, and a JSON body would realistically be pasted in anyway.
    await user.click(screen.getByLabelText('Query'));
    await user.paste('{ "indicators": ["new-indicator"] }');

    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'New title',
        dataSetId: 'ff5b1c17-6a1f-4a4e-9a3f-1c2b3d4e5f60',
        version: '2.0.0',
        query: '{ "indicators": ["new-indicator"] }',
      });
    });
  });

  test('clicking Cancel calls onCancel handler', async () => {
    const handleCancel = jest.fn();

    const { user } = render(
      <EditableApiQueryStatTileForm
        apiQueryStatTile={testApiQueryTile}
        testId="test-id"
        onCancel={handleCancel}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(handleCancel).toHaveBeenCalled();
  });

  test('shows a validation error if submit without a title', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableApiQueryStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter a title',
      }),
    ).toHaveAttribute('href', '#editableApiQueryStatTileForm-create-title');
    expect(
      screen.getByTestId('editableApiQueryStatTileForm-create-title-error'),
    ).toHaveTextContent('Enter a title');

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('shows a validation error if submit without a dataSetId', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableApiQueryStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter the dataSetId',
      }),
    ).toHaveAttribute('href', '#editableApiQueryStatTileForm-create-dataSetId');
    expect(
      screen.getByTestId('editableApiQueryStatTileForm-create-dataSetId-error'),
    ).toHaveTextContent('Enter the dataSetId');

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('shows a validation error if the dataSetId is not a 36 character id', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableApiQueryStatTileForm
        apiQueryStatTile={testApiQueryTile}
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.clear(screen.getByLabelText('DataSetId'));
    await user.type(screen.getByLabelText('DataSetId'), 'not-a-guid');

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      screen.getByTestId(
        `editableApiQueryStatTileForm-${testApiQueryTile.id}-dataSetId-error`,
      ),
    ).toHaveTextContent('exactly 36 characters');

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('shows a validation error if submit without a version', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableApiQueryStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter data set version',
      }),
    ).toHaveAttribute('href', '#editableApiQueryStatTileForm-create-version');
    expect(
      screen.getByTestId('editableApiQueryStatTileForm-create-version-error'),
    ).toHaveTextContent('Enter data set version');

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('shows a validation error if submit without a query', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableApiQueryStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter the API query JSON body',
      }),
    ).toHaveAttribute('href', '#editableApiQueryStatTileForm-create-query');
    expect(
      screen.getByTestId('editableApiQueryStatTileForm-create-query-error'),
    ).toHaveTextContent('Enter the API query JSON body');

    expect(handleSubmit).not.toHaveBeenCalled();
  });
});
