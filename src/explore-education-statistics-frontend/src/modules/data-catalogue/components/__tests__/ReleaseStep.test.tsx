import { ReleaseFormSubmitHandler } from '@frontend/modules/data-catalogue/components/ReleaseForm';
import ReleaseStep from '@frontend/modules/data-catalogue/components/ReleaseStep';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { ReleaseSummary } from '@common/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';

describe('ReleaseStep', () => {
  const testReleases: ReleaseSummary[] = [
    {
      id: 'release-3',
      latestRelease: true,
      slug: 'release-3-slug',
      title: 'Academic year 2021/22',
    } as ReleaseSummary,
    {
      id: 'release-2',
      latestRelease: false,
      slug: 'release-2-slug',
      title: 'Academic year 2020/21',
    } as ReleaseSummary,
    {
      id: 'release-1',
      latestRelease: false,
      slug: 'release-1-slug',
      title: 'Academic year 2019/20',
    } as ReleaseSummary,
  ];

  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    isActive: true,
    isEnabled: true,
    isLoading: false,
    setCurrentStep: (step, task) => task?.(),
    goToNextStep: task => task?.(),
    goToPreviousStep: task => task?.(),
  };

  test('renders with releases', () => {
    render(
      <ReleaseStep {...wizardProps} releases={testReleases} onSubmit={noop} />,
    );

    expect(screen.getByText('Choose a release')).toBeInTheDocument();

    expect(screen.getByLabelText(/Search/)).toBeInTheDocument();

    const releases = screen.getAllByRole('radio');

    expect(releases.length).toBe(3);
    expect(releases[0]).toHaveAttribute('value', 'release-3');
    expect(releases[0]).toBeEnabled();
    expect(releases[0]).not.toBeChecked();
    expect(releases[0]).toEqual(screen.getByLabelText('Academic year 2021/22'));
    expect(
      screen.getByTestId('Radio item for Academic year 2021/22'),
    ).toHaveTextContent('This is the latest data');

    expect(releases[1]).toHaveAttribute('value', 'release-2');
    expect(releases[1]).toBeEnabled();
    expect(releases[1]).not.toBeChecked();
    expect(releases[1]).toEqual(screen.getByLabelText('Academic year 2020/21'));

    expect(releases[2]).toHaveAttribute('value', 'release-1');
    expect(releases[2]).toBeEnabled();
    expect(releases[2]).not.toBeChecked();
    expect(releases[2]).toEqual(screen.getByLabelText('Academic year 2019/20'));

    expect(
      screen.getByRole('button', { name: 'Next step' }),
    ).toBeInTheDocument();
  });

  test('filters the list of releases when searching', async () => {
    render(
      <ReleaseStep {...wizardProps} releases={testReleases} onSubmit={noop} />,
    );

    expect(screen.getAllByRole('radio')).toHaveLength(3);

    await userEvent.type(screen.getByLabelText(/Search/), '2020/21');

    await waitFor(() => {
      expect(screen.getAllByRole('radio')).toHaveLength(1);

      expect(
        screen.getByLabelText('Academic year 2020/21'),
      ).toBeInTheDocument();
    });
  });

  test('when there is only one release it is pre-selected and no search is shown', () => {
    render(
      <ReleaseStep
        {...wizardProps}
        releases={[testReleases[0]]}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText(/Search/)).not.toBeInTheDocument();

    const releases = screen.getAllByRole('radio');

    expect(releases.length).toBe(1);
    expect(releases[0]).toHaveAttribute('value', 'release-3');
    expect(releases[0]).toBeEnabled();
    expect(releases[0]).toBeChecked();
  });

  test('renders a message when there are no releases', () => {
    render(<ReleaseStep {...wizardProps} releases={[]} onSubmit={noop} />);

    expect(screen.queryByLabelText(/Search/)).not.toBeInTheDocument();

    expect(
      screen.queryByRole('group', {
        name: 'Choose a release from the list below',
      }),
    ).not.toBeInTheDocument();

    expect(screen.getByText('No releases available.')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Download selected files' }),
    ).not.toBeInTheDocument();
  });

  test('shows validation error if a release is not selected before clicking `Next step`', async () => {
    const handleSubmit = jest.fn();

    render(
      <ReleaseStep
        {...wizardProps}
        releases={testReleases}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', { name: 'Choose a release' }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('clicking `Next step` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();

    render(
      <ReleaseStep
        {...wizardProps}
        releases={testReleases}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('Academic year 2019/20'));
    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<
        Parameters<ReleaseFormSubmitHandler>
      >({ release: testReleases[2] });
    });
  });

  test('renders with the selected release title when not active', () => {
    render(
      <ReleaseStep
        {...wizardProps}
        isActive={false}
        releases={testReleases}
        selectedRelease={testReleases[1]}
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('radio')).toHaveLength(0);

    expect(screen.getByTestId('Release')).toHaveTextContent(
      'Academic year 2020/21',
    );
  });
});
