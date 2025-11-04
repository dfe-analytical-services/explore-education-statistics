import render from '@common-test/render';
import Page from '@frontend/components/Page';
import { screen } from '@testing-library/react';
import React from 'react';

describe('Page', () => {
  test('renders the caption outside the h1 by default, if provided', () => {
    render(<Page title="Page Title" caption="Page Title Caption" />);
    expect(
      screen.queryByRole('heading', {
        name: 'Page Title',
        level: 1,
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', {
        name: 'Page Title Caption Page Title',
        level: 1,
      }),
    ).not.toBeInTheDocument();
  });

  test('renders the caption inside h1 if required', () => {
    render(
      <Page
        title="Page Title"
        caption="Page Title Caption"
        captionInsideTitle
      />,
    );
    expect(
      screen.queryByRole('heading', {
        name: 'Page Title Caption Page Title',
        level: 1,
      }),
    ).toBeInTheDocument();
  });
});
