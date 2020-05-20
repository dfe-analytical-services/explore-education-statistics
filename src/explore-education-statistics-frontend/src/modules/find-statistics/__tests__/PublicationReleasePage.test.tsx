import React from 'react';
import { render } from '@testing-library/react';

import PublicationReleasePage from '@frontend/modules/find-statistics/PublicationReleasePage';
import { Release } from '@common/services/publicationService';
import OfficialStats from './__data__/content.api.response.official.stats.json';
import NationalStats from './__data__/content.api.response.national.stats.json';

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
    const { container } = render(
      <PublicationReleasePage data={(NationalStats as unknown) as Release} />,
    );

    const elements = [
      // @ts-ignore
      ...container.querySelectorAll(
        '.govuk-accordion__section-header h3 button',
      ),
    ] as Element[];

    expect(
      elements.find(
        ele =>
          ele.textContent && ele.textContent.includes('National Statistics'),
      ),
    ).toBeDefined();
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
    const { container } = render(
      <PublicationReleasePage data={(OfficialStats as unknown) as Release} />,
    );

    const elements = [
      // @ts-ignore
      ...container.querySelectorAll(
        '.govuk-accordion__section-header h3 button',
      ),
    ] as Element[];

    expect(
      elements.find(
        ele =>
          ele.textContent && ele.textContent.includes('National Statistics'),
      ),
    ).not.toBeDefined();
  });
});
