import React from 'react';

import { render } from 'react-testing-library';
import domUtil from '@common-test/domUtil';
import VerticalBarBlock from '../VerticalBarBlock';

import testData from './__data__/testBlockData';

describe('VerticalBarBlock', () => {
  test('renders with correct output', () => {
    const { container } = render(
      <VerticalBarBlock {...testData.AbstractChartProps} />,
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

    expect(Array.from(container.querySelectorAll('.recharts-bar')).length).toBe(
      3,
    );

    expect(container).toMatchSnapshot();
  });
});
