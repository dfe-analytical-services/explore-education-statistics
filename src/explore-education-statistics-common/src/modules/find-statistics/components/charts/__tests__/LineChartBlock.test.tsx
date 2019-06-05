import React from 'react';

import { render } from 'react-testing-library';
import domUtil from '@common-test/domUtil';
import LineChartBlock from '../LineChartBlock';

import testData from './__data__/testBlockData';

describe('LineChartBlock', () => {
  test('renders with correct output', () => {
    const { container } = render(
      <LineChartBlock {...testData.AbstractChartProps} />,
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
      domUtil.elementContainingText(container, 'text tspan', '2014/15').length,
    ).toBe(1);

    expect(
      domUtil.elementContainingText(container, 'text tspan', '2015/16').length,
    ).toBe(1);

    expect(
      Array.from(container.querySelectorAll('.recharts-line')).length,
    ).toBe(3);

    expect(container).toMatchSnapshot();
  });
});
