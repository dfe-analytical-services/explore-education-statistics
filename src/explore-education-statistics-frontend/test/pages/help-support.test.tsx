import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import HelpSupportPage from '@frontend/pages/help-support';
import { screen, within } from '@testing-library/react';
import React from 'react';

// Lives outside `src/pages`, as Next.js would treat a test file there as a page.
describe('HelpSupportPage', () => {
  const standardsLinkName =
    'Standards for official statistics (opens in new tab)';

  test('renders the EES standards link and general enquiry section by default', () => {
    render(<HelpSupportPage />);

    const standardsLink = screen.getByRole('link', {
      name: standardsLinkName,
    });
    expect(standardsLink).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education',
    );
    expect(standardsLink.closest('p')).toHaveTextContent(
      `and DfE's ${standardsLinkName}`,
    );

    expect(
      screen.getByRole('heading', { name: 'Have a general enquiry?' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Have a general question about education or the Department for Education?',
      ),
    ).toBeInTheDocument();
  });

  test('renders the standards link and organisation acronym from the tenant', () => {
    render(<HelpSupportPage />, { tenant: fullTestTenant });

    const standardsLink = screen.getByRole('link', {
      name: standardsLinkName,
    });
    expect(standardsLink).toHaveAttribute(
      'href',
      'https://test-organisation.example.com/standards',
    );
    expect(standardsLink.closest('p')).toHaveTextContent(
      `and TO's ${standardsLinkName}`,
    );

    expect(
      screen.getByText(/contains all of TO's published official statistics/),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'the Test Organisation (TO) (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://test-organisation.example.com');
    expect(
      screen.getByText(
        'The test statistics service is operated by the Test Organisation (TO).',
      ),
    ).toBeInTheDocument();
  });

  test('uses the organisation name when the tenant has no acronym', () => {
    render(<HelpSupportPage />, {
      tenant: {
        ...fullTestTenant,
        organisation: minimalTestTenant.organisation,
      },
    });

    const standardsParagraph = screen
      .getByRole('link', { name: standardsLinkName })
      .closest('p');
    expect(standardsParagraph).toHaveTextContent(
      `and the Test Organisation's ${standardsLinkName}`,
    );

    expect(
      screen.getByText(
        /contains all of the Test Organisation's published official statistics/,
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByRole('main')).getByRole('link', {
        name: 'the Test Organisation (opens in new tab)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'The test statistics service is operated by the Test Organisation.',
      ),
    ).toBeInTheDocument();
  });

  test('hides the standards link for a tenant with no officialStatisticsStandardsUrl', () => {
    render(<HelpSupportPage />, { tenant: testTenant });

    expect(
      screen.queryByRole('link', { name: standardsLinkName }),
    ).not.toBeInTheDocument();

    // The sentence still ends cleanly after the Code of Practice link.
    const codeOfPracticeParagraph = screen
      .getByRole('link', {
        name: 'Code of Practice for Official Statistics (opens in new tab)',
      })
      .closest('p');
    expect(codeOfPracticeParagraph).toHaveTextContent(
      /Code of Practice for Official Statistics \(opens in new tab\)\.$/,
    );
  });

  test('hides the general enquiry section for a tenant with no publicEnquiries', () => {
    render(<HelpSupportPage />, { tenant: minimalTestTenant });

    expect(
      screen.queryByRole('heading', { name: 'Have a general enquiry?' }),
    ).not.toBeInTheDocument();
    expect(screen.queryByText(/Opening hours/)).not.toBeInTheDocument();
  });
});
