import ApiDataSetPreviewTokenCreateForm from '@admin/pages/release/data/components/ApiDataSetPreviewTokenCreateForm';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { AuthContext } from '@admin/contexts/AuthContext';
import React from 'react';
import { PreviewTokenCreateValues } from '@admin/pages/release/data/types/PreviewTokenCreateValues';
import Mock = jest.Mock;

const defaultPermissions = {
  isBauUser: true,
  canAccessSystem: true,
  canAccessPrereleasePages: true,
  canAccessAnalystPages: true,
  canAccessAllImports: true,
  canManageAllTaxonomy: true,
  isApprover: true,
};

const bau = {
  id: 'user-1',
  name: 'Test User',
  permissions: defaultPermissions,
};

describe('ApiDataSetPreviewTokenCreateForm', () => {
  test('renders the form correctly', () => {
    render(
      <ApiDataSetPreviewTokenCreateForm onCancel={noop} onSubmit={noop} />,
    );

    expect(screen.getByLabelText('Token name')).toBeInTheDocument();
    expect(screen.getByLabelText(/I agree/)).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Continue' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('shows validation error when no label given', async () => {
    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm onCancel={noop} onSubmit={noop} />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('Enter a token name', {
        selector: '#apiDataSetTokenCreateForm-label-error',
      }),
    ).toBeInTheDocument();
  });

  test('shows validation error when agreeing to the terms of usage not checked', async () => {
    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm onCancel={noop} onSubmit={noop} />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('The terms of usage must be agreed', {
        selector: '#apiDataSetTokenCreateForm-terms-error',
      }),
    ).toBeInTheDocument();
  });

  test('submitting form successfully calls `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.type(screen.getByLabelText('Token name'), 'Test label');
    await user.click(screen.getByLabelText(/I agree/));

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    await waitFor(() => expect(handleSubmit).toHaveBeenCalledTimes(1));

    const handleSubmitParam: PreviewTokenCreateValues = {
      activates: null,
      datePresetSpan: null,
      expires: null,
      label: 'Test label',
    };
    expect(handleSubmit).toHaveBeenCalledWith(handleSubmitParam);
  });

  test('submitting form with validation error does not call `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      await screen.findByText('Enter a token name', {
        selector: '#apiDataSetTokenCreateForm-label-error',
      }),
    ).toBeInTheDocument();

    await waitFor(() => expect(handleSubmit).not.toHaveBeenCalled());
  });

  test('clicking the cancel button calls the `onCancel` handler', async () => {
    const handleCancel = jest.fn();

    const { user } = render(
      <ApiDataSetPreviewTokenCreateForm
        onCancel={handleCancel}
        onSubmit={noop}
      />,
    );

    expect(handleCancel).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    await waitFor(() => expect(handleCancel).toHaveBeenCalled());
  });

  describe('Custom dates selection', () => {
    test('sets activates as the beginning of the day and expires as the end of the day that the user selects.', async () => {
      // Arrange
      const activates = new Date();
      activates.setDate(activates.getDate() + 1);
      const expectedExpires = new Date();
      expectedExpires.setDate(expectedExpires.getDate() + 4);
      const handleSubmit = jest.fn();

      // Act
      await renderWithCustomDatesSelected(
        activates,
        expectedExpires,
        handleSubmit,
      );

      // Assert
      const expectedActivatesUtc = new Date(
        Date.UTC(
          activates.getFullYear(),
          activates.getMonth(),
          activates.getDate(),
          0,
          0,
          0,
          0,
        ),
      );
      expectedExpires.setHours(23, 59, 59, 999);

      const handleSubmitParam: PreviewTokenCreateValues = {
        activates: expectedActivatesUtc,
        datePresetSpan: null,
        expires: expectedExpires,
        label: 'Test label',
      };

      await waitFor(() =>
        expect(handleSubmit).toHaveBeenCalledWith(handleSubmitParam),
      );
    });

    test('shows validation error when start date is in the past', async () => {
      const today = new Date();
      const beforeTodayBy8Days = new Date();
      beforeTodayBy8Days.setDate(today.getDate() - 8);
      const handleSubmit = jest.fn();

      await renderWithCustomDatesSelected(
        beforeTodayBy8Days,
        today,
        handleSubmit,
      );

      expect(
        await screen.findByText('Activates date must not be in the past', {
          selector: '#apiDataSetTokenCreateForm-activates-error',
        }),
      ).toBeInTheDocument();
    });

    test('shows validation error when start date is more than 7 days from current time', async () => {
      const today = new Date();
      const afterTodayBy8Days = new Date();
      const afterTodayBy10Days = new Date();
      afterTodayBy8Days.setDate(today.getDate() + 8);
      afterTodayBy10Days.setDate(today.getDate() + 10);
      const handleSubmit = jest.fn();

      await renderWithCustomDatesSelected(
        afterTodayBy8Days,
        afterTodayBy10Days,
        handleSubmit,
      );

      expect(
        await screen.findByText(
          'Activates date must be within 7 days from today',
          {
            selector: '#apiDataSetTokenCreateForm-activates-error',
          },
        ),
      ).toBeInTheDocument();
    });

    test('shows validation error when end date is beyond 7 days from the start date', async () => {
      const today = new Date();
      const beyond7 = new Date();
      beyond7.setDate(today.getDate() + 8);
      const handleSubmit = jest.fn();

      await renderWithCustomDatesSelected(today, beyond7, handleSubmit);

      expect(
        await screen.findByText(
          'Expires date must be later than Activates date by at most 7 days',
          {
            selector: '#apiDataSetTokenCreateForm-expires-error',
          },
        ),
      ).toBeInTheDocument();
    });

    test('options are not visible for analysts', async () => {
      const today = new Date();
      const beyond7 = new Date();
      beyond7.setDate(today.getDate() + 8);
      const handleSubmit = jest.fn();
      const analyst = {
        ...bau,
        permissions: { ...defaultPermissions, isBauUser: false },
      };

      const { user } = render(
        <TestConfigContextProvider>
          <AuthContext.Provider value={{ user: analyst }}>
            <ApiDataSetPreviewTokenCreateForm
              onCancel={noop}
              onSubmit={handleSubmit}
            />
          </AuthContext.Provider>
        </TestConfigContextProvider>,
      );
      await user.type(screen.getByLabelText('Token name'), 'Test label');
      await user.click(screen.getByLabelText(/I agree/));

      expect(
        screen.queryByLabelText('Enter specific start and end dates', {
          selector: '#apiDataSetTokenCreateForm-selectionMethod-customDates',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByLabelText('Choose number of days', {
          selector: '#apiDataSetTokenCreateForm-selectionMethod-presetDays',
        }),
      ).not.toBeInTheDocument();
    });
  });
});

async function renderWithCustomDatesSelected(
  activatesDate: Date,
  expiresDate: Date,
  handleSubmit: Mock<void, [PreviewTokenCreateValues], void>,
) {
  const { user } = render(
    <TestConfigContextProvider>
      <AuthContext.Provider value={{ user: bau }}>
        <ApiDataSetPreviewTokenCreateForm
          onCancel={noop}
          onSubmit={handleSubmit}
        />
      </AuthContext.Provider>
    </TestConfigContextProvider>,
  );

  await user.type(screen.getByLabelText('Token name'), 'Test label');
  await user.click(screen.getByLabelText(/I agree/));
  const radioButtonCustomDates = await screen.findByLabelText(
    'Enter specific start and end dates',
    {
      selector: '#apiDataSetTokenCreateForm-selectionMethod-customDates',
    },
  );
  const activatesGroup = screen.getByRole('group', { name: /activates on/i });

  await user.click(radioButtonCustomDates);
  const activateDate = activatesDate.getDate().toString();
  const activateMonth = (activatesDate.getMonth() + 1).toString();
  const activateYear = activatesDate.getFullYear().toString();
  const expireDate = expiresDate.getDate().toString();
  const expireMonth = (expiresDate.getMonth() + 1).toString();
  const expireYear = expiresDate.getFullYear().toString();

  await user.type(within(activatesGroup).getByLabelText(/day/i), activateDate);
  await user.type(
    within(activatesGroup).getByLabelText(/month/i),
    activateMonth,
  );
  await user.type(within(activatesGroup).getByLabelText(/year/i), activateYear);

  const expiresGroup = screen.getByRole('group', { name: /expires on/i });
  await user.type(within(expiresGroup).getByLabelText(/day/i), expireDate);
  await user.type(within(expiresGroup).getByLabelText(/month/i), expireMonth);
  await user.type(within(expiresGroup).getByLabelText(/year/i), expireYear);

  await user.click(screen.getByRole('button', { name: 'Continue' }));
}
