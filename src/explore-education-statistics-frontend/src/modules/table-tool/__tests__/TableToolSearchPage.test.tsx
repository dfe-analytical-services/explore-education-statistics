import render from '@common-test/render';
import {
  testPublicationSummary,
  testReleaseVersionSummary,
} from '@frontend/modules/find-statistics/__tests__/__data__/testReleaseData';
import TableToolSearchPage from '@frontend/modules/table-tool/TableToolSearchPage';
import { screen } from '@testing-library/react';

describe('TableToolSearchPage', () => {
  test('renders the page correctly with search form', async () => {
    render(
      <TableToolSearchPage
        publicationSummary={testPublicationSummary}
        latestReleaseVersion={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Pupil attendance in schools',
        level: 1,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText('Search these statistics'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: /Help and example searches/,
      }),
    ).toBeInTheDocument();
  });
});
