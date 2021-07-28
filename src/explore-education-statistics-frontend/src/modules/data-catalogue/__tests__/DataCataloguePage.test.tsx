import DataCataloguePage from '@frontend/modules/data-catalogue/DataCataloguePage';
import { Theme } from '@common/services/tableBuilderService';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import preloadAll from 'jest-next-dynamic';
import React from 'react';

describe('DataCataloguePage', () => {
  const testPublicationId = '536154f5-7f82-4dc7-060a-08d9097c1945';

  const testThemes: Theme[] = [
    {
      id: 'ee1855ca-d1e1-4f04-a795-cbd61d326a1f',
      title: 'Pupils and schools',
      slug: 'pupils-and-schools',
      topics: [
        {
          id: 'c9f0b897-d58a-42b0-9d12-ca874cc7c810',
          title: 'Admission appeals',
          slug: 'admission-appeals',
          publications: [
            {
              id: testPublicationId,
              title: 'Test publication',
              slug: 'test-publication',
            },
          ],
        },
      ],
    },
  ];

  beforeAll(preloadAll);

  test('renders the page correctly with themes and publications', async () => {
    render(<DataCataloguePage themes={testThemes} />);

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
});
