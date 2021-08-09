import DataCataloguePage from '@frontend/modules/data-catalogue/DataCataloguePage';
import { DownloadTheme } from '@common/services/themeService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import preloadAll from 'jest-next-dynamic';
import React from 'react';

describe('DataCataloguePage', () => {
  const testThemes: DownloadTheme[] = [
    {
      id: 'theme-1',
      title: 'Pupils and schools',
      topics: [
        {
          id: 'topic-1',
          title: 'Admission appeals',
          slug: 'admission-appeals',
          publications: [
            {
              id: 'publication-1',
              title: 'Test publication',
              slug: 'test-publication',
            },
          ],
        },
      ],
    } as DownloadTheme,
  ];

  beforeAll(preloadAll);

  test('renders the page correctly with themes and publications', async () => {
    render(<DataCataloguePage themes={testThemes} />);

    expect(screen.getByText('Choose a publication')).toBeInTheDocument();

    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByLabelText('Test publication')).not.toBeVisible();

    userEvent.click(screen.getByRole('button', { name: 'Pupils and schools' }));
    userEvent.click(screen.getByRole('button', { name: 'Admission appeals' }));

    // Check there is only one radio for the publication
    expect(screen.getAllByRole('radio', { hidden: true })).toHaveLength(1);
    expect(screen.getByLabelText('Test publication')).toBeVisible();
  });

  // EES-2207 this test will need to be updated to mock the api responses insted of using the fake data
  test('can go through all the steps to get to download files', async () => {
    render(<DataCataloguePage themes={testThemes} />);

    expect(screen.getByText('Choose a publication')).toBeInTheDocument();
    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );
    userEvent.click(screen.getByRole('button', { name: 'Pupils and schools' }));
    userEvent.click(screen.getByRole('button', { name: 'Admission appeals' }));
    userEvent.click(screen.getByRole('radio', { name: 'Test publication' }));
    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-1')).not.toHaveAttribute(
        'aria-current',
        'step',
      );
      expect(screen.getByTestId('wizardStep-2')).toHaveAttribute(
        'aria-current',
        'step',
      );
    });

    expect(screen.getByText('Choose a release')).toBeInTheDocument();
    expect(screen.getAllByRole('radio')).toHaveLength(3);
    userEvent.click(
      screen.getByRole('radio', { name: 'Another Release (1 January 2021)' }),
    );
    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-2')).not.toHaveAttribute(
        'aria-current',
        'step',
      );
      expect(screen.getByTestId('wizardStep-3')).toHaveAttribute(
        'aria-current',
        'step',
      );
    });

    expect(screen.getByText('Choose files to download')).toBeInTheDocument();
    expect(screen.getAllByRole('checkbox')).toHaveLength(3);
    expect(
      screen.getByRole('button', { name: 'Download selected files' }),
    ).toBeInTheDocument();
  });
});
