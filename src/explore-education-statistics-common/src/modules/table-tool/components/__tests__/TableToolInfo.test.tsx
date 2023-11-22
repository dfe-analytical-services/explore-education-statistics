import React from 'react';
import { render, screen } from '@testing-library/react';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { ReleaseType } from '@common/services/types/releaseType';

describe('TableToolInfo', () => {
  test('renders', () => {
    render(<TableToolInfo />);

    expect(
      screen.getByRole('heading', { name: 'Related information' }),
    ).toBeInTheDocument();
  });

  test('displays Release Type and helper modal if one is provided', () => {
    const testReleaseType: ReleaseType = 'OfficialStatistics';

    render(<TableToolInfo releaseType={testReleaseType} />);

    expect(
      screen.getByText('Release type: Official statistics'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'What are release types?' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: /Office for Statistics Regulation/ }),
    ).toBeInTheDocument();
  });

  test('does not display the helper modal if no Release Type is given, but still displays the regulation text', () => {
    render(<TableToolInfo releaseType={undefined} />);

    expect(
      screen.queryByRole('button', { name: 'What are release types?' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: /Office for Statistics Regulation/ }),
    ).toBeInTheDocument();
  });
});
