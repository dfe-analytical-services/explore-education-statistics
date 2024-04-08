import DataSetFilePageNav from '@frontend/modules/data-catalogue/components/DataSetFilePageNav';
import { render, screen } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

describe('DataSetFilePageNav', () => {
  test('renders the navigation', () => {
    render(<DataSetFilePageNav activeSection="details" onClickItem={noop} />);

    expect(
      screen.getByRole('heading', { name: 'On this page' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Data set details' }),
    ).toHaveAttribute('href', '#details');
    expect(
      screen.getByRole('link', { name: 'Using this data' }),
    ).toHaveAttribute('href', '#using');
  });

  test('renders the back to top link on desktop', () => {
    render(<DataSetFilePageNav activeSection="details" onClickItem={noop} />);

    expect(screen.getByRole('link', { name: 'Back to top' })).toHaveAttribute(
      'href',
      '#main-content',
    );
  });

  test('does not render the back to top link on mobile', () => {
    mockIsMedia = true;
    render(<DataSetFilePageNav activeSection="details" onClickItem={noop} />);

    expect(
      screen.queryByRole('link', { name: 'Back to top' }),
    ).not.toBeInTheDocument();
  });
});
