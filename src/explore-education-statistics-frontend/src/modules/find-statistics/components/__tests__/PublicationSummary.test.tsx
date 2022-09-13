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
    ).toHaveAttribute('href', '/find-statistics/publication-1-slug');
    expect(screen.getByText('Publication 1 summary')).toBeInTheDocument();

    expect(screen.getByTestId('release-type')).toHaveTextContent(
      'Ad hoc statistics',
    );
    expect(screen.getByTestId('published')).toHaveTextContent('8 Jun 2021');
    expect(screen.getByTestId('theme')).toHaveTextContent('Theme 1');
  });

  test('renders a legacy publication correctly', () => {
    render(<PublicationSummary publication={testPublications[2]} />);

    const heading = screen.getByRole('heading', { name: 'Publication 3' });
    expect(
      within(heading).getByRole('link', { name: 'Publication 3' }),
    ).toHaveAttribute('href', 'http://test.com');
    expect(
      screen.getByText(
        'Not yet on this service. Currently available via Statistics at DfE.',
      ),
    ).toBeInTheDocument();
    expect(screen.queryByRole('list')).not.toBeInTheDocument();
  });
});
