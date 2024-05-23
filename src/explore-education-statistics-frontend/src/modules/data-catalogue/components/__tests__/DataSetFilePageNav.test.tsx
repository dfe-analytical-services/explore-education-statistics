import DataSetFilePageNav from '@frontend/modules/data-catalogue/components/DataSetFilePageNav';
import { render, screen } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import { Dictionary } from '@common/types';

let mockIsMedia = false;

jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

const testSections: Dictionary<string> = {
  section1: 'Section 1',
  section2: 'Section 2',
};

describe('DataSetFilePageNav', () => {
  test('renders the navigation', () => {
    render(
      <DataSetFilePageNav
        activeSection="details"
        sections={testSections}
        onClickItem={noop}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'On this page' }),
    ).toBeInTheDocument();

    expect(screen.getByRole('link', { name: 'Section 1' })).toHaveAttribute(
      'href',
      '#section1',
    );
    expect(screen.getByRole('link', { name: 'Section 2' })).toHaveAttribute(
      'href',
      '#section2',
    );
  });

  test('renders the back to top link on desktop', () => {
    render(
      <DataSetFilePageNav
        activeSection="details"
        sections={testSections}
        onClickItem={noop}
      />,
    );

    expect(screen.getByRole('link', { name: 'Back to top' })).toHaveAttribute(
      'href',
      '#main-content',
    );
  });

  test('does not render the back to top link on mobile', () => {
    mockIsMedia = true;

    render(
      <DataSetFilePageNav
        activeSection="details"
        sections={testSections}
        onClickItem={noop}
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'Back to top' }),
    ).not.toBeInTheDocument();
  });
});
