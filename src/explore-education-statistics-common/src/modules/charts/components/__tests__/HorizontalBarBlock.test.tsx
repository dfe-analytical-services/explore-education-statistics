import {
  testChartConfiguration,
  testChartTableData,
} from '@common/modules/charts/components/__tests__/__data__/testChartData';
import { expectTicks } from '@common/modules/charts/components/__tests__/testUtils';
import HorizontalBarBlock, {
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import { VerticalBarProps } from '@common/modules/charts/components/VerticalBarBlock';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';

jest.mock('recharts/lib/util/LogUtils');

describe('HorizontalBarBlock', () => {
  const fullTable = mapFullTable(testChartTableData);
  const props: VerticalBarProps = {
    ...testChartConfiguration,
    legend: testChartConfiguration.legend as LegendConfiguration,
    axes: testChartConfiguration.axes as VerticalBarProps['axes'],
    meta: fullTable.subjectMeta,
    data: fullTable.results,
    dataLabelPosition: 'inside',
  };

  const { axes } = props;

  test('renders basic chart correctly', async () => {
    const { container } = render(<HorizontalBarBlock {...props} />);

    await waitFor(() => {
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

      expect(container.querySelectorAll('.recharts-rectangle')).toHaveLength(
        15,
      );
    });
  });

  test('major axis can be hidden', () => {
    const { container } = render(
      <HorizontalBarBlock
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
      container.querySelector('.recharts-cartesian-axis.yAxis'),
    ).not.toBeInTheDocument();
  });

  test('minor axis can be hidden', () => {
    const { container } = render(
      <HorizontalBarBlock
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
      container.querySelector('.recharts-cartesian-axis.xAxis'),
    ).not.toBeInTheDocument();
  });

  test('both axes can be hidden', () => {
    const { container } = render(
      <HorizontalBarBlock
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
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        legend={{
          ...props.legend,
          position: 'none',
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-default-legend'),
    ).not.toBeInTheDocument();
  });

  test('can stack data', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          ...props.axes,
          minor: {
            ...props.axes.minor,
            min: -10,
            max: 20,
          },
        }}
        stacked
      />,
    );

    expect(
      Array.from(container.querySelectorAll('.recharts-rectangle')).length,
    ).toBe(15);
  });

  test('can render major axis reference line', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          ...props.axes,
          major: {
            ...props.axes.major,
            referenceLines: [
              {
                label: 'hello',
                position: '2014_AY',
              },
            ],
          },
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-reference-line'),
    ).toHaveTextContent('hello');
  });
  test('can render minor axis reference line', () => {
    const { container } = render(
      <HorizontalBarBlock
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
      />,
    );

    expect(
      container.querySelector('.recharts-reference-line'),
    ).toHaveTextContent('hello');
  });

  test('dies gracefully with bad data', () => {
    const invalidData = undefined as unknown as TableDataResponse['results'];
    const invalidMeta = undefined as unknown as FullTableMeta;
    const invalidAxes = undefined as unknown as HorizontalBarProps['axes'];
    const invalidLegend = undefined as unknown as LegendConfiguration;

    const { container } = render(
      <HorizontalBarBlock
        alt=""
        title=""
        height={300}
        data={invalidData}
        meta={invalidMeta}
        axes={invalidAxes}
        legend={invalidLegend}
      />,
    );
    expect(container).toHaveTextContent('Unable to render chart');
  });

  test('can change width of chart', () => {
    const { container } = render(<HorizontalBarBlock {...props} width={200} />);

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
    const { container } = render(
      <HorizontalBarBlock {...props} height={200} />,
    );

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
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          major: props.axes.major,
          minor: {
            ...props.axes.minor,
            tickConfig: 'default',
          },
        }}
      />,
    );

    expectTicks(container, 'x', '0', '2', '4', '6');
  });

  test('can limit range of minor ticks to start and end', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          major: props.axes.major,
          minor: {
            ...props.axes.minor,
            tickConfig: 'startEnd',
          },
        }}
      />,
    );

    expectTicks(container, 'x', '0', '6');
  });

  test('can limit range of minor ticks to custom', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          major: props.axes.major,
          minor: {
            ...props.axes.minor,
            tickConfig: 'custom',
            tickSpacing: 1,
          },
        }}
      />,
    );

    expectTicks(container, 'x', '0', '1', '2', '3', '4', '5', '6');
  });

  test('can limit range of major ticks to default', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          minor: props.axes.minor,
          major: {
            ...props.axes.major,
            tickConfig: 'default',
          },
        }}
      />,
    );

    expectTicks(
      container,
      'y',
      '2012/13',
      '2013/14',
      '2014/15',
      '2015/16',
      '2016/17',
    );
  });

  test('can limit range of major ticks to start and end', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          minor: props.axes.minor,
          major: {
            ...props.axes.major,
            tickConfig: 'startEnd',
          },
        }}
      />,
    );

    expectTicks(container, 'y', '2012/13', '2016/17');
  });

  test('can limit range of major ticks to custom', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          minor: props.axes.minor,
          major: {
            ...props.axes.major,
            tickConfig: 'custom',
            tickSpacing: 2,
          },
        }}
      />,
    );

    expectTicks(container, 'y', '2012/13', '2014/15', '2016/17');
  });

  test('can sort by name', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          minor: props.axes.minor,
          major: {
            ...props.axes.major,
            sortBy: 'name',
            sortAsc: true,
          },
        }}
      />,
    );

    expectTicks(
      container,
      'y',
      '2012/13',
      '2013/14',
      '2014/15',
      '2015/16',
      '2016/17',
    );
  });

  test('can sort by name descending', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          minor: props.axes.minor,
          major: {
            ...props.axes.major,
            sortBy: 'name',
            sortAsc: false,
          },
        }}
      />,
    );

    expectTicks(
      container,
      'y',
      '2016/17',
      '2015/16',
      '2014/15',
      '2013/14',
      '2012/13',
    );
  });

  test('can filter a data range', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          minor: props.axes.minor,
          major: {
            ...props.axes.major,
            sortBy: 'name',
            sortAsc: true,
            min: 0,
            max: 1,
          },
        }}
      />,
    );

    expectTicks(container, 'y', '2012/13', '2013/14');
  });

  test('can add axis labels', () => {
    render(
      <HorizontalBarBlock
        {...props}
        axes={{
          ...axes,
          major: {
            ...axes.major,
            label: {
              text: 'Test axis label 1',
            },
          },
          minor: {
            ...axes.minor,
            label: {
              text: 'Test axis label 2',
            },
          },
        }}
      />,
    );

    expect(screen.getByTestId('x-axis-label')).toHaveTextContent(
      'Test axis label 2',
    );
    expect(screen.getByTestId('y-axis-label')).toHaveTextContent(
      'Test axis label 1',
    );
  });

  test('can add reference lines', () => {
    const { container } = render(
      <HorizontalBarBlock
        {...props}
        axes={{
          ...axes,
          major: {
            ...axes.major,
            referenceLines: [{ label: 'Test label 1', position: '2015_AY' }],
          },
          minor: {
            ...axes.minor,
            referenceLines: [{ label: 'Test label 2', position: 3.4 }],
          },
        }}
      />,
    );

    const referenceLines = container.querySelectorAll<HTMLElement>(
      '.recharts-reference-line',
    );

    expect(referenceLines).toHaveLength(2);
    expect(
      within(referenceLines[0]).getByText('Test label 1'),
    ).toBeInTheDocument();
    expect(
      within(referenceLines[1]).getByText('Test label 2'),
    ).toBeInTheDocument();
  });
});
