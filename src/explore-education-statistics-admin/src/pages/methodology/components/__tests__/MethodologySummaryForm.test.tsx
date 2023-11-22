import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';
import MethodologySummaryForm from '@admin/pages/methodology/components/MethodologySummaryForm';

describe('MethodologySummaryForm', () => {
  test('renders the form with initial values', () => {
    render(
      <MethodologySummaryForm
        id="id"
        initialValues={{
          title: 'the publication title',
          titleType: 'default',
        }}
        defaultTitle="the publication title"
        submitText="Update methodology"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByText('Methodology title')).toBeInTheDocument();

    expect(screen.getByLabelText('Use publication title')).toBeInTheDocument();

    expect(
      screen.getByLabelText('Set an alternative title'),
    ).toBeInTheDocument();
  });

  test('shows validation error when select alternative title type and no title given', async () => {
    render(
      <MethodologySummaryForm
        id="id"
        initialValues={{
          title: 'the publication title',
          titleType: 'default',
        }}
        defaultTitle="the publication title"
        submitText="Update methodology"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('Set an alternative title'));
    userEvent.clear(screen.getByLabelText('Enter methodology title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a methodology title', {
          selector: '#id-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('submits successfully with an alternative title', async () => {
    const handleSubmit = jest.fn();
    render(
      <MethodologySummaryForm
        id="id"
        initialValues={{
          title: 'the publication title',
          titleType: 'default',
        }}
        defaultTitle="the publication title"
        submitText="Update methodology"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('Set an alternative title'));
    userEvent.clear(screen.getByLabelText('Enter methodology title'));
    await userEvent.type(
      screen.getByLabelText('Enter methodology title'),
      'an alternative title',
    );

    userEvent.click(screen.getByRole('button', { name: 'Update methodology' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith('an alternative title');
    });
  });

  test('submits successfully with the publication title', async () => {
    const handleSubmit = jest.fn();
    render(
      <MethodologySummaryForm
        id="id"
        initialValues={{
          title: 'the publication title',
          titleType: 'default',
        }}
        defaultTitle="the publication title"
        submitText="Update methodology"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Update methodology' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith('the publication title');
    });
  });

  test('submits successfully when change back to publication title from an alternative title', async () => {
    const handleSubmit = jest.fn();
    render(
      <MethodologySummaryForm
        id="id"
        initialValues={{
          title: 'the alternative title',
          titleType: 'alternative',
        }}
        defaultTitle="the publication title"
        submitText="Update methodology"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('Use publication title'));

    userEvent.click(screen.getByRole('button', { name: 'Update methodology' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith('the publication title');
    });
  });
});
