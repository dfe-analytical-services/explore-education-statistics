import { Release } from '@common/services/publicationService';
import PublicationReleasePage from '@frontend/modules/find-statistics/PublicationReleasePage';
import { render, screen } from '@testing-library/react';
import React from 'react';
import NationalStats from './__data__/content.api.response.national.stats.json';
import OfficialStats from './__data__/content.api.response.official.stats.json';

jest.mock('next/router', () => {
  return {
    push: () => {},
    prefetch: () => {},
  };
});

describe('PublicationReleasePage', () => {
  test('renders national statistics image', () => {
    const { container } = render(
      <PublicationReleasePage data={(NationalStats as unknown) as Release} />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeDefined();
  });

  test('renders national statistics section', () => {
    render(
      <PublicationReleasePage data={(NationalStats as unknown) as Release} />,
    );

    expect(
      screen.getByRole('button', { name: 'National Statistics' }),
    ).toBeInTheDocument();
  });

  test('renders official statistics image', () => {
    const { container } = render(
      <PublicationReleasePage data={(OfficialStats as unknown) as Release} />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeNull();
  });

  test('renders official statistics section', () => {
    render(
      <PublicationReleasePage data={(OfficialStats as unknown) as Release} />,
    );

    expect(
      screen.queryByRole('button', { name: 'National Statistics' }),
    ).not.toBeInTheDocument();
  });
});
