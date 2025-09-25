import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import EditableFreeTextStatTileForm from '../components/EditableFreeTextStatTileForm';
import testTile from './__data__/testTile';

describe('EditableFreeTextStatTileForm', () => {
  test('renders correctly without initial values', async () => {
    render(
      <EditableFreeTextStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).not.toHaveValue();
    expect(screen.getByLabelText('Statistic')).not.toHaveValue();
    expect(screen.getByLabelText('Trend')).not.toHaveValue();
    expect(screen.getByLabelText('Link URL')).not.toHaveValue();
    expect(screen.getByLabelText('Link text')).not.toHaveValue();

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly with initial values', async () => {
    render(
      <EditableFreeTextStatTileForm
        statTile={testTile}
        testId="test-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).toHaveValue('Tile 1 title');
    expect(screen.getByLabelText('Statistic')).toHaveValue('1000');
    expect(screen.getByLabelText('Trend')).toHaveValue('Tile 1 trend');
    expect(screen.getByLabelText('Link text')).toHaveValue('Tile 1 link text');
    expect(screen.getByLabelText('Link URL')).toHaveValue(
      'https://example.com/tile-1',
    );

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('submitting form calls onSubmit handler with updated values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableFreeTextStatTileForm
        statTile={testTile}
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.clear(screen.getByLabelText('Title'));
    await user.type(screen.getByLabelText('Title'), 'New title');

    await user.clear(screen.getByLabelText('Statistic'));
    await user.type(screen.getByLabelText('Statistic'), 'New stat');

    await user.clear(screen.getByLabelText('Trend'));
    await user.type(screen.getByLabelText('Trend'), 'New trend');

    await user.clear(screen.getByLabelText('Link text'));
    await user.type(screen.getByLabelText('Link text'), 'New link text');

    await user.clear(screen.getByLabelText('Link URL'));
    await user.type(screen.getByLabelText('Link URL'), 'http://example.com');

    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'New title',
        statistic: 'New stat',
        trend: 'New trend',
        linkText: 'New link text',
        linkUrl: 'http://example.com',
      });
    });
  });

  test('shows a validation error if submit without a title', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableFreeTextStatTileForm
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
    ).toHaveAttribute('href', '#editableFreeTextStatTileForm-create-title');
    expect(
      screen.getByTestId('editableFreeTextStatTileForm-create-title-error'),
    ).toHaveTextContent('Enter a title');
  });

  test('shows a validation error if submit without a statistic', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableFreeTextStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter a statistic',
      }),
    ).toHaveAttribute('href', '#editableFreeTextStatTileForm-create-statistic');
    expect(
      screen.getByTestId('editableFreeTextStatTileForm-create-statistic-error'),
    ).toHaveTextContent('Enter a statistic');
  });

  test('shows a validation error if submit without a trend', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableFreeTextStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter a trend',
      }),
    ).toHaveAttribute('href', '#editableFreeTextStatTileForm-create-trend');
    expect(
      screen.getByTestId('editableFreeTextStatTileForm-create-trend-error'),
    ).toHaveTextContent('Enter a trend');
  });

  test('shows a validation error when there is link url without link text', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <EditableFreeTextStatTileForm
        testId="test-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.type(screen.getByLabelText('Statistic'), 'Test stat');
    await user.type(screen.getByLabelText('Trend'), 'Test trend');
    await user.type(
      screen.getByLabelText('Link URL'),
      'https://example.com/tile-1',
    );
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      within(screen.getByTestId('errorSummary')).getByRole('link', {
        name: 'Enter the link text',
      }),
    ).toHaveAttribute('href', '#editableFreeTextStatTileForm-create-linkText');
    expect(
      screen.getByTestId('editableFreeTextStatTileForm-create-linkText-error'),
    ).toHaveTextContent('Enter the link text');
  });
});
