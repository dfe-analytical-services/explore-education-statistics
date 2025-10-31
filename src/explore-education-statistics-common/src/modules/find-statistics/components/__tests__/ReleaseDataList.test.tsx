import render from '@common-test/render';
import ReleaseDataList from '@common/modules/find-statistics/components/ReleaseDataList';
import { screen } from '@testing-library/react';
import React from 'react';

describe('ReleaseDataList', () => {
  test('renders', () => {
    render(
      <ReleaseDataList heading="Test heading">
        <li>List item</li>
      </ReleaseDataList>,
    );

    expect(
      screen.getByRole('heading', { name: 'Test heading' }),
    ).toBeInTheDocument();

    expect(screen.getByRole('list')).toBeInTheDocument();
  });

  test('renders action and toggle if provided', () => {
    render(
      <ReleaseDataList
        heading="Test heading"
        actions={<a href="test-url">Action link</a>}
        toggle={<button type="button">Test toggle</button>}
      >
        <li>List item</li>
      </ReleaseDataList>,
    );

    expect(screen.getByRole('link', { name: 'Action link' })).toHaveAttribute(
      'href',
      'test-url',
    );
    expect(
      screen.getByRole('button', { name: 'Test toggle' }),
    ).toBeInTheDocument();
  });
});
