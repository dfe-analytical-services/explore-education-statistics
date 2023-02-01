import EditableEmbedForm from '@admin/components/editable/EditableEmbedForm';
import {
  render as baseRender,
  RenderResult,
  screen,
  waitFor,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactNode } from 'react';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';

describe('EditableEmbedForm', () => {
  test('renders the form', () => {
    render(<EditableEmbedForm onCancel={noop} onSubmit={noop} />);

    expect(screen.getByLabelText('Title')).toBeInTheDocument();
    expect(screen.getByLabelText('URL')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders the form with initial values', () => {
    render(
      <EditableEmbedForm
        initialValues={{
          title: 'Dashboard title',
          url: 'https://department-for-education.shinyapps.io/test-dashboard',
        }}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Title')).toHaveValue('Dashboard title');
    expect(screen.getByLabelText('URL')).toHaveValue(
      'https://department-for-education.shinyapps.io/test-dashboard',
    );
    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('shows a validation error when no title is set', async () => {
    render(<EditableEmbedForm onCancel={noop} onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('Title'));
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter a title',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows a validation error when no url is set', async () => {
    render(<EditableEmbedForm onCancel={noop} onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('URL'));
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter a URL',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows a validation error when the url is invalid', async () => {
    render(<EditableEmbedForm onCancel={noop} onSubmit={noop} />);

    userEvent.type(screen.getByLabelText('URL'), 'Not a url');
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter a valid URL',
        }),
      ).toBeInTheDocument();
    });
  });

  test('shows a validation error when the url is not from an allowed domain', async () => {
    render(<EditableEmbedForm onCancel={noop} onSubmit={noop} />);

    userEvent.type(screen.getByLabelText('URL'), 'http://test.com');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'URL must be on a permitted domain',
        }),
      ).toBeInTheDocument();
    });
  });

  test('calls `onSubmit` with the form values when submitted successfully', async () => {
    const handleSubmit = jest.fn();
    render(<EditableEmbedForm onCancel={noop} onSubmit={handleSubmit} />);

    userEvent.type(screen.getByLabelText('Title'), 'Dashboard title');
    userEvent.type(
      screen.getByLabelText('URL'),
      'https://department-for-education.shinyapps.io/test-dashboard',
    );
    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(
        {
          title: 'Dashboard title',
          url: 'https://department-for-education.shinyapps.io/test-dashboard',
        },
        expect.anything(),
      );
    });
  });

  function render(child: ReactNode): RenderResult {
    return baseRender(
      <TestConfigContextProvider>{child}</TestConfigContextProvider>,
    );
  }
});
