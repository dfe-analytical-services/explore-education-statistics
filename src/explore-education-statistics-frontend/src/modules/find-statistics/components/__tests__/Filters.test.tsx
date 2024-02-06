import Filters from '@frontend/modules/find-statistics/components/Filters';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('Filters', () => {
  test('renders the filters', () => {
    render(<Filters themes={testThemeSummaries} onChange={noop} />);

    expect(
      screen.getByRole('group', { name: 'Filter by theme' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('group', { name: 'Filter by release type' }),
    ).toBeInTheDocument();
  });
});
