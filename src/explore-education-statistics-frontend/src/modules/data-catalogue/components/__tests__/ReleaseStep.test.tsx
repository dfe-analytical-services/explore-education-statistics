import ReleaseStep from '@frontend/modules/data-catalogue/components/ReleaseStep';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { Release } from '@common/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';

describe('ReleaseStep', () => {
  const testReleases: Release[] = [
    {
      id: 'test-rel-3',
      published: '2021-01-01T11:21:17.7585345',
      slug: 'test-rel-3-slug',
      title: 'Another Release',
    } as Release,
    {
      id: 'test-rel-1',
      latestRelease: true,
      published: '2021-06-30T11:21:17.7585345',
      slug: 'test-rel-1-slug',
      title: 'Release 1',
    } as Release,
    {
      id: 'test-rel-2',
      published: '2021-05-30T11:21:17.7585345',
      slug: 'test-rel-2-slug',
      title: 'Release 2',
    } as Release,
  ];

  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    setCurrentStep: noop,
    isActive: true,
    isLoading: false,
    goToNextStep: noop,
    goToPreviousStep: noop,
  };

  test('renders with releases', () => {
    render(
      <ReleaseStep {...wizardProps} releases={testReleases} onSubmit={noop} />,
    );

    expect(screen.getByText('Choose a release')).toBeInTheDocument();

    expect(screen.getByLabelText('Search releases')).toBeInTheDocument();

    const releasesGroup = within(
      screen.getByRole('group', {
        name: 'Choose a release from the list below',
      }),
    );
    const releases = releasesGroup.getAllByRole('radio');

    expect(releases.length).toBe(3);
    expect(releases[0]).toHaveAttribute('value', 'test-rel-1');
    expect(releases[0]).toBeEnabled();
    expect(releases[0]).not.toBeChecked();
    expect(releases[0]).toEqual(
      releasesGroup.getByLabelText('Release 1 (30 June 2021)'),
    );
    expect(
      screen.getByTestId('Radio item for Release 1 (30 June 2021)'),
    ).toHaveTextContent('This is the latest data');

    expect(releases[1]).toHaveAttribute('value', 'test-rel-2');
    expect(releases[1]).toBeEnabled();
    expect(releases[1]).not.toBeChecked();
    expect(releases[1]).toEqual(
      releasesGroup.getByLabelText('Release 2 (30 May 2021)'),
    );

    expect(releases[2]).toHaveAttribute('value', 'test-rel-3');
    expect(releases[2]).toBeEnabled();
    expect(releases[2]).not.toBeChecked();
    expect(releases[2]).toEqual(
      releasesGroup.getByLabelText('Another Release (1 January 2021)'),
    );

    expect(
      screen.getByRole('button', { name: 'Next step' }),
    ).toBeInTheDocument();
  });

  test('filters the list of releases when searching', async () => {
    render(
      <ReleaseStep {...wizardProps} releases={testReleases} onSubmit={noop} />,
    );

    expect(screen.getAllByRole('radio')).toHaveLength(3);

    userEvent.type(screen.getByLabelText('Search releases'), 'Another');

    await waitFor(() => {
      expect(screen.getAllByRole('radio')).toHaveLength(1);

      expect(
        screen.getByLabelText('Another Release (1 January 2021)'),
      ).toHaveAttribute('type', 'radio');
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
    expect(screen.queryByLabelText('Search releases')).not.toBeInTheDocument();

    const releasesGroup = within(
      screen.getByRole('group', {
        name: 'Choose a release from the list below',
      }),
    );
    const releases = releasesGroup.getAllByRole('radio');

    expect(releases.length).toBe(1);
    expect(releases[0]).toHaveAttribute('value', 'test-rel-3');
    expect(releases[0]).toBeEnabled();
    expect(releases[0]).toBeChecked();
  });

  test('renders a message when there are no releases', () => {
    render(<ReleaseStep {...wizardProps} releases={[]} onSubmit={noop} />);
    expect(screen.queryByLabelText('Search releases')).not.toBeInTheDocument();

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

    userEvent.click(
      screen.getByRole('radio', { name: 'Release 2 (30 May 2021)' }),
    );
    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({ releaseId: 'test-rel-2' });
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

    expect(screen.getByTestId('Release')).toHaveTextContent('Release 1');
  });
});
