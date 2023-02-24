import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import PublicPreReleaseAccessForm from '@admin/pages/release/pre-release/components/PublicPreReleaseAccessForm';
import { render, screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import userEvent from '@testing-library/user-event';

describe('PublicPreReleaseAccessForm', () => {
  const testAccessList = `
    <p>Test pre-release access list</p>
    <ul>
        <li>Test person 1</li>
        <li>Test person 2</li>
    </ul>
  `;

  test('renders with existing access list correctly', async () => {
    render(
      <TestConfigContextProvider>
        <PublicPreReleaseAccessForm
          canUpdateRelease
          preReleaseAccessList={testAccessList}
          onSubmit={noop}
        />
      </TestConfigContextProvider>,
    );

    expect(
      screen.getByText('Test pre-release access list', { selector: 'p' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test person 1', { selector: 'li' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test person 2', { selector: 'li' }),
    ).toBeInTheDocument();
  });

  test('clicking Create button renders form with default text', () => {
    render(
      <TestConfigContextProvider>
        <PublicPreReleaseAccessForm
          canUpdateRelease
          preReleaseAccessList=""
          onSubmit={noop}
        />
      </TestConfigContextProvider>,
    );

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();

    userEvent.click(
      screen.getByRole('button', {
        name: 'Create public pre-release access list',
      }),
    );

    expect(screen.getByLabelText('Public access list')).toHaveTextContent(
      'Beside Department for Education (DfE) professional and production staff the',
    );
  });

  test(`renders with existing access list correctly`, async () => {
    render(
      <TestConfigContextProvider>
        <PublicPreReleaseAccessForm
          canUpdateRelease={false}
          isReleaseLive
          preReleaseAccessList={testAccessList}
          onSubmit={noop}
        />
      </TestConfigContextProvider>,
    );

    expect(
      screen.getByText(
        'This release has been approved, and can no longer be updated.',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Test pre-release access list', { selector: 'p' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test person 1', { selector: 'li' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test person 2', { selector: 'li' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        'http://localhost/publication/publication-1/release/release-1/prerelease',
      ),
    ).not.toBeInTheDocument();
  });

  test('submitting form hides the form', async () => {
    render(
      <TestConfigContextProvider>
        <PublicPreReleaseAccessForm
          canUpdateRelease
          preReleaseAccessList=""
          onSubmit={noop}
        />
      </TestConfigContextProvider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Create public pre-release access list',
      }),
    );

    expect(screen.getByLabelText('Public access list')).toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Save access list' }));

    await waitFor(() => {
      expect(
        screen.queryByLabelText('Public access list'),
      ).not.toBeInTheDocument();
    });
  });

  test('submitting form calls `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    render(
      <TestConfigContextProvider>
        <PublicPreReleaseAccessForm
          canUpdateRelease
          preReleaseAccessList=""
          onSubmit={handleSubmit}
        />
      </TestConfigContextProvider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Create public pre-release access list',
      }),
    );

    userEvent.clear(screen.getByLabelText('Public access list'));
    await userEvent.type(
      screen.getByLabelText('Public access list'),
      'Test updated access list',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save access list' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenLastCalledWith({
        preReleaseAccessList: 'Test updated access list',
      });
    });
  });

  test('does not render the Create / Edit button and shows a warning if the user does not have permission to update the release', () => {
    render(
      <TestConfigContextProvider>
        <PublicPreReleaseAccessForm
          canUpdateRelease={false}
          preReleaseAccessList={testAccessList}
          onSubmit={noop}
        />
      </TestConfigContextProvider>,
    );
    expect(screen.queryByTestId('access-list-btn')).not.toBeInTheDocument();
  });
});
