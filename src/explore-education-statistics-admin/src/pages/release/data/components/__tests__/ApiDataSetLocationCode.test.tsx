import ApiDataSetLocationCode from '@admin/pages/release/data/components/ApiDataSetLocationCode';
import render from '@common-test/render';
import React from 'react';

describe('ApiDataSetLocationCode', () => {
  test('renders correctly with location code', () => {
    const { container } = render(
      <ApiDataSetLocationCode
        location={{
          label: 'Location 1',
          urn: 'urn-1',
          ukprn: 'ukprn-1',
        }}
      />,
    );

    expect(container).toHaveTextContent('URN: urn-1, UKPRN: ukprn-1');
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
