import ApiDataSetLocationCode from '@admin/pages/release/data/components/ApiDataSetLocationCode';
import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetLocationCode', () => {
  test('renders correctly with location code', () => {
    render(
      <ApiDataSetLocationCode
        location={{
          label: 'Location 1',
          urn: 'urn-1',
          ukprn: 'ukprn-1',
        }}
      />,
    );

    expect(screen.getByText('URN: urn-1, UKPRN: ukprn-1')).toBeInTheDocument();
  });

  test('does not render without location code', () => {
    const { container } = render(
      <ApiDataSetLocationCode
        location={{
          label: 'Location 1',
        }}
      />,
    );

    expect(container).toBeEmptyDOMElement();
  });
});
