import EditableEmbedForm from '@admin/components/editable/EditableEmbedForm';
import baseRender from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
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
    const { user } = render(
      <EditableEmbedForm onCancel={noop} onSubmit={noop} />,
    );

    await user.click(screen.getByLabelText('Title'));
    await user.click(screen.getByRole('button', { name: 'Save' }));

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
    const { user } = render(
      <EditableEmbedForm onCancel={noop} onSubmit={noop} />,
    );

    await user.click(screen.getByLabelText('URL'));
    await user.click(screen.getByRole('button', { name: 'Save' }));

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
    const { user } = render(
      <EditableEmbedForm onCancel={noop} onSubmit={noop} />,
    );

    await user.type(screen.getByLabelText('URL'), 'Not a url');
    await user.click(screen.getByRole('button', { name: 'Save' }));
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
    const { user } = render(
      <EditableEmbedForm onCancel={noop} onSubmit={noop} />,
    );

    await user.type(screen.getByLabelText('URL'), 'http://test.com');
    await user.click(screen.getByRole('button', { name: 'Save' }));

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
    const { user } = render(
      <EditableEmbedForm onCancel={noop} onSubmit={handleSubmit} />,
    );

    await user.type(screen.getByLabelText('Title'), 'Dashboard title');
    await user.type(
      screen.getByLabelText('URL'),
      'https://department-for-education.shinyapps.io/test-dashboard',
    );
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Dashboard title',
        url: 'https://department-for-education.shinyapps.io/test-dashboard',
      });
    });
  });

  function render(child: ReactNode) {
    return baseRender(
      <TestConfigContextProvider>{child}</TestConfigContextProvider>,
    );
  }
});
