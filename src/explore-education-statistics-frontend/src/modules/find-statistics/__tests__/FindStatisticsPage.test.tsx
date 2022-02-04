import FindStatisticsPage from '@frontend/modules/find-statistics/FindStatisticsPage';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import {
  testThemes,
  testThemeWithAllPublicationTypes,
} from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';

describe('FindStatisticsPage', () => {
  test('renders basic page correctly', async () => {
    render(<FindStatisticsPage themes={[]} />);

    expect(screen.getByTestId('page-title')).toHaveTextContent(
      'Find statistics and data',
    );

    expect(
      screen.getByLabelText(
        'Search to find the statistics and data you’re looking for.',
      ),
    ).toBeInTheDocument();
  });

  test('renders related information links', async () => {
    render(<FindStatisticsPage themes={[]} />);

    const relatedInformationNav = screen.getByRole('navigation', {
      name: 'Related information',
    });

    const relatedInformationLinks = within(relatedInformationNav).getAllByRole(
      'link',
    );

    expect(relatedInformationLinks).toHaveLength(2);
    expect(relatedInformationLinks[0]).toHaveTextContent(
      'Education statistics: methodology',
    );
    expect(relatedInformationLinks[1]).toHaveTextContent(
      'Education statistics: glossary',
    );
  });

  test('renders without the accordion when there are no themes', async () => {
    render(<FindStatisticsPage themes={[]} />);

    expect(screen.queryByTestId('accordion')).not.toBeInTheDocument();

    expect(
      screen.getByText('No data currently published.', { selector: 'div' }),
    ).toBeInTheDocument();
  });

  test('renders the accordion correctly with multiple themes, topics and publications', async () => {
    const { container } = render(<FindStatisticsPage themes={testThemes} />);

    const contentAccordion = screen.getByTestId('accordion');

    expect(container.querySelectorAll('details[open]')).toHaveLength(0);

    const accordionSections = within(contentAccordion).getAllByTestId(
      'accordionSection',
    );

    expect(accordionSections).toHaveLength(2);

    // Theme 1
    const theme1 = within(accordionSections[0]);

    expect(
      theme1.getByRole('button', {
        name: 'Early years',
      }),
    ).toBeInTheDocument();
    expect(
      theme1.getByText(
        'Including early years foundation stage profile and early years surveys statistics',
      ),
    ).toBeInTheDocument();
    userEvent.click(
      theme1.getByRole('button', {
        name: 'Early years',
      }),
    );

    const theme1Details = theme1.getAllByRole('group');
    expect(theme1Details).toHaveLength(1);

    // Theme 1 Topic 1
    const theme1Topic1 = within(theme1Details[0]);

    expect(
      theme1Topic1.getByRole('button', {
        name: 'Childcare and early years',
      }),
    ).toBeInTheDocument();
    userEvent.click(
      theme1Topic1.getByRole('button', {
        name: 'Childcare and early years',
      }),
    );

    const theme1Topic1Types = theme1Topic1.getAllByTestId('publication-type');
    expect(theme1Topic1Types).toHaveLength(1);

    // Theme 1 Topic 1 Type 1
    const theme1Topic1Type1 = within(theme1Topic1Types[0]);

    expect(
      theme1Topic1Type1.getByRole('heading', {
        level: 3,
      }),
    ).toHaveTextContent('National and official statistics');

    const theme1Topic1Type1Publications = theme1Topic1Type1.getAllByRole(
      'listitem',
    );

    expect(theme1Topic1Type1Publications).toHaveLength(1);
    expect(theme1Topic1Type1Publications[0]).toHaveTextContent(
      'Education provision: children under 5 years of age',
    );

    // Theme 2
    const theme2 = within(accordionSections[1]);

    expect(
      theme2.getByRole('button', {
        name: 'Pupils and schools',
      }),
    ).toBeInTheDocument();
    expect(
      theme2.getByText(
        'Including absence, application and offers, capacity, exclusion and special educational needs (SEN) statistics',
      ),
    ).toBeInTheDocument();
    userEvent.click(
      theme2.getByRole('button', {
        name: 'Pupils and schools',
      }),
    );

    const theme2Details = theme2.getAllByRole('group');
    expect(theme2Details).toHaveLength(2);

    // Theme 2 Topic 1
    const theme2Topic1 = within(theme2Details[0]);

    expect(
      theme2Topic1.getByRole('button', {
        name: 'School capacity',
      }),
    ).toBeInTheDocument();
    userEvent.click(
      theme2Topic1.getByRole('button', {
        name: 'School capacity',
      }),
    );

    const theme2Topic1Types = theme2Topic1.getAllByTestId('publication-type');
    expect(theme2Topic1Types).toHaveLength(2);

    // Theme 2 Topic 1 Type 1
    const theme2Topic1Type1 = within(theme2Topic1Types[0]);

    expect(
      theme2Topic1Type1.getByRole('heading', {
        level: 3,
      }),
    ).toHaveTextContent('National and official statistics');

    const theme2Topic1Type1Publications = theme2Topic1Type1.getAllByRole(
      'listitem',
    );

    expect(theme2Topic1Type1Publications).toHaveLength(2);
    expect(theme2Topic1Type1Publications[0]).toHaveTextContent(
      'Local authority school places scorecards',
    );
    expect(theme2Topic1Type1Publications[1]).toHaveTextContent(
      'School capacity',
    );

    // Theme 2 Topic 1 Type 2
    const theme2Topic1Type2 = within(theme2Topic1Types[1]);

    expect(
      theme2Topic1Type2.getByRole('heading', {
        level: 3,
      }),
    ).toHaveTextContent('Ad hoc statistics');

    const theme2Topic1Type2Publications = theme2Topic1Type2.getAllByRole(
      'listitem',
    );

    expect(theme2Topic1Type2Publications).toHaveLength(1);
    expect(theme2Topic1Type2Publications[0]).toHaveTextContent(
      'School places sufficiency survey',
    );

    // Theme 2 Topic 2
    const theme2Topic2 = within(theme2Details[1]);

    expect(
      theme2Topic2.getByRole('button', {
        name: 'Special educational needs (SEN)',
      }),
    ).toBeInTheDocument();
    userEvent.click(
      theme2Topic2.getByRole('button', {
        name: 'Special educational needs (SEN)',
      }),
    );

    const theme2Topic2Types = theme2Topic2.getAllByTestId('publication-type');
    expect(theme2Topic2Types).toHaveLength(2);

    // Theme 2 Topic 2 Type 1
    const theme2Topic2Type1 = within(theme2Topic2Types[0]);

    expect(
      theme2Topic2Type1.getByRole('heading', {
        level: 3,
      }),
    ).toHaveTextContent('National and official statistics');

    const theme2Topic2Type1Publications = theme2Topic2Type1.getAllByRole(
      'listitem',
    );

    expect(theme2Topic2Type1Publications).toHaveLength(2);
    expect(theme2Topic2Type1Publications[0]).toHaveTextContent(
      'Education, health and care plans',
    );
    expect(theme2Topic2Type1Publications[1]).toHaveTextContent(
      'Special educational needs in England',
    );

    // Theme 2 Topic 2 Type 2
    const theme2Topic2Type2 = within(theme2Topic2Types[1]);

    expect(
      theme2Topic2Type2.getByRole('heading', {
        level: 3,
      }),
    ).toHaveTextContent('Not yet on this service');

    const theme2Topic2Type2Publications = theme2Topic2Type2.getAllByRole(
      'listitem',
    );

    expect(theme2Topic2Type2Publications).toHaveLength(1);
    expect(theme2Topic2Type2Publications[0]).toHaveTextContent(
      'Special educational needs: analysis and summary of data sources',
    );
  });

  test('renders publication types in correct order within a topic', async () => {
    render(<FindStatisticsPage themes={[testThemeWithAllPublicationTypes]} />);

    const contentAccordion = screen.getByTestId('accordion');
    const accordionSections = within(contentAccordion).getAllByTestId(
      'accordionSection',
    );

    expect(accordionSections).toHaveLength(1);

    const theme = within(accordionSections[0]);

    expect(
      theme.getByRole('button', {
        name: 'Test theme',
      }),
    ).toBeInTheDocument();
    userEvent.click(
      theme.getByRole('button', {
        name: 'Test theme',
      }),
    );

    const themeDetails = theme.getAllByRole('group');
    expect(themeDetails).toHaveLength(1);

    const topic = within(themeDetails[0]);

    expect(
      topic.getByRole('button', {
        name: 'Test topic',
      }),
    ).toBeInTheDocument();
    userEvent.click(
      topic.getByRole('button', {
        name: 'Test topic',
      }),
    );

    const types = topic.getAllByTestId('publication-type');
    expect(types).toHaveLength(5);

    expect(types[0]).toHaveTextContent('National and official statistics');
    expect(types[1]).toHaveTextContent('Experimental statistics');
    expect(types[2]).toHaveTextContent('Ad hoc statistics');
    expect(types[3]).toHaveTextContent('Management information');
    expect(types[4]).toHaveTextContent('Not yet on this service');
  });
});
