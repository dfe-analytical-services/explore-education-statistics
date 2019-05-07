import React from 'react';
import { render, wait } from 'react-testing-library';
import LineChartBlock from '../LineChartBlock';

import testData from './testBlockData';

const elementContainingText = (
  container: HTMLElement,
  selector: string,
  text: string,
) =>
  Array.from(container.querySelectorAll(selector)).filter(
    element => element.textContent && element.textContent.trim() === text,
  );

describe('LineChartBlock', () => {
  test('renders', async () => {
    const { container } = render(
      <LineChartBlock {...testData.AbstractChartProps} />,
    );

    expect(
      elementContainingText(container, 'text tspan', 'test x axis').length,
    ).toBe(1);

    expect(
      elementContainingText(container, 'text tspan', 'test y axis').length,
    ).toBe(1);

    expect(elementContainingText(container, 'text tspan', '2014').length).toBe(
      1,
    );

    expect(elementContainingText(container, 'text tspan', '2015').length).toBe(
      1,
    );

    expect(container).toMatchSnapshot();
  });
});
