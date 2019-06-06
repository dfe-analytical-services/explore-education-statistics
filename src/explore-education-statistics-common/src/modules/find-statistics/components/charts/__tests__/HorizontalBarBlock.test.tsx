import React from 'react';

import { render } from 'react-testing-library';

import domUtil from '@common-test/domUtil';
import HorzontalBarBlock from '../HorizontalBarBlock';

import testData from './__data__/testBlockData';

describe('HorzontalBarBlock', () => {
  test('renders with correct output', () => {
    const { container } = render(
      <HorzontalBarBlock
        {...testData.AbstractChartProps}
        stacked
        height={600}
        width={900}
      />,
    );

    expect(
      domUtil.elementContainingText(
        container,
        '.xAxis text tspan',
        'test x axis',
      ).length,
    ).toBe(1);

    expect(
      domUtil.elementContainingText(
        container,
        '.yAxis text tspan',
        'test y axis',
      ).length,
    ).toBe(1);

    expect(
      domUtil.elementContainingText(container, 'text tspan', '2014').length,
    ).toBe(1);

    expect(
      domUtil.elementContainingText(container, 'text tspan', '2015').length,
    ).toBe(1);

    expect(container).toMatchSnapshot();
  });
});
