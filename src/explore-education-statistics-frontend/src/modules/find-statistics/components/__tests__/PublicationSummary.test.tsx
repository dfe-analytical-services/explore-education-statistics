import PublicationSummary from '@frontend/modules/find-statistics/components/PublicationSummary';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

describe('PublicationSummary', () => {
  test('renders a publication correctly', () => {
    render(<PublicationSummary publication={testPublications[0]} />);

    const heading = screen.getByRole('heading', { name: 'Publication 1' });
    expect(
      within(heading).getByRole('link', { name: 'Publication 1' }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/publication-1-slug/latest-release-slug-1',
    );
    expect(screen.getByText('Publication 1 summary')).toBeInTheDocument();

    expect(screen.getByTestId('Release type')).toHaveTextContent(
      'Ad hoc statistics',
    );
    expect(screen.getByTestId('Published')).toHaveTextContent('8 Jun 2021');
    expect(screen.getByTestId('Theme')).toHaveTextContent('Theme 1');
  });

  test('renders summary content with html highlight', () => {
    render(<PublicationSummary publication={testPublications[1]} />);

    const searchHighlight = screen.getByTestId('search-highlight');
    expect(
      within(searchHighlight).getByText('find me highlight', { exact: false })
        .nodeName,
    ).toEqual('EM');
  });
});
