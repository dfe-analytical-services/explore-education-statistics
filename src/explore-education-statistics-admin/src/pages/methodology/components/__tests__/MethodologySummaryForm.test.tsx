import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import MethodologySummaryForm from '@admin/pages/methodology/components/MethodologySummaryForm';
import React from 'react';

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
          title: '',
          titleType: 'default',
        }}
        defaultTitle="the publication title"
        submitText="Update methodology"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('Set an alternative title'));
    userEvent.click(screen.getByLabelText('Enter methodology title'));
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
          title: '',
          titleType: 'default',
        }}
        defaultTitle="the publication title"
        submitText="Update methodology"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('Set an alternative title'));
    userEvent.type(
      screen.getByLabelText('Enter methodology title'),
      'an alternative title',
    );

    userEvent.click(screen.getByRole('button', { name: 'Update methodology' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'an alternative title',
        titleType: 'alternative',
      });
    });
  });

  test('submits successfully with the publication title', async () => {
    const handleSubmit = jest.fn();
    render(
      <MethodologySummaryForm
        id="id"
        initialValues={{
          title: '',
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
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'the publication title',
        titleType: 'default',
      });
    });
  });
});
