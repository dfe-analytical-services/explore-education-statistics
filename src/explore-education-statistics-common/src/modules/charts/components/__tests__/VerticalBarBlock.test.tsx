import testData from '@common/modules/charts/components/__tests__/__data__/testBlockData';
import { expectTicks } from '@common/modules/charts/components/__tests__/testUtils';
import VerticalBarBlock, {
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import { ChartMetaData } from '@common/modules/charts/types/chart';
import { DataBlockData } from '@common/services/dataBlockService';
import { render } from '@testing-library/react';
import React from 'react';

jest.mock('recharts/lib/util/LogUtils');

const props = {
  ...testData.chartProps1,
  height: 900,
} as VerticalBarProps;

const { axes } = props;

describe('VerticalBarBlock', () => {
  test('renders basic chart correctly', () => {
    const { container } = render(<VerticalBarBlock {...props} />);

    // axes
    expect(
      container.querySelector('.recharts-cartesian-axis.xAxis'),
    ).toBeInTheDocument();
    expect(
      container.querySelector('.recharts-cartesian-axis.yAxis'),
    ).toBeInTheDocument();

    // grid & grid lines
    expect(
      container.querySelector('.recharts-cartesian-grid'),
    ).toBeInTheDocument();
    expect(
      container.querySelector('.recharts-cartesian-grid-horizontal'),
    ).toBeInTheDocument();
    expect(
      container.querySelector('.recharts-cartesian-grid-vertical'),
    ).toBeInTheDocument();

    // Legend
    expect(
      container.querySelector('.recharts-default-legend'),
    ).toBeInTheDocument();

    const legendItems = container.querySelectorAll('.recharts-legend-item');

    expect(legendItems[0]).toHaveTextContent('Unauthorised absence rate');
    expect(legendItems[1]).toHaveTextContent('Overall absence rate');
    expect(legendItems[2]).toHaveTextContent('Authorised absence rate');

    // expect there to be rectangles for all 3 data sets across both years
    expect(container.querySelectorAll('.recharts-rectangle')).toHaveLength(6);
  });

  test('major axis can be hidden', () => {
    const { container } = render(
      <VerticalBarBlock
        {...props}
        axes={{
          ...axes,
          major: {
            ...axes.major,
            visible: false,
          },
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-cartesian-axis.xAxis'),
    ).not.toBeInTheDocument();
  });

  test('minor axis can be hidden', () => {
    const { container } = render(
      <VerticalBarBlock
        {...props}
        axes={{
          ...axes,
          minor: {
            ...axes.minor,
            visible: false,
          },
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-cartesian-axis.yAxis'),
    ).not.toBeInTheDocument();
  });

  test('both axes can be hidden', () => {
    const { container } = render(
      <VerticalBarBlock
        {...props}
        axes={{
          ...axes,
          minor: {
            ...axes.minor,
            visible: false,
          },
          major: {
            ...axes.major,
            visible: false,
          },
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-cartesian-axis.yAxis'),
    ).not.toBeInTheDocument();

    expect(
      container.querySelector('.recharts-cartesian-axis.xAxis'),
    ).not.toBeInTheDocument();
  });

  test('can hide legend', () => {
    const { container } = render(<VerticalBarBlock {...props} legend="none" />);

    expect(
      container.querySelector('.recharts-default-legend'),
    ).not.toBeInTheDocument();
  });

  test('can stack data', () => {
    const { container } = render(
      <VerticalBarBlock
        {...{
          ...props,
          axes: {
            ...props.axes,
            minor: {
              ...props.axes.minor,
              min: -10,
              max: 20,
            },
          },
        }}
        stacked
        legend="none"
      />,
    );

    // Unsure how to tell stacked data apart, other than the snapshot

    expect(
      Array.from(container.querySelectorAll('.recharts-rectangle')).length,
    ).toBe(6);
  });

  test('can render major axis reference line', () => {
    const { container } = render(
      <VerticalBarBlock
        {...{
          ...props,
          axes: {
            ...props.axes,
            major: {
              ...props.axes.major,
              referenceLines: [
                {
                  label: 'hello',
                  position: '2014/15',
                },
              ],
            },
          },
        }}
        legend="none"
      />,
    );

    expect(
      container.querySelector('.recharts-reference-line'),
    ).toBeInTheDocument();
  });
  test('can render minor axis reference line', () => {
    const { container } = render(
      <VerticalBarBlock
        {...props}
        axes={{
          ...props.axes,
          minor: {
            ...props.axes.minor,
            referenceLines: [
              {
                label: 'hello',
                position: 0,
              },
            ],
          },
        }}
        legend="none"
      />,
    );

    expect(
      container.querySelector('.recharts-reference-line'),
    ).toBeInTheDocument();
  });

  test('dies gracefully with bad data', () => {
    const invalidData = (undefined as unknown) as DataBlockData;
    const invalidMeta = (undefined as unknown) as ChartMetaData;
    const invalidAxes = (undefined as unknown) as VerticalBarProps['axes'];

    const { container } = render(
      <VerticalBarBlock
        height={300}
        data={invalidData}
        labels={{}}
        meta={invalidMeta}
        axes={invalidAxes}
      />,
    );
    expect(container).toHaveTextContent('Unable to render chart');
  });

  test('can change width of chart', () => {
    const propsWithSize = {
      ...props,
      width: 200,
    };

    const { container } = render(<VerticalBarBlock {...propsWithSize} />);

    const responsiveContainer = container.querySelector(
      '.recharts-responsive-container',
    );

    expect(responsiveContainer).toHaveProperty('style');

    if (responsiveContainer) {
      const div = responsiveContainer as HTMLElement;
      expect(div.style.width).toEqual('200px');
    }
  });

  test('can change height of chart', () => {
    const propsWithSize = {
      ...props,
      height: 200,
    };

    const { container } = render(<VerticalBarBlock {...propsWithSize} />);

    const responsiveContainer = container.querySelector(
      '.recharts-responsive-container',
    );

    expect(responsiveContainer).toHaveProperty('style');

    if (responsiveContainer) {
      const div = responsiveContainer as HTMLElement;
      expect(div.style.height).toEqual('200px');
    }
  });

  test('can limit range of minor ticks to default', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        major: props.axes.major,
        minor: {
          ...props.axes.minor,
          tickConfig: 'default',
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'y', '-3', '3', '9', '20');
  });

  test('can limit range of minor ticks to start and end', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        major: props.axes.major,

        minor: {
          ...props.axes.minor,
          tickConfig: 'startEnd',
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'y', '-3', '20');
  });

  test('can limit range of minor ticks to custom', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        major: props.axes.major,
        minor: {
          ...props.axes.minor,
          tickConfig: 'custom',
          tickSpacing: 1,
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(
      container,
      'y',
      '-3',
      '-2',
      '-1',
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      '10',
    );
  });

  test('can limit range of major ticks to default', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        minor: props.axes.minor,
        major: {
          ...props.axes.major,
          tickConfig: 'default',
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'x', '2014/15', '2015/16');
  });

  test('can limit range of major ticks to start and end', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        minor: props.axes.minor,
        major: {
          ...props.axes.major,
          tickConfig: 'startEnd',
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'x', '2014/15', '2015/16');
  });

  test('can limit range of minor ticks to custom', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        minor: props.axes.minor,
        major: {
          ...props.axes.major,
          tickConfig: 'custom',
          tickSpacing: 2,
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'x', '2014/15', '2015/16');
  });

  test('can sort by name', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        minor: props.axes.minor,
        major: {
          ...props.axes.major,
          sortBy: 'name',
          sortAsc: true,
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'x', '2014/15', '2015/16');
  });

  test('can sort by name descending', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        minor: props.axes.minor,
        major: {
          ...props.axes.major,
          sortBy: 'name',
          sortAsc: false,
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'x', '2015/16', '2014/15');
  });

  test('can filter a data range', () => {
    const propsWithTicks: VerticalBarProps = {
      ...props,
      axes: {
        minor: props.axes.minor,
        major: {
          ...props.axes.major,
          sortBy: 'name',
          sortAsc: true,
          min: 0,
          max: 1,
        },
      },
    };

    const { container } = render(<VerticalBarBlock {...propsWithTicks} />);

    expectTicks(container, 'x', '2014/15');
  });
});
