import RelatedPageForm from '@admin/pages/release/content/components/RelatedPageForm';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import render from '@common-test/render';
import noop from 'lodash/noop';

describe('RelatedPageForm', () => {
  test('renders without initial values', async () => {
    render(
      <RelatedPageForm onCancel={noop} onSubmit={() => Promise.resolve()} />,
    );

    expect(screen.getByLabelText('Title')).toBeInTheDocument();
    expect(screen.getByLabelText('Title')).toHaveValue('');

    expect(screen.getByLabelText('Link URL')).toBeInTheDocument();
    expect(screen.getByLabelText('Link URL')).toHaveValue('');

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders with initial values', async () => {
    render(
      <RelatedPageForm
        initialValues={{
          description: 'Page 1',
          url: 'https://gov.uk',
        }}
        onCancel={noop}
        onSubmit={() => Promise.resolve()}
      />,
    );

    expect(screen.getByLabelText('Title')).toBeInTheDocument();
    expect(screen.getByLabelText('Title')).toHaveValue('Page 1');

    expect(screen.getByLabelText('Link URL')).toBeInTheDocument();
    expect(screen.getByLabelText('Link URL')).toHaveValue('https://gov.uk');

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('shows a validation error when no title is set', async () => {
    const { user } = render(
      <RelatedPageForm onCancel={noop} onSubmit={() => Promise.resolve()} />,
    );

    await user.click(screen.getByLabelText('Title'));
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter a link title',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows a validation error when no url is set', async () => {
    const { user } = render(
      <RelatedPageForm onCancel={noop} onSubmit={() => Promise.resolve()} />,
    );

    await user.click(screen.getByLabelText('Link URL'));
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter a link URL',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows a validation error when the url is invalid', async () => {
    const { user } = render(
      <RelatedPageForm onCancel={noop} onSubmit={() => Promise.resolve()} />,
    );

    await user.type(screen.getByLabelText('Link URL'), 'Not a url');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter a valid link URL',
        }),
      ).toBeInTheDocument();
    });
  });

  test('calls onSubmit when the form is successfully submitted', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <RelatedPageForm onCancel={noop} onSubmit={handleSubmit} />,
    );

    await user.type(screen.getByLabelText('Title'), 'Test title');
    await user.type(screen.getByLabelText('Link URL'), 'https://gov.uk');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(handleSubmit).toHaveBeenCalledTimes(1);
    expect(handleSubmit).toHaveBeenCalledWith({
      description: 'Test title',
      url: 'https://gov.uk',
    });
  });
});
