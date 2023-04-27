import { Subject } from '@common/services/tableBuilderService';
import DownloadStep from '@frontend/modules/data-catalogue/components/DownloadStep';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { getDescribedBy } from '@common-test/queries';
import { ReleaseSummary } from '@common/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('DownloadStep', () => {
  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 3,
    currentStep: 3,
    setCurrentStep: noop,
    isActive: true,
    isEnabled: true,
    isLoading: false,
    goToNextStep: noop,
    goToPreviousStep: noop,
  };

  const testSubjects: Subject[] = [
    {
      id: 'test-subject-2',
      name: 'Another Subject',
      content: 'Some content here 2',
      geographicLevels: ['Local Authority'],
      timePeriods: {
        from: '2016',
        to: '2019',
      },
      file: {
        id: 'file-2',
        name: 'Another Subject',
        fileName: 'file-2.csv',
        extension: 'csv',
        size: '20 Mb',
        type: 'Data',
      },
    },
    {
      id: 'test-subject-1',
      name: 'Subject 1',
      content: 'Some content here 1',
      geographicLevels: ['National'],
      timePeriods: {
        from: '2018',
        to: '2019',
      },
      file: {
        id: 'file-1',
        name: 'Subject 1',
        fileName: 'file-1.csv',
        extension: 'csv',
        size: '10 Mb',
        type: 'Data',
      },
    },
    {
      id: 'test-subject-3',
      name: 'Subject 3',
      content: 'Some content here 3',
      geographicLevels: ['Local Authority District'],
      timePeriods: {
        from: '2017',
        to: '2019',
      },
      file: {
        id: 'file-3',
        name: 'Subject 3',
        fileName: 'file-3.csv',
        extension: 'csv',
        size: '30 Mb',
        type: 'Data',
      },
    },
  ];

  const testNotLatestRelease = {
    id: 'release-1',
    latestRelease: false,
  } as ReleaseSummary;

  const testLatestRelease = {
    id: 'release-2',
    latestRelease: true,
  } as ReleaseSummary;

  test('renders with downloads', () => {
    render(
      <DownloadStep {...wizardProps} subjects={testSubjects} onSubmit={noop} />,
    );

    expect(screen.getByText('Choose files to download')).toBeInTheDocument();

    const downloadsGroup = within(
      screen.getByRole('group', {
        name: /Choose files to download/,
      }),
    );
    const downloads = downloadsGroup.getAllByRole('checkbox');

    expect(downloads.length).toBe(3);

    expect(downloads[0]).toHaveAttribute('value', 'file-2');
    expect(downloads[0]).toBeEnabled();
    expect(downloads[0]).not.toBeChecked();
    expect(downloads[0]).toEqual(
      downloadsGroup.getByLabelText('Another Subject (csv, 20 Mb)'),
    );

    const subject2Hint = within(getDescribedBy(downloads[0]));
    expect(
      within(subject2Hint.getByTestId('Content')).getByText(
        'Some content here 2',
      ),
    ).toBeInTheDocument();
    expect(subject2Hint.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority',
    );
    expect(subject2Hint.getByTestId('Time period')).toHaveTextContent(
      '2016 to 2019',
    );

    expect(downloads[1]).toHaveAttribute('value', 'file-1');
    expect(downloads[1]).toBeEnabled();
    expect(downloads[1]).not.toBeChecked();
    expect(downloads[1]).toEqual(
      downloadsGroup.getByLabelText('Subject 1 (csv, 10 Mb)'),
    );
    const subject1Hint = within(getDescribedBy(downloads[1]));
    expect(
      within(subject1Hint.getByTestId('Content')).getByText(
        'Some content here 1',
      ),
    ).toBeInTheDocument();
    expect(subject1Hint.getByTestId('Geographic levels')).toHaveTextContent(
      'National',
    );
    expect(subject1Hint.getByTestId('Time period')).toHaveTextContent(
      '2018 to 2019',
    );

    expect(downloads[2]).toHaveAttribute('value', 'file-3');
    expect(downloads[2]).toBeEnabled();
    expect(downloads[2]).not.toBeChecked();
    expect(downloads[2]).toEqual(
      downloadsGroup.getByLabelText('Subject 3 (csv, 30 Mb)'),
    );

    const subject3Hint = within(getDescribedBy(downloads[2]));
    expect(
      within(subject3Hint.getByTestId('Content')).getByText(
        'Some content here 3',
      ),
    ).toBeInTheDocument();
    expect(subject3Hint.getByTestId('Geographic levels')).toHaveTextContent(
      'Local Authority District',
    );
    expect(subject3Hint.getByTestId('Time period')).toHaveTextContent(
      '2017 to 2019',
    );

    expect(
      screen.getByRole('button', { name: 'Download selected files' }),
    ).toBeInTheDocument();
  });

  test('shows latest data tag', () => {
    render(
      <DownloadStep
        {...wizardProps}
        subjects={testSubjects}
        release={testLatestRelease}
        onSubmit={noop}
      />,
    );

    expect(screen.getByText('This is the latest data')).toBeInTheDocument();
  });

  test('shows not latest data tag', () => {
    render(
      <DownloadStep
        {...wizardProps}
        subjects={testSubjects}
        release={testNotLatestRelease}
        onSubmit={noop}
      />,
    );

    expect(screen.getByText('This is not the latest data')).toBeInTheDocument();
  });

  test('shows not latest data tag when hideLatestDataTag is true', () => {
    render(
      <DownloadStep
        {...wizardProps}
        subjects={testSubjects}
        release={testLatestRelease}
        onSubmit={noop}
        hideLatestDataTag
      />,
    );

    expect(screen.getByText('This is not the latest data')).toBeInTheDocument();
  });

  test('renders a message when there are no downloads', () => {
    const handleSubmit = jest.fn();
    render(
      <DownloadStep {...wizardProps} subjects={[]} onSubmit={handleSubmit} />,
    );

    expect(
      screen.queryByRole('group', {
        name: 'Choose files from the list below',
      }),
    ).not.toBeInTheDocument();

    expect(screen.getByText('No downloads available.')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Download selected files' }),
    ).not.toBeInTheDocument();
  });

  test('shows validation error if a download is not selected before clicking the download button', async () => {
    const handleSubmit = jest.fn();

    render(
      <DownloadStep
        {...wizardProps}
        subjects={testSubjects}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Download selected files' }),
    );

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', { name: 'Choose a file' }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('clicking the download button calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <DownloadStep
        {...wizardProps}
        subjects={testSubjects}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(
      screen.getByRole('checkbox', { name: 'Subject 1 (csv, 10 Mb)' }),
    );
    userEvent.click(
      screen.getByRole('button', { name: 'Download selected files' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(
        { files: ['file-1'] },
        expect.anything(),
      );
    });
  });
});
