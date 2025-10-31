import render from '@common-test/render';
import ReleaseDataListItem from '@common/modules/find-statistics/components/ReleaseDataListItem';
import { screen } from '@testing-library/react';
import React from 'react';

describe('ReleaseDataListItem', () => {
  test('renders', () => {
    render(<ReleaseDataListItem title="Test title" />);

    expect(
      screen.getByRole('heading', { name: 'Test title' }),
    ).toBeInTheDocument();

    expect(screen.getByRole('listitem')).toBeInTheDocument();
  });

  test('renders optional props if provided', () => {
    render(
      <ReleaseDataListItem
        title="Test title"
        actions={
          <>
            <a href="test-url-1">Action link one</a>
            <a href="test-url-2">Action link two</a>
          </>
        }
        description="Test description"
        metaInfo="Test meta info"
      >
        <p>Test content</p>
      </ReleaseDataListItem>,
    );

    expect(
      screen.getByRole('link', { name: 'Action link one' }),
    ).toHaveAttribute('href', 'test-url-1');
    expect(
      screen.getByRole('link', { name: 'Action link two' }),
    ).toHaveAttribute('href', 'test-url-2');
    expect(screen.getByText('Test description')).toBeInTheDocument();
    expect(screen.getByText('Test meta info')).toBeInTheDocument();
    expect(screen.getByText('Test content')).toBeInTheDocument();
  });
});
