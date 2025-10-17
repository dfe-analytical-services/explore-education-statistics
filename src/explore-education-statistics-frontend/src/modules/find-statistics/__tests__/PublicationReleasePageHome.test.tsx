import render from '@common-test/render';
import PublicationReleasePageHome from '@frontend/modules/find-statistics/PublicationReleasePageHome';
import { screen, within } from '@testing-library/react';
import React from 'react';
import {
  testPublicationSummary,
  testReleaseHomeContent,
  testReleaseVersionSummary,
} from './__data__/testReleaseData';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

describe('PublicationReleasePageHome', () => {
  test('Does not render summary block and publication summary on desktop', () => {
    render(
      <PublicationReleasePageHome
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
        homeContent={testReleaseHomeContent}
      />,
    );

    expect(
      screen.queryByRole('heading', {
        level: 2,
        name: 'Introduction',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders summary block and publication summary on mobile', () => {
    mockIsMedia = true;
    render(
      <PublicationReleasePageHome
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
        homeContent={testReleaseHomeContent}
      />,
    );

    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'Introduction',
      }),
    ).toBeInTheDocument();

    mockIsMedia = false;
  });

  test('renders summary section if summary content exists', () => {
    render(
      <PublicationReleasePageHome
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
        homeContent={testReleaseHomeContent}
      />,
    );

    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'Background information',
      }),
    ).toBeInTheDocument();
  });

  test('does not render summary section if no summary content', () => {
    render(
      <PublicationReleasePageHome
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
        homeContent={{
          ...testReleaseHomeContent,
          summarySection: {
            ...testReleaseHomeContent.summarySection,
            content: [],
          },
        }}
      />,
    );

    expect(
      screen.queryByRole('heading', {
        level: 2,
        name: 'Background information',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders headlines section', () => {
    render(
      <PublicationReleasePageHome
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
        homeContent={testReleaseHomeContent}
      />,
    );
    const headlinesSection = screen.getByTestId('headlines-section');
    expect(headlinesSection).toBeInTheDocument();

    expect(
      within(headlinesSection).getByRole('heading', {
        level: 2,
        name: 'Headline facts and figures',
      }),
    ).toBeInTheDocument();
  });

  test('renders content sections as normal sections on desktop', () => {
    render(
      <PublicationReleasePageHome
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
        homeContent={testReleaseHomeContent}
      />,
    );

    const content = screen.getByTestId('home-content');
    expect(content).toBeInTheDocument();

    expect(within(content).queryByTestId('accordion')).not.toBeInTheDocument();
    expect(within(content).getAllByRole('heading', { level: 2 })).toHaveLength(
      3,
    );
  });

  test('renders content sections as accordions on mobile', () => {
    mockIsMedia = true;
    render(
      <PublicationReleasePageHome
        publicationSummary={testPublicationSummary}
        releaseVersionSummary={testReleaseVersionSummary}
        homeContent={testReleaseHomeContent}
      />,
    );

    const content = screen.getByTestId('home-content');

    expect(within(content).getByTestId('accordion')).toBeInTheDocument();
    mockIsMedia = false;
  });
});
