import React from 'react';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';

interface Props {
  chartType?: string;
}

const ExampleCharts = ({ chartType }: Props) => {
  return (
    <>
      {chartType === 'barChart' && (
        <Tabs id="barChartExample">
          <TabsSection title="Chart">
            <figure
              className="govuk-!-margin-0"
              id="dataBlock-23831949-ec60-480e-a076-28b721b48747-chart"
              data-testid="dataBlock-23831949-ec60-480e-a076-28b721b48747-chart"
            >
              <figcaption className="govuk-heading-s">
                Apprenticeship starts sex summary - click table view for
                participation and achievements test
              </figcaption>
              <div className="govuk-!-margin-bottom-6">
                <div className="dfe-flex dfe-align-items--center">
                  <div style={{ width: '100%' }}>
                    <div
                      className="recharts-responsive-container"
                      style={{
                        width: '100%',
                        height: '300px',
                        minWidth: '0px',
                      }}
                    >
                      <div
                        className="recharts-wrapper"
                        role="region"
                        style={{
                          position: 'relative',
                          cursor: 'default',
                          width: '100%',
                          height: '100%',
                        }}
                      >
                        <svg
                          aria-label="The chart shows Apprenticeship starts sex split male and female from 2017/18 to 2021/22"
                          role="img"
                          focusable="false"
                          className="recharts-surface"
                          width="100%"
                          height="100%"
                          viewBox="0 0 918 300"
                        >
                          <title />
                          <desc />

                          <g className="recharts-cartesian-grid" />
                          <g className="recharts-layer recharts-cartesian-axis recharts-yAxis yAxis">
                            <line
                              width="72"
                              orientation="left"
                              height="230"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-line"
                              stroke="#666"
                              fill="none"
                              x1="102"
                              y1="20"
                              x2="102"
                              y2="250"
                            />
                            <g className="recharts-cartesian-axis-ticks">
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="30"
                                  y="20"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="96"
                                  y1="250"
                                  x2="102"
                                  y2="250"
                                />
                                <text
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="94"
                                  y="250"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="end"
                                >
                                  <tspan x="94" dy="0.355em">
                                    0
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="30"
                                  y="20"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="96"
                                  y1="192.5"
                                  x2="102"
                                  y2="192.5"
                                />
                                <text
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="94"
                                  y="192.5"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="end"
                                >
                                  <tspan x="94" dy="0.355em">
                                    100,000
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="30"
                                  y="20"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="96"
                                  y1="135"
                                  x2="102"
                                  y2="135"
                                />
                                <text
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="94"
                                  y="135"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="end"
                                >
                                  <tspan x="94" dy="0.355em">
                                    200,000
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="30"
                                  y="20"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="96"
                                  y1="77.5"
                                  x2="102"
                                  y2="77.5"
                                />
                                <text
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="94"
                                  y="77.5"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="end"
                                >
                                  <tspan x="94" dy="0.355em">
                                    300,000
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="30"
                                  y="20"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="96"
                                  y1="20"
                                  x2="102"
                                  y2="20"
                                />
                                <text
                                  width="72"
                                  orientation="left"
                                  height="230"
                                  x="94"
                                  y="20"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="end"
                                >
                                  <tspan x="94" dy="0.355em">
                                    400,000
                                  </tspan>
                                </text>
                              </g>
                            </g>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis recharts-xAxis xAxis">
                            <line
                              height="50"
                              orientation="bottom"
                              width="816"
                              x="102"
                              y="250"
                              className="recharts-cartesian-axis-line"
                              stroke="#666"
                              fill="none"
                              x1="102"
                              y1="250"
                              x2="918"
                              y2="250"
                            />
                            <g className="recharts-cartesian-axis-ticks">
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="102"
                                  y="250"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="199.6"
                                  y1="256"
                                  x2="199.6"
                                  y2="250"
                                />
                                <text
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="199.6"
                                  y="266"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="middle"
                                >
                                  <tspan x="199.6" dy="0.71em">
                                    2017/18
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="102"
                                  y="250"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="354.79999999999995"
                                  y1="256"
                                  x2="354.79999999999995"
                                  y2="250"
                                />
                                <text
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="354.79999999999995"
                                  y="266"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="middle"
                                >
                                  <tspan x="354.79999999999995" dy="0.71em">
                                    2018/19
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="102"
                                  y="250"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="510"
                                  y1="256"
                                  x2="510"
                                  y2="250"
                                />
                                <text
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="510"
                                  y="266"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="middle"
                                >
                                  <tspan x="510" dy="0.71em">
                                    2019/20
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="102"
                                  y="250"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="665.1999999999999"
                                  y1="256"
                                  x2="665.1999999999999"
                                  y2="250"
                                />
                                <text
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="665.1999999999999"
                                  y="266"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="middle"
                                >
                                  <tspan x="665.1999999999999" dy="0.71em">
                                    2020/21
                                  </tspan>
                                </text>
                              </g>
                              <g className="recharts-layer recharts-cartesian-axis-tick">
                                <line
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="102"
                                  y="250"
                                  className="recharts-cartesian-axis-tick-line"
                                  stroke="#666"
                                  fill="none"
                                  x1="820.4"
                                  y1="256"
                                  x2="820.4"
                                  y2="250"
                                />
                                <text
                                  height="50"
                                  orientation="bottom"
                                  width="816"
                                  x="820.4"
                                  y="266"
                                  stroke="none"
                                  fill="#0b0c0c"
                                  className="recharts-text recharts-cartesian-axis-tick-value"
                                  textAnchor="middle"
                                >
                                  <tspan x="820.4" dy="0.71em">
                                    2021/22
                                  </tspan>
                                </text>
                              </g>
                            </g>
                          </g>
                          <g className="recharts-layer recharts-bar">
                            <g className="recharts-layer recharts-bar-rectangles">
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2017_AY"
                                  fill="#12436d"
                                  width="124"
                                  height="110.124"
                                  x="137.52"
                                  y="139.876"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 137.52,139.876 h 124 v 110.124 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2018_AY"
                                  fill="#12436d"
                                  width="124"
                                  height="112.85524999999998"
                                  x="292.71999999999997"
                                  y="137.14475000000002"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 292.71999999999997,137.14475000000002 h 124 v 112.85524999999998 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2019_AY"
                                  fill="#12436d"
                                  width="124"
                                  height="94.96700000000001"
                                  x="447.91999999999996"
                                  y="155.033"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 447.91999999999996,155.033 h 124 v 94.96700000000001 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2020_AY"
                                  fill="#12436d"
                                  width="124"
                                  height="86.043"
                                  x="603.1199999999999"
                                  y="163.957"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 603.1199999999999,163.957 h 124 v 86.043 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2021_AY"
                                  fill="#12436d"
                                  width="124"
                                  height="98.83099999999999"
                                  x="758.3199999999999"
                                  y="151.169"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 758.3199999999999,151.169 h 124 v 98.83099999999999 h -124 Z"
                                />
                              </g>
                            </g>
                            <g className="recharts-layer" />
                          </g>
                          <g className="recharts-layer recharts-bar">
                            <g className="recharts-layer recharts-bar-rectangles">
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2017_AY"
                                  fill="#f46a25"
                                  width="124"
                                  height="105.93800000000002"
                                  x="137.52"
                                  y="33.937999999999995"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 137.52,33.937999999999995 h 124 v 105.93800000000002 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2018_AY"
                                  fill="#f46a25"
                                  width="124"
                                  height="113.33825000000003"
                                  x="292.71999999999997"
                                  y="23.80649999999999"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 292.71999999999997,23.80649999999999 h 124 v 113.33825000000003 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2019_AY"
                                  fill="#f46a25"
                                  width="124"
                                  height="90.48774999999998"
                                  x="447.91999999999996"
                                  y="64.54525000000001"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 447.91999999999996,64.54525000000001 h 124 v 90.48774999999998 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2020_AY"
                                  fill="#f46a25"
                                  width="124"
                                  height="98.785"
                                  x="603.1199999999999"
                                  y="65.172"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 603.1199999999999,65.172 h 124 v 98.785 h -124 Z"
                                />
                              </g>
                              <g className="recharts-layer recharts-bar-rectangle">
                                <path
                                  name="2021_AY"
                                  fill="#f46a25"
                                  width="124"
                                  height="101.95325"
                                  x="758.3199999999999"
                                  y="49.215750000000014"
                                  radius="0"
                                  className="recharts-rectangle"
                                  d="M 758.3199999999999,49.215750000000014 h 124 v 101.95325 h -124 Z"
                                />
                              </g>
                            </g>
                            <g className="recharts-layer" />
                          </g>
                        </svg>
                        <div
                          className="recharts-legend-wrapper"
                          style={{
                            position: 'absolute',
                            width: 'auto',
                            height: 'auto',
                            left: '30px',
                            bottom: '0px',
                          }}
                        />
                        <div
                          role="dialog"
                          className="recharts-tooltip-wrapper recharts-tooltip-wrapper-right recharts-tooltip-wrapper-top"
                          style={{
                            pointerEvents: 'none',
                            visibility: 'hidden',
                            position: 'absolute',
                            top: '0px',
                            left: '0px',
                            zIndex: '1000',
                            transform: 'translate(209.6px, 92.9062px)',
                          }}
                        >
                          <div
                            className="CustomTooltip_tooltip__vB5AA"
                            data-testid="chartTooltip"
                          >
                            <p
                              className="govuk-!-font-weight-bold"
                              data-testid="chartTooltip-label"
                            >
                              2017/18
                            </p>
                            <ul
                              className="CustomTooltip_itemList___oA4a"
                              data-testid="chartTooltip-items"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div aria-hidden="true" className="govuk-!-margin-bottom-6">
                <ul
                  className="recharts-default-legend"
                  style={{ padding: '0px', margin: '0px', textAlign: 'left' }}
                >
                  <li
                    className="recharts-legend-item legend-item-0"
                    style={{ display: 'block', marginRight: '10px' }}
                  >
                    <svg
                      className="recharts-surface"
                      width="14"
                      height="14"
                      viewBox="0 0 32 32"
                      style={{
                        display: 'inline-block',
                        verticalAlign: 'middle',
                        marginRight: '4px',
                      }}
                    >
                      <title />
                      <desc />
                      <path
                        stroke="none"
                        fill="#12436d"
                        d="M0,4h32v24h-32z"
                        className="recharts-legend-icon"
                      />
                    </svg>
                    <span
                      className="recharts-legend-item-text"
                      style={{ color: 'rgb(18, 67, 109)' }}
                    >
                      Starts (Male)
                    </span>
                  </li>
                  <li
                    className="recharts-legend-item legend-item-1"
                    style={{ display: 'block', marginRight: '10px' }}
                  >
                    <svg
                      className="recharts-surface"
                      width="14"
                      height="14"
                      viewBox="0 0 32 32"
                      style={{
                        display: 'inline-block',
                        verticalAlign: 'middle',
                        marginRight: '4px',
                      }}
                    >
                      <title />
                      <desc />
                      <path
                        stroke="none"
                        fill="#f46a25"
                        d="M0,4h32v24h-32z"
                        className="recharts-legend-icon"
                      />
                    </svg>
                    <span
                      className="recharts-legend-item-text"
                      style={{ color: 'rgb(244, 106, 37)' }}
                    >
                      Starts (Female)
                    </span>
                  </li>
                </ul>
              </div>
              <h3 className="govuk-heading-m">
                Footnotes
                <span className="govuk-visually-hidden">
                  {' '}
                  for Apprenticeship starts sex summary - click table view for
                  participation and achievements
                </span>
              </h3>
              <ol
                id="chartFootnotes-dataBlock-23831949-ec60-480e-a076-28b721b48747-chart"
                className="govuk-list govuk-list--number"
                data-testid="footnotes"
              >
                <li>
                  <div className="dfe-content">
                    Figures for 2022/23 are provisional and cover the first
                    three quarters (Aug 2022 to Apr 2023). All other years are
                    final, full-year figures.
                  </div>
                </li>
                <li>
                  <div className="dfe-content">
                    Volumes are rounded to the nearest 10 and 'low' indicates a
                    base value of fewer than 5. Where data shows 'x' this
                    indicates data is unavailable, 'z' indicates data is not
                    applicable, and 'c' indicates data is suppressed.
                  </div>
                </li>
              </ol>

              <p className="govuk-body-s">
                Source: Individualised Learner Record (ILR)
              </p>
            </figure>
          </TabsSection>
          <TabsSection title="Table">
            <figure className="FixedMultiHeaderDataTable_figure__nHDwl govuk-!-margin-0">
              <figcaption>
                <strong
                  id="dataTableCaption-23831949-ec60-480e-a076-28b721b48747"
                  data-testid="dataTableCaption"
                >
                  Apprenticeship starts sex summary
                </strong>
              </figcaption>
              <div
                className="FixedMultiHeaderDataTable_container__7qrG_"
                role="region"
              >
                <table
                  data-testid="dataTableCaption-23831949-ec60-480e-a076-28b721b48747-table"
                  aria-labelledby="dataTableCaption-23831949-ec60-480e-a076-28b721b48747"
                  className="govuk-table MultiHeaderTable_table__BH58O"
                >
                  <thead className="MultiHeaderTable_tableHead__B087C">
                    <tr>
                      <td colSpan={2} rowSpan={1} />
                      <th colSpan={1} rowSpan={1} scope="col">
                        2017/18
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2018/19
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2019/20
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2020/21
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2021/22
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <th colSpan={1} rowSpan={4} scope="rowgroup" className="">
                        Starts
                      </th>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Total
                      </th>
                      <td className="govuk-table__cell--numeric">375,760</td>
                      <td className="govuk-table__cell--numeric">393,380</td>
                      <td className="govuk-table__cell--numeric">322,530</td>
                      <td className="govuk-table__cell--numeric">321,440</td>
                      <td className="govuk-table__cell--numeric">349,190</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Female
                      </th>
                      <td className="govuk-table__cell--numeric">184,240</td>
                      <td className="govuk-table__cell--numeric">197,110</td>
                      <td className="govuk-table__cell--numeric">157,370</td>
                      <td className="govuk-table__cell--numeric">171,800</td>
                      <td className="govuk-table__cell--numeric">177,310</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Male
                      </th>
                      <td className="govuk-table__cell--numeric">191,520</td>
                      <td className="govuk-table__cell--numeric">196,270</td>
                      <td className="govuk-table__cell--numeric">165,160</td>
                      <td className="govuk-table__cell--numeric">149,640</td>
                      <td className="govuk-table__cell--numeric">171,880</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Not App/ Known
                      </th>
                      <td className="govuk-table__cell--numeric">low</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={4} scope="rowgroup" className="">
                        Participation
                      </th>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Total
                      </th>
                      <td className="govuk-table__cell--numeric">814,790</td>
                      <td className="govuk-table__cell--numeric">742,390</td>
                      <td className="govuk-table__cell--numeric">718,950</td>
                      <td className="govuk-table__cell--numeric">712,990</td>
                      <td className="govuk-table__cell--numeric">740,350</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Female
                      </th>
                      <td className="govuk-table__cell--numeric">405,020</td>
                      <td className="govuk-table__cell--numeric">355,220</td>
                      <td className="govuk-table__cell--numeric">339,200</td>
                      <td className="govuk-table__cell--numeric">345,270</td>
                      <td className="govuk-table__cell--numeric">360,100</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Male
                      </th>
                      <td className="govuk-table__cell--numeric">409,770</td>
                      <td className="govuk-table__cell--numeric">387,170</td>
                      <td className="govuk-table__cell--numeric">379,760</td>
                      <td className="govuk-table__cell--numeric">367,720</td>
                      <td className="govuk-table__cell--numeric">380,260</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Not App/ Known
                      </th>
                      <td className="govuk-table__cell--numeric">low</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={4} scope="rowgroup" className="">
                        Achievements
                      </th>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Total
                      </th>
                      <td className="govuk-table__cell--numeric">276,160</td>
                      <td className="govuk-table__cell--numeric">185,150</td>
                      <td className="govuk-table__cell--numeric">146,900</td>
                      <td className="govuk-table__cell--numeric">156,530</td>
                      <td className="govuk-table__cell--numeric">137,220</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Female
                      </th>
                      <td className="govuk-table__cell--numeric">147,160</td>
                      <td className="govuk-table__cell--numeric">92,210</td>
                      <td className="govuk-table__cell--numeric">71,960</td>
                      <td className="govuk-table__cell--numeric">78,590</td>
                      <td className="govuk-table__cell--numeric">69,290</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Male
                      </th>
                      <td className="govuk-table__cell--numeric">129,000</td>
                      <td className="govuk-table__cell--numeric">92,940</td>
                      <td className="govuk-table__cell--numeric">74,940</td>
                      <td className="govuk-table__cell--numeric">77,940</td>
                      <td className="govuk-table__cell--numeric">67,930</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Not App/ Known
                      </th>
                      <td className="govuk-table__cell--numeric">low</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                      <td className="govuk-table__cell--numeric">no data</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div>
                <h3 className="govuk-heading-m">
                  Footnotes
                  <span className="govuk-visually-hidden">
                    {' '}
                    for Apprenticeship starts sex summary
                  </span>
                </h3>
                <ol
                  id="dataTableFootnotes-23831949-ec60-480e-a076-28b721b48747"
                  className="govuk-list govuk-list--number"
                  data-testid="footnotes"
                >
                  <li>
                    <div className="dfe-content">
                      Figures for 2022/23 are provisional and cover the first
                      three quarters (Aug 2022 to Apr 2023). All other years are
                      final, full-year figures.
                    </div>
                  </li>
                  <li>
                    <div className="dfe-content">
                      Volumes are rounded to the nearest 10 and 'low' indicates
                      a base value of fewer than 5. Where data shows 'x' this
                      indicates data is unavailable, 'z' indicates data is not
                      applicable, and 'c' indicates data is suppressed.
                    </div>
                  </li>
                </ol>
              </div>
              <p className="govuk-body-s">
                Source: Individualised Learner Record (ILR)
              </p>
            </figure>
          </TabsSection>
        </Tabs>
      )}
      {chartType === 'map' && (
        <Tabs id="mapExample">
          <TabsSection title="Chart">
            <figure
              className="govuk-!-margin-0"
              id="dataBlock-2904c403-643a-4877-9b5a-9adc7785fa19-chart"
              data-testid="dataBlock-2904c403-643a-4877-9b5a-9adc7785fa19-chart"
            >
              <h2 className="govuk-heading-s">
                Apprenticeship starts by region [volume and per 100k population]
              </h2>
              <form className="govuk-!-margin-bottom-2">
                <div className="govuk-grid-row">
                  <div className="govuk-form-group govuk-grid-column-two-thirds">
                    <label
                      className="govuk-label"
                      htmlFor="dataBlock-2904c403-643a-4877-9b5a-9adc7785fa19-chart-map-selectedDataSet"
                    >
                      1. Select data to view
                    </label>
                    <select
                      className="govuk-select govuk-!-width-full"
                      id="dataBlock-2904c403-643a-4877-9b5a-9adc7785fa19-chart-map-selectedDataSet"
                      name="selectedDataSet"
                    >
                      <option value='{"filters":["2ffab0a9-24bd-4ce3-ad3a-57961d125a29","b4b6a974-921b-4179-a521-aeb4816d3a10"],"indicator":"8387a129-c1a7-49b6-e652-08db81e22486","timePeriod":"2021_AY"}'>
                        Indicative starts rate per 100,000 population (2021/22)
                      </option>
                      <option value='{"filters":["2ffab0a9-24bd-4ce3-ad3a-57961d125a29","b4b6a974-921b-4179-a521-aeb4816d3a10"],"indicator":"fc20def6-066c-4350-e64e-08db81e22486","timePeriod":"2021_AY"}'>
                        Starts (2021/22)
                      </option>
                    </select>
                  </div>
                  <div className="govuk-form-group govuk-grid-column-one-third">
                    <label
                      className="govuk-label"
                      htmlFor="dataBlock-2904c403-643a-4877-9b5a-9adc7785fa19-chart-map-selectedLocation"
                    >
                      2. Select a Region
                    </label>
                    <select
                      className="govuk-select"
                      id="dataBlock-2904c403-643a-4877-9b5a-9adc7785fa19-chart-map-selectedLocation"
                      name="selectedLocation"
                    >
                      <option value="">None selected</option>
                      <option value='{"level":"region","value":"c37c1656-546a-43fc-8df5-cc8adb7e9d3f"}'>
                        East Midlands
                      </option>
                      <option value='{"level":"region","value":"b06cd079-d19b-402c-ad03-e3c4c1e0c86c"}'>
                        East of England
                      </option>
                      <option value='{"level":"region","value":"4c33ca58-a84e-472a-ab9d-56e67d992c94"}'>
                        London
                      </option>
                      <option value='{"level":"region","value":"6b0c3349-34c4-48d4-aa6e-745ab358acc1"}'>
                        North East
                      </option>
                      <option value='{"level":"region","value":"520fa9b3-ea9f-4cf4-b5a7-de9539900771"}'>
                        North West
                      </option>
                      <option value='{"level":"region","value":"b6d884e1-7b55-43cd-a549-99e48bb3e93c"}'>
                        South East
                      </option>
                      <option value='{"level":"region","value":"652bbef0-71ff-4171-a64a-79e3f6368bbc"}'>
                        South West
                      </option>
                      <option value='{"level":"region","value":"41a0ef0f-c361-471d-a156-5ff8a8e3f77d"}'>
                        West Midlands
                      </option>
                      <option value='{"level":"region","value":"9ea0d131-13fe-4ceb-308b-08d93cac8230"}'>
                        Yorkshire and The Humber
                      </option>
                    </select>
                  </div>
                </div>
              </form>
              <div className="govuk-grid-row govuk-!-margin-bottom-4">
                <div className="govuk-grid-column-two-thirds">
                  <div
                    className="MapBlock_map__gko1p dfe-print-break-avoid leaflet-container leaflet-touch leaflet-fade-anim leaflet-grab leaflet-touch-drag leaflet-touch-zoom"
                    style={{
                      width: '100%',
                      height: '600px',
                      position: 'relative',
                      outline: 'none',
                    }}
                  >
                    <div
                      className="leaflet-pane leaflet-map-pane"
                      style={{ transform: 'translate3d(0px, 0px, 0px)' }}
                    >
                      <div className="leaflet-pane leaflet-tile-pane" />
                      <div className="leaflet-pane leaflet-overlay-pane">
                        <svg
                          pointerEvents="none"
                          className="leaflet-zoom-animated"
                          width="722"
                          height="720"
                          viewBox="-60 -60 722 720"
                          style={{
                            transform: 'translate3d(-60px, -60px, 0px)',
                          }}
                        >
                          <g>
                            <path
                              className="MapBlock_uk__UjZWa leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="#3388ff"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M229 414L229 414zM301 260L299 257L301 260zM231 413L230 414L231 413zM230 412L230 412zM230 412L229 412zM328 386L328 386zM329 386L329 385zM231 411L231 411zM352 381L353 382L351 382L352 380zM349 381L349 381zM353 381L353 381zM268 369L268 367L268 369zM344 383L349 384L349 386L345 390L338 386L344 383zM266 386L266 386zM245 348L245 348zM249 348L249 348zM253 343L253 343zM396 346L394 346L397 346zM403 343L402 343zM403 342L402 342zM403 342L403 342zM403 342L403 342zM268 351L267 352L268 351zM393 353L393 353zM394 354L393 353L396 352L395 353zM392 348L392 348zM391 348L390 348zM254 347L253 347zM387 360L387 360zM395 359L396 361L394 362L391 361L390 359L394 359zM390 360L389 360zM390 359L389 359zM265 310L265 309zM394 302L394 302zM390 301L389 301zM274 285L275 284L277 286L276 287L278 287L278 289L282 289L275 296L273 294L274 293L273 295L269 290L267 290L267 288L270 290L270 286L273 284zM221 160L221 160zM221 160L221 160zM242 150L241 150zM221 161L216 165L215 164L216 161L221 161zM249 166L247 168L247 166L249 166zM232 162L234 164L232 164L232 162zM229 171L229 171zM249 168L249 168zM228 170L229 169zM235 156L239 162L245 165L245 167L244 166L241 170L241 168L239 170L231 172L229 170L235 169L237 167L233 167L238 163L237 162L235 164L232 161L230 161L230 159L233 159L232 158L234 156zM227 156L224 159L222 159L227 155zM231 163L231 163zM250 160L247 164L247 162L250 160zM200 151L201 151zM201 149L200 150L201 149zM204 144L204 144zM202 144L204 145L202 144zM203 147L204 146zM209 128L209 128zM209 127L209 127zM209 127L209 127zM209 133L209 133zM208 132L208 132zM208 132L208 132zM207 141L207 141zM204 141L206 141L205 144L202 143L205 140L204 139L205 140zM206 140L206 139zM209 138L208 139L209 138zM231 148L232 149L231 148zM235 145L234 147L233 146L235 145zM222 143L222 143zM222 143L222 143zM333 123L333 123zM332 124L332 124zM233 135L232 136L233 135zM234 133L234 133zM239 130L237 129L239 129zM327 138L327 138zM327 137L327 137zM226 139L227 140L226 139zM226 139L224 140L226 139zM230 139L232 141L231 144L227 142L229 140zM206 124L209 127L208 128L210 128L208 132L209 135L207 135L209 137L207 137L204 132L206 129L206 125zM238 121L237 128L236 122L237 123L238 121zM229 112L231 112L234 117L234 129L236 129L236 131L237 130L240 132L243 130L245 132L243 135L242 134L238 141L237 138L241 134L238 135L237 133L236 136L234 134L231 135L231 133L230 134L230 132L227 131L228 128L230 129L225 124L224 128L220 124L221 120L224 124L223 121L225 121L223 117L230 123L228 118L229 112zM208 125L208 125zM237 120L237 120zM210 125L210 125zM210 124L210 124zM237 122L237 122zM209 123L209 123zM209 123L209 123zM209 123L209 123zM209 123L209 123zM198 119L198 119zM311 113L311 113zM311 113L311 113zM211 114L211 114zM209 118L209 118zM209 118L209 118zM209 118L209 118zM199 119L199 119zM209 118L209 118zM209 118L209 118zM198 119L198 119zM210 120L210 120zM274 116L274 116zM199 119L199 120zM211 121L210 122L211 121zM211 121L211 121zM238 118L238 120L238 117zM209 122L210 123L209 122L210 123L208 123L210 123L208 125L205 122L209 122zM199 119L199 119zM199 119L199 119zM199 119L199 119zM199 119L199 119zM198 113L198 113zM198 113L198 113zM198 113L198 113zM199 113L199 113zM198 113L198 113zM208 114L210 112L210 114L213 115L210 116L212 118L208 118L212 118L211 120L209 120L209 122L205 118L207 118L202 116L208 112L207 114zM203 115L203 115zM203 115L203 115zM208 114L208 114zM206 114L206 114zM206 114L206 114zM227 214L234 214L237 217L236 220L238 221L238 224L245 231L239 237L241 238L243 236L247 236L250 243L250 247L245 254L241 253L242 252L240 254L240 258L237 261L231 258L229 258L229 260L224 261L222 259L223 256L219 254L219 252L217 251L218 249L215 246L213 246L210 250L211 253L208 258L208 256L208 258L202 258L198 254L196 254L188 245L192 241L195 242L199 238L195 238L194 235L202 233L207 221L214 221L215 215L220 216L227 213zM234 211L233 213L232 211zM257 192L256 191L257 192zM248 204L248 204zM333 196L332 196zM244 195L244 197L243 194L244 195zM255 195L257 196L258 201L257 206L254 205L251 199L254 194zM263 192L262 193L263 191zM259 189L260 194L255 187L258 187L259 189zM239 184L239 184zM239 184L239 184zM239 185L239 185zM239 185L239 185zM239 185L239 185zM247 179L247 179zM223 178L223 178zM223 178L223 178zM223 178L223 178zM247 174L246 175L247 174zM244 174L245 175L245 173L244 174zM246 175L246 172L246 175zM245 176L243 176L245 175zM234 178L232 182L231 181L234 178zM235 186L234 187L237 196L231 200L230 194L232 192L230 192L227 196L226 191L230 187L230 190L234 186zM245 185L244 185zM244 177L245 178L239 190L236 191L235 187L240 184L239 185L238 182L244 177zM246 173L246 172zM247 170L246 171L247 170zM304 58L304 58zM299 58L299 59zM303 63L304 64L303 63zM300 65L302 65L298 65L296 61L297 59L299 59L301 62L302 64L300 65zM271 55L271 55zM297 55L297 55zM297 55L298 55zM297 55L297 55zM297 55L297 55zM297 55L297 55zM297 55L297 55zM234 51L234 51zM234 52L234 52zM234 52L234 52zM234 51L234 51zM234 52L234 52zM234 52L234 52zM234 51L234 51zM234 51L234 51zM320 39L319 40L320 38zM308 40L309 39L308 40zM308 39L308 39zM307 41L306 42L309 44L308 46L309 44L306 44L304 41L307 41zM338 32L337 33L337 31L338 32zM314 49L316 51L315 52L313 52L314 49zM242 51L242 51zM241 51L241 51zM242 51L241 50zM319 43L313 47L315 44L314 43L319 43zM302 49L306 53L303 56L308 55L308 57L309 56L311 60L310 59L312 57L310 61L308 60L309 64L307 65L307 68L305 64L306 65L306 63L305 64L308 63L308 61L306 58L302 60L300 57L298 57L298 51L301 49zM310 53L310 55L307 54L308 52L310 52zM274 52L274 52zM274 52L274 52zM234 52L234 52zM234 52L234 51zM308 49L307 50L308 48zM305 48L306 47L307 50L303 48L305 48zM312 46L311 49L311 45L312 46zM351 5L351 4zM348 -2L348 -2zM348 2L348 2zM349 2L350 5L348 5L349 2zM327 4L327 4zM340 -2L340 -2zM338 -3L338 -3zM335 -5L335 -3L335 -5zM343 -4L342 -5L343 -4zM341 -5L341 -5zM340 -5L340 -3L340 -5zM356 -8L356 -8zM357 -9L356 -8zM357 -17L357 -17zM357 -17L357 -17zM354 -29L354 -29zM351 -22L351 -22zM354 -28L354 -26L356 -28L357 -27L355 -25L356 -24L354 -24L356 -24L354 -21L355 -20L352 -20L352 -25L354 -28zM353 -6L351 -4L351 -6L354 -7zM343 -6L342 -5L342 -7zM341 -6L341 -6zM340 -13L340 -13zM355 -18L356 -15L352 -17L354 -18zM352 -17L351 -17zM350 -23L352 -21L352 -18L350 -20L351 -17L349 -17L351 -16L351 -14L350 -11L348 -11L347 -18L350 -23zM345 12L345 12zM347 8L347 8zM347 6L347 6zM343 8L343 8zM343 8L343 8zM344 7L344 9L344 7zM344 6L343 9L344 6zM345 6L345 5zM327 5L327 4zM344 -18L343 -7L345 -9L344 -11L346 -12L346 -7L347 -8L348 -6L350 -10L348 -7L350 -7L350 -5L347 -5L349 -1L346 2L347 1L346 3L347 2L348 4L346 6L347 12L345 12L345 18L342 15L345 7L345 -1L343 2L342 -1L343 2L342 1L341 4L339 0L339 2L335 0L336 -3L344 -3L344 -5L341 -8L341 -12L337 -12L339 -15L339 -13L341 -12L342 -13L340 -13L342 -17L344 -18zM180 108L180 108zM179 108L179 108zM179 108L179 108zM179 108L179 108zM179 108L179 108zM179 108L179 108zM178 107L178 107zM178 107L178 107zM178 107L178 107zM178 107L178 107zM178 107L179 108L178 107zM178 107L178 107zM178 107L178 107zM179 107L179 107zM211 111L210 112L211 111zM199 112L199 112zM216 110L216 110zM210 109L209 110L210 109zM279 105L279 105zM222 105L223 106L222 105zM213 104L213 104zM215 103L213 105L215 103zM299 288L304 292L304 290L301 287L302 285L305 284L307 289L312 289L312 287L309 288L306 286L303 280L305 276L310 272L306 273L304 271L305 265L306 266L310 262L308 263L308 259L310 259L309 254L305 257L305 255L302 259L300 256L301 253L299 255L296 251L296 248L291 243L297 228L299 228L300 225L303 226L303 224L302 225L293 223L293 227L289 229L288 228L284 232L282 232L282 230L281 232L279 232L278 228L277 230L274 228L274 226L274 236L264 228L261 231L264 237L263 238L261 236L256 227L256 223L258 222L260 227L260 218L264 213L265 207L268 206L269 203L267 201L268 200L264 198L262 195L264 193L263 186L264 184L270 186L264 180L264 184L263 179L263 183L261 183L261 188L259 188L259 185L257 183L259 187L256 186L256 184L255 190L252 187L254 179L251 183L250 182L251 189L253 191L249 197L249 204L247 206L248 209L246 211L242 211L245 196L249 191L246 193L247 192L245 190L247 182L245 183L245 185L249 175L246 178L248 174L247 173L249 173L247 173L247 169L249 169L248 168L250 164L255 164L249 163L256 154L255 153L258 149L245 163L237 158L237 156L240 157L243 154L232 154L235 151L241 152L240 151L242 151L241 150L244 147L239 147L241 145L240 146L241 142L246 142L242 140L246 136L248 138L249 137L244 135L246 131L248 130L250 132L249 130L244 129L250 125L248 127L246 127L246 125L243 127L240 122L242 117L245 120L242 114L242 112L245 113L245 111L242 110L242 105L244 105L246 109L246 103L250 106L252 102L256 104L256 102L250 96L251 95L253 97L253 93L255 93L253 93L254 92L252 91L252 88L259 89L256 84L258 82L259 83L258 76L260 72L265 75L265 73L268 76L267 78L270 74L274 77L273 78L275 76L278 77L278 75L281 76L283 73L283 75L288 75L294 72L294 74L297 74L297 70L302 72L305 71L303 77L303 79L305 79L304 82L286 99L282 102L283 101L281 101L283 105L280 106L275 104L277 106L279 105L279 107L285 107L288 105L284 112L278 112L274 116L278 113L282 113L280 117L281 118L280 117L278 120L274 121L280 121L282 118L281 117L285 117L289 114L292 115L297 111L305 114L308 112L317 114L326 112L330 113L333 116L333 124L328 129L327 137L323 147L317 155L317 159L312 163L305 164L301 168L299 168L306 166L308 164L310 167L309 168L315 172L311 175L306 175L297 183L289 180L289 182L290 181L290 183L292 182L299 184L302 183L303 185L305 185L310 180L312 180L321 186L325 186L327 188L329 194L334 199L335 198L337 200L337 208L339 210L339 217L342 223L339 224L343 224L344 232L348 238L347 240L348 237L361 243L368 256L372 257L369 259L369 263L377 275L377 279L376 276L373 277L367 272L358 274L360 273L362 275L367 273L372 279L373 278L376 282L378 282L382 291L382 297L375 303L379 307L379 309L379 307L382 307L382 309L382 307L387 301L397 302L396 301L407 305L413 311L414 320L413 325L411 327L410 335L407 336L405 340L401 337L403 340L398 340L403 340L401 346L397 346L398 345L396 344L392 348L390 348L396 348L394 352L391 351L394 352L392 352L393 355L387 355L388 356L384 356L384 358L372 356L378 356L380 358L390 357L390 359L386 359L388 361L390 361L390 359L391 362L406 360L406 369L399 372L396 375L396 377L386 380L380 384L372 381L360 382L355 384L353 380L351 380L351 382L349 382L350 380L348 380L348 382L340 377L344 381L342 383L330 386L328 384L329 385L327 385L329 387L330 386L330 388L327 390L325 388L318 389L316 387L319 391L318 392L316 388L308 384L300 386L297 389L296 386L294 391L295 394L293 395L294 398L293 397L290 403L288 402L289 401L286 402L286 400L282 400L280 398L281 397L278 396L278 394L279 399L273 397L271 399L267 399L260 405L259 403L257 407L259 409L255 412L252 407L249 406L246 409L244 408L245 404L251 403L259 395L259 391L265 389L270 382L271 375L276 375L280 371L279 372L277 369L279 367L305 367L305 363L307 361L306 360L308 360L313 355L314 352L309 355L306 354L302 358L302 360L297 361L293 360L286 352L284 352L284 354L278 355L276 353L281 350L277 350L274 348L276 348L274 347L275 346L273 346L273 348L270 347L267 349L267 351L263 351L261 353L258 350L261 350L260 349L255 348L258 345L257 343L253 343L253 341L255 341L259 337L262 338L267 333L271 333L277 329L280 326L282 318L284 317L281 318L280 315L283 311L280 308L280 304L282 303L275 305L274 304L271 309L270 307L266 309L266 306L275 299L279 292L287 289L286 288L292 290L299 287zM178 106L177 107L178 106zM177 106L177 106zM180 105L180 105zM181 105L181 104zM181 105L181 105zM181 105L181 105zM181 105L181 105zM181 105L181 105zM212 94L212 94zM227 93L227 93zM217 93L217 93zM251 89L251 89zM218 88L220 90L218 90L217 88zM218 89L218 89zM201 88L201 88zM201 87L201 87zM201 88L201 88zM220 86L220 86zM201 88L201 88zM201 88L201 88zM201 87L201 87zM220 86L220 86zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM200 87L200 87zM202 87L202 87zM202 87L202 87zM202 87L202 87zM201 87L201 87zM201 87L201 87zM251 99L251 99zM221 99L221 99zM213 99L213 99zM212 98L211 99L212 98zM213 97L213 97zM228 95L228 95zM227 95L226 95zM222 97L222 97zM258 80L258 80zM256 83L256 83zM256 83L256 83zM258 77L258 77zM279 76L279 76zM280 75L280 75zM231 77L231 77zM272 75L272 75zM289 74L288 74zM272 75L272 75zM272 75L272 75zM271 74L271 74zM293 72L293 72zM200 87L200 87zM232 83L234 85L229 89L232 90L234 88L234 90L232 92L231 90L229 90L229 94L226 93L227 94L223 96L228 95L229 98L224 99L227 99L225 103L223 101L224 103L221 101L223 97L220 100L222 103L219 104L221 107L220 106L219 108L218 106L218 108L215 111L212 107L213 106L213 108L216 105L216 103L218 104L218 102L212 100L217 97L214 98L214 96L212 96L212 92L214 92L213 91L215 89L218 94L217 93L219 91L221 92L219 86L225 84L231 78L233 78L233 82zM200 87L200 87zM233 85L233 85zM201 87L201 87zM201 87L201 87zM201 87L201 87zM231 77L231 77z"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(32, 115, 188, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M329 192L329 194L334 199L335 198L338 203L337 208L339 210L339 217L342 223L339 224L343 224L344 232L348 238L347 240L348 237L353 239L356 241L354 243L346 242L344 244L341 243L341 245L335 241L333 243L332 242L332 244L330 245L327 244L328 243L326 244L321 238L321 231L319 229L317 231L315 229L315 222L317 222L317 220L313 217L313 214L325 204L323 203L321 198L324 197L324 194L326 194L327 191L328 192zM333 196L332 196z"
                              aria-describedby="leaflet-tooltip-109"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(77, 143, 201, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M316 219L317 222L315 222L315 229L317 231L319 229L321 231L321 238L325 242L325 244L322 245L322 247L320 248L321 252L316 256L318 261L320 260L320 262L327 267L325 274L331 280L328 285L329 292L325 294L322 298L321 297L320 300L316 303L314 301L309 300L309 298L306 295L306 292L301 287L302 285L305 284L307 289L312 289L312 287L309 288L306 286L303 280L305 276L310 272L306 273L304 271L305 265L306 266L310 262L308 263L308 259L310 259L309 254L305 257L305 255L302 259L300 256L301 253L299 255L296 251L296 248L291 243L297 228L299 228L300 225L303 226L305 221L307 221L312 216L315 219zM301 260L299 257L301 260z"
                              aria-describedby="leaflet-tooltip-108"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(32, 115, 188, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M357 241L361 243L368 256L372 257L369 259L369 263L377 275L377 279L376 276L373 277L367 272L361 274L360 273L358 274L360 273L362 275L367 273L374 280L371 284L369 282L370 279L367 277L366 279L363 280L365 280L363 281L363 283L361 283L360 281L354 283L353 281L348 286L347 289L337 289L338 287L336 287L325 274L327 269L325 265L321 263L320 260L319 261L316 257L319 253L321 253L320 248L322 247L322 245L328 243L327 244L330 245L332 244L332 242L333 243L335 241L338 242L339 244L340 243L341 245L341 243L354 243L357 241z"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(77, 143, 201, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M368 278L371 283L375 281L378 282L380 286L382 291L382 297L375 303L379 307L379 309L379 307L380 308L375 313L364 314L363 317L365 316L364 317L366 321L362 328L360 328L359 331L355 333L353 337L353 335L351 335L347 339L345 339L344 335L346 334L344 332L347 323L341 317L339 317L339 313L335 311L338 307L332 305L332 301L334 301L334 299L333 295L328 291L329 281L331 280L338 287L338 289L346 289L353 281L353 283L360 281L360 283L363 283L363 281L365 281L364 280L367 277z"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(77, 143, 201, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M331 293L334 298L332 305L338 307L335 310L339 313L338 316L344 319L347 323L347 325L345 326L346 329L342 334L340 334L337 340L337 337L335 336L335 334L333 334L332 337L330 336L328 338L326 338L326 336L324 336L325 338L322 339L318 337L318 342L312 344L308 340L306 341L303 337L303 330L307 328L305 328L307 325L303 324L300 321L305 319L306 317L303 318L306 311L302 308L302 306L305 302L309 303L310 305L312 301L316 303L319 302L320 299L327 293L331 293z"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(121, 171, 215, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M396 301L407 305L413 311L414 320L413 325L411 327L410 335L407 336L405 340L401 337L403 340L398 340L403 340L401 346L397 346L398 345L396 344L392 348L390 348L396 348L394 352L391 351L394 352L392 352L392 356L387 355L388 356L384 356L384 358L380 358L379 357L381 355L379 351L374 352L375 351L372 349L365 352L363 351L363 353L361 347L359 347L357 344L362 344L358 342L358 340L361 334L359 332L359 328L361 328L363 324L366 322L364 317L365 316L363 317L363 315L366 313L374 313L375 311L378 311L382 307L382 309L382 307L387 301L396 302zM394 354L393 353L396 352L395 353zM396 346L394 346L397 346zM394 302L394 302zM393 353L393 353zM390 301L389 301zM403 342L402 342zM392 348L392 348zM391 348L390 348zM403 343L402 343zM403 342L403 342zM403 342L403 342z"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(210, 227, 242, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M374 350L374 352L379 351L382 355L378 357L373 356L379 357L375 364L373 362L371 364L368 361L366 363L367 360L365 360L362 357L363 352L368 351L370 349L374 350z"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(121, 171, 215, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M360 331L359 332L361 334L359 336L358 341L362 344L358 344L363 350L363 357L367 361L366 363L368 361L371 364L373 362L375 364L379 358L390 357L390 359L386 359L388 361L390 361L390 359L391 362L406 360L406 369L399 372L396 375L396 377L386 380L380 384L372 381L360 382L355 384L353 380L351 380L351 382L349 382L350 380L348 380L348 382L340 377L344 381L338 385L335 384L336 383L334 384L333 379L332 380L333 377L329 375L338 375L335 367L337 367L337 365L339 366L340 361L335 350L337 342L336 339L337 340L340 337L340 334L344 332L346 334L344 335L345 338L347 339L351 335L353 335L353 337L355 335L354 334L359 331zM344 383L349 384L349 386L345 390L338 386L344 383zM395 359L396 361L394 362L391 361L390 359L394 359zM352 381L353 382L351 382L352 380zM353 381L353 381zM390 360L389 360zM390 359L389 359zM387 360L387 360zM349 381L349 381z"
                            />
                            <path
                              className="leaflet-interactive"
                              stroke="#3388ff"
                              strokeOpacity="1"
                              strokeWidth="3"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              fill="rgba(32, 115, 188, 1)"
                              fillOpacity="0.2"
                              fillRule="evenodd"
                              d="M334 334L337 337L335 351L340 363L339 362L339 366L338 365L336 367L338 374L336 376L332 374L329 375L333 377L333 383L334 382L334 384L336 384L330 386L328 384L329 385L327 385L329 387L330 386L330 388L327 390L325 388L318 389L316 387L319 391L318 392L316 388L308 384L300 386L297 389L296 386L294 391L295 394L293 395L294 398L293 397L290 403L288 402L289 401L286 402L286 400L282 400L280 398L281 397L278 396L278 394L279 399L273 397L271 399L267 399L260 405L259 403L257 407L259 409L255 412L252 407L249 406L246 409L244 408L245 404L251 403L259 395L259 391L265 389L270 382L271 375L276 375L280 371L279 372L277 369L279 367L305 367L305 363L307 361L306 360L308 360L313 355L313 345L318 341L317 339L319 337L322 339L325 338L325 336L325 338L330 336L332 338L331 335L333 334zM231 413L230 414L231 413zM268 369L268 367L268 369zM230 412L230 412zM231 411L231 411zM329 386L329 385zM229 414L229 414zM230 412L229 412zM328 386L328 386zM266 386L266 386z"
                            />
                          </g>
                        </svg>
                      </div>
                      <div className="leaflet-pane leaflet-shadow-pane" />
                      <div className="leaflet-pane leaflet-marker-pane" />
                      <div className="leaflet-pane leaflet-tooltip-pane" />
                      <div className="leaflet-pane leaflet-popup-pane" />
                      <div
                        className="leaflet-proxy leaflet-zoom-animated"
                        style={{
                          transform:
                            'translate3d(4022.87px, 2668.02px, 0px) scale(16)',
                        }}
                      />
                    </div>
                    <div className="leaflet-control-container">
                      <div className="leaflet-top leaflet-left">
                        <div className="leaflet-control-zoom leaflet-bar leaflet-control">
                          <a
                            className="leaflet-control-zoom-in"
                            href="#"
                            title="Zoom in"
                            role="button"
                            aria-label="Zoom in"
                            aria-disabled="false"
                          >
                            <span aria-hidden="true">+</span>
                          </a>
                          <a
                            className="leaflet-control-zoom-out leaflet-disabled"
                            href="#"
                            title="Zoom out"
                            role="button"
                            aria-label="Zoom out"
                            aria-disabled="true"
                          >
                            <span aria-hidden="true">−</span>
                          </a>
                        </div>
                      </div>
                      <div className="leaflet-top leaflet-right" />
                      <div className="leaflet-bottom leaflet-left" />
                      <div className="leaflet-bottom leaflet-right">
                        <div className="leaflet-control-attribution leaflet-control">
                          <a
                            href="https://leafletjs.com"
                            title="A JavaScript library for interactive maps"
                          >
                            <svg
                              aria-hidden="true"
                              xmlns="http://www.w3.org/2000/svg"
                              width="12"
                              height="8"
                              viewBox="0 0 12 8"
                              className="leaflet-attribution-flag"
                            >
                              <path fill="#4C7BE1" d="M0 0h12v4H0z" />
                              <path fill="#FFD500" d="M0 4h12v3H0z" />
                              <path fill="#E0BC00" d="M0 7h12v1H0z" />
                            </svg>{' '}
                            Leaflet
                          </a>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="govuk-grid-column-one-third">
                  <h3 className="govuk-heading-s dfe-word-break--break-word">
                    Key to Indicative starts rate per 100,000 population
                    (2021/22)
                  </h3>
                  <ul className="govuk-list" data-testid="mapBlock-legend">
                    <li
                      className="MapBlock_legend__ZVvW0"
                      data-testid="mapBlock-legend-item"
                    >
                      <span
                        className="MapBlock_legendIcon__f70jz"
                        data-testid="mapBlock-legend-colour"
                        style={{ backgroundColor: 'rgb(210, 227, 242)' }}
                      />
                      624 to 740
                    </li>
                    <li
                      className="MapBlock_legend__ZVvW0"
                      data-testid="mapBlock-legend-item"
                    >
                      <span
                        className="MapBlock_legendIcon__f70jz"
                        data-testid="mapBlock-legend-colour"
                        style={{ backgroundColor: 'rgb(166, 199, 228)' }}
                      />
                      741 to 857
                    </li>
                    <li
                      className="MapBlock_legend__ZVvW0"
                      data-testid="mapBlock-legend-item"
                    >
                      <span
                        className="MapBlock_legendIcon__f70jz"
                        data-testid="mapBlock-legend-colour"
                        style={{ backgroundColor: 'rgb(121, 171, 215)' }}
                      />
                      858 to 974
                    </li>
                    <li
                      className="MapBlock_legend__ZVvW0"
                      data-testid="mapBlock-legend-item"
                    >
                      <span
                        className="MapBlock_legendIcon__f70jz"
                        data-testid="mapBlock-legend-colour"
                        style={{ backgroundColor: 'rgb(77, 143, 201' }}
                      />
                      975 to 1,091
                    </li>
                    <li
                      className="MapBlock_legend__ZVvW0"
                      data-testid="mapBlock-legend-item"
                    >
                      <span
                        className="MapBlock_legendIcon__f70jz"
                        data-testid="mapBlock-legend-colour"
                        style={{ backgroundColor: 'rgb(32, 115, 188' }}
                      />
                      1,092 to 1,205
                    </li>
                  </ul>
                  <div
                    aria-live="polite"
                    className="govuk-!-margin-top-5"
                    data-testid="mapBlock-indicator"
                  />
                </div>
              </div>
              <h3 className="govuk-heading-m">
                Footnotes
                <span className="govuk-visually-hidden">
                  {' '}
                  for Apprenticeship starts by region [volume and per 100k
                  population]
                </span>
              </h3>
              <ol
                id="chartFootnotes-dataBlock-2904c403-643a-4877-9b5a-9adc7785fa19-chart"
                className="govuk-list govuk-list--number"
                data-testid="footnotes"
              >
                <li>
                  <div className="dfe-content">
                    Figures for 2022/23 are provisional and cover the first
                    three quarters (Aug 2022 to Apr 2023). All other years are
                    final, full-year figures.
                  </div>
                </li>
                <li>
                  <div className="dfe-content">
                    Volumes are rounded to the nearest 10 and 'low' indicates a
                    base value of fewer than 5. Where data shows 'x' this
                    indicates data is unavailable, 'z' indicates data is not
                    applicable, and 'c' indicates data is suppressed.
                  </div>
                </li>
              </ol>

              <p className="govuk-body-s">
                Source: Individualised Learner Record (ILR)
              </p>
            </figure>
            <div className="govuk-!-display-none-print">
              <h3 className="govuk-heading-m">
                Explore and edit this data online
                <span className="govuk-visually-hidden">
                  {' '}
                  for Apprenticeship starts by region [volume and per 100k
                  population]
                </span>
              </h3>
              <p>Use our table tool to explore this data.</p>
              <a
                href="/data-tables/fast-track/2904c403-643a-4877-9b5a-9adc7785fa19"
                className="govuk-button"
                aria-disabled="false"
              >
                Explore data
                <span className="govuk-visually-hidden">
                  {' '}
                  for Apprenticeship starts by region [volume and per 100k
                  population]
                </span>
              </a>
            </div>
          </TabsSection>
          <TabsSection title="Table">
            <figure
              className="FixedMultiHeaderDataTable_figure__nHDwl"
              style={{ padding: '0', margin: '0' }}
            >
              <figcaption>
                <strong
                  id="dataTableCaption-53358e8f-5fc1-4ecd-b6ac-bd1310c6a0ad"
                  data-testid="dataTableCaption"
                >
                  Apprenticeship achievements by region [volume and per 100k
                  population]
                </strong>
              </figcaption>
              <div
                className="FixedMultiHeaderDataTable_container__7qrG_"
                role="region"
                style={{
                  maxWidth: '750px',
                  overflow: 'auto',
                  marginBottom: '1rem',
                }}
              >
                <table
                  data-testid="dataTableCaption-53358e8f-5fc1-4ecd-b6ac-bd1310c6a0ad-table"
                  aria-labelledby="dataTableCaption-53358e8f-5fc1-4ecd-b6ac-bd1310c6a0ad"
                  className="govuk-table"
                >
                  <thead>
                    <tr>
                      <td colSpan={1} rowSpan={2} />
                      <th colSpan={5} rowSpan={1} scope="colgroup">
                        Achievements
                      </th>
                      <th colSpan={5} rowSpan={1} scope="colgroup">
                        Indicative achievements rate per 100,000 population
                      </th>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2017/18
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2018/19
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2019/20
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2020/21
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2021/22
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2017/18
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2018/19
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2019/20
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2020/21
                      </th>
                      <th colSpan={1} rowSpan={1} scope="col">
                        2021/22
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        North East
                      </th>
                      <td className="govuk-table__cell--numeric">20,000</td>
                      <td className="govuk-table__cell--numeric">12,390</td>
                      <td className="govuk-table__cell--numeric">9,260</td>
                      <td className="govuk-table__cell--numeric">9,040</td>
                      <td className="govuk-table__cell--numeric">7,710</td>
                      <td className="govuk-table__cell--numeric">1,206</td>
                      <td className="govuk-table__cell--numeric">746</td>
                      <td className="govuk-table__cell--numeric">557</td>
                      <td className="govuk-table__cell--numeric">543</td>
                      <td className="govuk-table__cell--numeric">471</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        North West
                      </th>
                      <td className="govuk-table__cell--numeric">44,330</td>
                      <td className="govuk-table__cell--numeric">29,120</td>
                      <td className="govuk-table__cell--numeric">22,220</td>
                      <td className="govuk-table__cell--numeric">22,710</td>
                      <td className="govuk-table__cell--numeric">20,490</td>
                      <td className="govuk-table__cell--numeric">977</td>
                      <td className="govuk-table__cell--numeric">641</td>
                      <td className="govuk-table__cell--numeric">487</td>
                      <td className="govuk-table__cell--numeric">497</td>
                      <td className="govuk-table__cell--numeric">442</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Yorkshire and The Humber
                      </th>
                      <td className="govuk-table__cell--numeric">33,520</td>
                      <td className="govuk-table__cell--numeric">22,160</td>
                      <td className="govuk-table__cell--numeric">17,650</td>
                      <td className="govuk-table__cell--numeric">18,870</td>
                      <td className="govuk-table__cell--numeric">15,830</td>
                      <td className="govuk-table__cell--numeric">983</td>
                      <td className="govuk-table__cell--numeric">649</td>
                      <td className="govuk-table__cell--numeric">516</td>
                      <td className="govuk-table__cell--numeric">550</td>
                      <td className="govuk-table__cell--numeric">463</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        East Midlands
                      </th>
                      <td className="govuk-table__cell--numeric">25,880</td>
                      <td className="govuk-table__cell--numeric">17,010</td>
                      <td className="govuk-table__cell--numeric">13,700</td>
                      <td className="govuk-table__cell--numeric">13,970</td>
                      <td className="govuk-table__cell--numeric">12,540</td>
                      <td className="govuk-table__cell--numeric">870</td>
                      <td className="govuk-table__cell--numeric">570</td>
                      <td className="govuk-table__cell--numeric">458</td>
                      <td className="govuk-table__cell--numeric">464</td>
                      <td className="govuk-table__cell--numeric">412</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        West Midlands
                      </th>
                      <td className="govuk-table__cell--numeric">34,190</td>
                      <td className="govuk-table__cell--numeric">21,800</td>
                      <td className="govuk-table__cell--numeric">17,130</td>
                      <td className="govuk-table__cell--numeric">17,290</td>
                      <td className="govuk-table__cell--numeric">15,190</td>
                      <td className="govuk-table__cell--numeric">940</td>
                      <td className="govuk-table__cell--numeric">597</td>
                      <td className="govuk-table__cell--numeric">468</td>
                      <td className="govuk-table__cell--numeric">470</td>
                      <td className="govuk-table__cell--numeric">412</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        East of England
                      </th>
                      <td className="govuk-table__cell--numeric">24,070</td>
                      <td className="govuk-table__cell--numeric">17,380</td>
                      <td className="govuk-table__cell--numeric">14,320</td>
                      <td className="govuk-table__cell--numeric">15,700</td>
                      <td className="govuk-table__cell--numeric">13,950</td>
                      <td className="govuk-table__cell--numeric">637</td>
                      <td className="govuk-table__cell--numeric">460</td>
                      <td className="govuk-table__cell--numeric">378</td>
                      <td className="govuk-table__cell--numeric">413</td>
                      <td className="govuk-table__cell--numeric">357</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        London
                      </th>
                      <td className="govuk-table__cell--numeric">23,970</td>
                      <td className="govuk-table__cell--numeric">15,750</td>
                      <td className="govuk-table__cell--numeric">12,860</td>
                      <td className="govuk-table__cell--numeric">15,290</td>
                      <td className="govuk-table__cell--numeric">13,590</td>
                      <td className="govuk-table__cell--numeric">401</td>
                      <td className="govuk-table__cell--numeric">262</td>
                      <td className="govuk-table__cell--numeric">213</td>
                      <td className="govuk-table__cell--numeric">253</td>
                      <td className="govuk-table__cell--numeric">224</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        South East
                      </th>
                      <td className="govuk-table__cell--numeric">36,480</td>
                      <td className="govuk-table__cell--numeric">25,840</td>
                      <td className="govuk-table__cell--numeric">20,890</td>
                      <td className="govuk-table__cell--numeric">23,380</td>
                      <td className="govuk-table__cell--numeric">20,290</td>
                      <td className="govuk-table__cell--numeric">650</td>
                      <td className="govuk-table__cell--numeric">460</td>
                      <td className="govuk-table__cell--numeric">372</td>
                      <td className="govuk-table__cell--numeric">415</td>
                      <td className="govuk-table__cell--numeric">352</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        South West
                      </th>
                      <td className="govuk-table__cell--numeric">31,270</td>
                      <td className="govuk-table__cell--numeric">22,090</td>
                      <td className="govuk-table__cell--numeric">17,680</td>
                      <td className="govuk-table__cell--numeric">19,020</td>
                      <td className="govuk-table__cell--numeric">16,220</td>
                      <td className="govuk-table__cell--numeric">928</td>
                      <td className="govuk-table__cell--numeric">653</td>
                      <td className="govuk-table__cell--numeric">523</td>
                      <td className="govuk-table__cell--numeric">560</td>
                      <td className="govuk-table__cell--numeric">468</td>
                    </tr>
                    <tr>
                      <th colSpan={1} rowSpan={1} scope="row" className="">
                        Outside of England and unknown
                      </th>
                      <td className="govuk-table__cell--numeric">2,460</td>
                      <td className="govuk-table__cell--numeric">1,610</td>
                      <td className="govuk-table__cell--numeric">1,190</td>
                      <td className="govuk-table__cell--numeric">1,270</td>
                      <td className="govuk-table__cell--numeric">1,410</td>
                      <td className="govuk-table__cell--numeric">z</td>
                      <td className="govuk-table__cell--numeric">z</td>
                      <td className="govuk-table__cell--numeric">z</td>
                      <td className="govuk-table__cell--numeric">z</td>
                      <td className="govuk-table__cell--numeric">z</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div>
                <h3 className="govuk-heading-m">
                  Footnotes
                  <span className="govuk-visually-hidden">
                    {' '}
                    for Apprenticeship achievements by region [volume and per
                    100k population]
                  </span>
                </h3>
                <ol
                  id="dataTableFootnotes-53358e8f-5fc1-4ecd-b6ac-bd1310c6a0ad"
                  className="govuk-list govuk-list--number"
                  data-testid="footnotes"
                >
                  <li>
                    <div className="dfe-content">
                      Figures for 2022/23 are provisional and cover the first
                      three quarters (Aug 2022 to Apr 2023). All other years are
                      final, full-year figures.
                    </div>
                  </li>
                  <li>
                    <div className="dfe-content">
                      Volumes are rounded to the nearest 10 and 'low' indicates
                      a base value of fewer than 5. Where data shows 'x' this
                      indicates data is unavailable, 'z' indicates data is not
                      applicable, and 'c' indicates data is suppressed.
                    </div>
                  </li>
                </ol>
              </div>
              <p className="govuk-body-s">
                Source: Individualised Learner Record (ILR)
              </p>
            </figure>
          </TabsSection>
        </Tabs>
      )}

      {chartType === 'lineChart' && (
        <figure
          className="govuk-!-margin-0"
          id="dataBlock-0cd5437b-0be3-4cc8-9b62-249df4d4076c-chart"
          data-testid="dataBlock-0cd5437b-0be3-4cc8-9b62-249df4d4076c-chart"
        >
          <figcaption className="govuk-heading-s">
            Full history of Adult (19+) FE and skills participation
          </figcaption>
          <div className="govuk-!-margin-bottom-6">
            <div className="dfe-flex dfe-align-items--center">
              <div style={{ width: '100%' }}>
                <div
                  className="recharts-responsive-container"
                  style={{ width: '100%', height: '600px', minWidth: '0px' }}
                >
                  <div
                    className="recharts-wrapper"
                    role="region"
                    style={{
                      position: 'relative',
                      cursor: 'default',
                      width: '100%',
                      height: '600px',
                    }}
                  >
                    <svg
                      aria-label="The chart shows full history of adult 19 plus participation in FE and skills, Education and training, apprenticeships and community learning from 2010/11 to 2021/22 full year. It has markers to show when the single ILR data return was introduced in 2011/12 and the introduction of the apprenticeship levy in May 2017."
                      role="img"
                      focusable="false"
                      className="recharts-surface"
                      width="100%"
                      height="600"
                      viewBox="0 0 918 600"
                    >
                      <title />
                      <desc />
                      <defs />
                      <g className="recharts-cartesian-grid">
                        <g className="recharts-cartesian-grid-horizontal">
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="550"
                            x2="918"
                            y2="550"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="474.2857142857143"
                            x2="918"
                            y2="474.2857142857143"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="398.5714285714286"
                            x2="918"
                            y2="398.5714285714286"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="322.85714285714283"
                            x2="918"
                            y2="322.85714285714283"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="247.14285714285714"
                            x2="918"
                            y2="247.14285714285714"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="171.42857142857142"
                            x2="918"
                            y2="171.42857142857142"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="95.71428571428574"
                            x2="918"
                            y2="95.71428571428574"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="20"
                            x2="918"
                            y2="20"
                          />
                        </g>
                        <g className="recharts-cartesian-grid-vertical">
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="134"
                            y1="20"
                            x2="134"
                            y2="550"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="898"
                            y1="20"
                            x2="898"
                            y2="550"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="114"
                            y1="20"
                            x2="114"
                            y2="550"
                          />
                          <line
                            strokeDasharray="3 3"
                            stroke="#ccc"
                            fill="none"
                            x="114"
                            y="20"
                            width="804"
                            height="530"
                            offset="[object Object]"
                            x1="918"
                            y1="20"
                            x2="918"
                            y2="550"
                          />
                        </g>
                      </g>
                      <g className="recharts-layer recharts-cartesian-axis recharts-yAxis yAxis">
                        <line
                          width="84"
                          orientation="left"
                          height="530"
                          x="30"
                          y="20"
                          className="recharts-cartesian-axis-line"
                          stroke="#666"
                          fill="none"
                          x1="114"
                          y1="20"
                          x2="114"
                          y2="550"
                        />
                        <g className="recharts-cartesian-axis-ticks">
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="550"
                              x2="114"
                              y2="550"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="550"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                0
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="474.2857142857143"
                              x2="114"
                              y2="474.2857142857143"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="474.2857142857143"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                500,000
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="398.5714285714286"
                              x2="114"
                              y2="398.5714285714286"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="398.5714285714286"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                1,000,000
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="322.85714285714283"
                              x2="114"
                              y2="322.85714285714283"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="322.85714285714283"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                1,500,000
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="247.14285714285714"
                              x2="114"
                              y2="247.14285714285714"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="247.14285714285714"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                2,000,000
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="171.42857142857142"
                              x2="114"
                              y2="171.42857142857142"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="171.42857142857142"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                2,500,000
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="95.71428571428574"
                              x2="114"
                              y2="95.71428571428574"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="95.71428571428574"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                3,000,000
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              width="84"
                              orientation="left"
                              height="530"
                              x="30"
                              y="20"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="108"
                              y1="20"
                              x2="114"
                              y2="20"
                            />
                            <text
                              width="84"
                              orientation="left"
                              height="530"
                              x="106"
                              y="20"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="end"
                            >
                              <tspan x="106" dy="0.355em">
                                3,500,000
                              </tspan>
                            </text>
                          </g>
                        </g>
                      </g>
                      <g className="recharts-layer recharts-cartesian-axis recharts-xAxis xAxis">
                        <line
                          height="50"
                          orientation="bottom"
                          width="804"
                          x="114"
                          y="550"
                          className="recharts-cartesian-axis-line"
                          stroke="#666"
                          fill="none"
                          x1="114"
                          y1="550"
                          x2="918"
                          y2="550"
                        />
                        <g className="recharts-cartesian-axis-ticks">
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              height="50"
                              orientation="bottom"
                              width="804"
                              x="114"
                              y="550"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="134"
                              y1="556"
                              x2="134"
                              y2="550"
                            />
                            <text
                              height="50"
                              orientation="bottom"
                              width="804"
                              x="134"
                              y="566"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="middle"
                            >
                              <tspan x="134" dy="0.71em">
                                2010/11
                              </tspan>
                            </text>
                          </g>
                          <g className="recharts-layer recharts-cartesian-axis-tick">
                            <line
                              height="50"
                              orientation="bottom"
                              width="804"
                              x="114"
                              y="550"
                              className="recharts-cartesian-axis-tick-line"
                              stroke="#666"
                              fill="none"
                              x1="898"
                              y1="556"
                              x2="898"
                              y2="550"
                            />
                            <text
                              height="50"
                              orientation="bottom"
                              width="804"
                              x="882.125"
                              y="566"
                              stroke="none"
                              fill="#0b0c0c"
                              className="recharts-text recharts-cartesian-axis-tick-value"
                              textAnchor="middle"
                            >
                              <tspan x="882.125" dy="0.71em">
                                2021/22
                              </tspan>
                            </text>
                          </g>
                        </g>
                      </g>
                      <g className="recharts-layer recharts-line">
                        <path
                          name="Further education and skills"
                          stroke="#12436D"
                          fill="none"
                          strokeWidth="2"
                          strokeDasharray=""
                          width="804"
                          height="530"
                          className="recharts-curve recharts-line-curve"
                          d="M134,71.001L203.455,73.045L272.909,53.223L342.364,106.375L411.818,154.211L481.273,197.974L550.727,211.285L620.182,220.022L689.636,236.815L759.091,285.636L828.545,301.612L898,289.603"
                        />
                        <g className="recharts-layer" />
                        <g
                          className="recharts-layer recharts-line-dots"
                          role="img"
                        />
                        <g className="recharts-layer recharts-label-list" />
                      </g>
                      <g className="recharts-layer recharts-line">
                        <path
                          name="Education and training"
                          stroke="#28a197"
                          fill="none"
                          strokeWidth="2"
                          strokeDasharray=""
                          width="804"
                          height="530"
                          className="recharts-curve recharts-line-curve"
                          d="M134,366.257L203.455,320.131L272.909,280.124L342.364,307.154L411.818,344.814L481.273,383.656L550.727,386.397L620.182,378.628L689.636,385.897L759.091,417.485L828.545,418.318L898,416.97"
                        />
                        <g className="recharts-layer" />
                        <g
                          className="recharts-layer recharts-line-dots"
                          role="img"
                        />
                        <g className="recharts-layer recharts-label-list" />
                      </g>
                      <g className="recharts-layer recharts-reference-line">
                        <line
                          stroke="#b1b4b6"
                          strokeDasharray="8 4"
                          strokeWidth="3"
                          x="2011_AY"
                          fill="none"
                          fillOpacity="1"
                          x1="203.45454545454544"
                          y1="550"
                          x2="203.45454545454544"
                          y2="20"
                          className="recharts-reference-line-line"
                        />
                        <text
                          x="203.45454545454544"
                          y="20"
                          fill="#808080"
                          className="recharts-text CustomReferenceLineLabel_text__h6bVf"
                          textAnchor="middle"
                        >
                          <tspan x="203.45454545454544" dy="0em">
                            SILR introduced
                          </tspan>
                        </text>
                      </g>
                      <g className="recharts-layer recharts-reference-line">
                        <line
                          stroke="#b1b4b6"
                          strokeDasharray="8 4"
                          strokeWidth="3"
                          x="2016_AY"
                          fill="none"
                          fillOpacity="1"
                          x1="550.7272727272727"
                          y1="550"
                          x2="550.7272727272727"
                          y2="20"
                          className="recharts-reference-line-line"
                        />
                        <text
                          x="550.7272727272727"
                          y="20"
                          fill="#808080"
                          className="recharts-text CustomReferenceLineLabel_text__h6bVf"
                          textAnchor="middle"
                        >
                          <tspan x="550.7272727272727" dy="0em">
                            Apprenticeship levy introduced (May 2017)
                          </tspan>
                        </text>
                      </g>
                    </svg>
                    <div
                      className="recharts-legend-wrapper"
                      style={{
                        position: 'absolute',
                        width: 'auto',
                        height: 'auto',
                        left: '30px',
                        bottom: '0px',
                      }}
                    />
                    <div
                      role="dialog"
                      className="recharts-tooltip-wrapper recharts-tooltip-wrapper-right recharts-tooltip-wrapper-top"
                      style={{
                        pointerEvents: 'none',
                        visibility: 'hidden',
                        position: 'absolute',
                        top: '0px',
                        left: '0px',
                        zIndex: '1000',
                        transform: 'translate(560.727px, 344.812px)',
                      }}
                    >
                      <div
                        className="CustomTooltip_tooltip__vB5AA"
                        data-testid="chartTooltip"
                      >
                        <p
                          className="govuk-!-font-weight-bold"
                          data-testid="chartTooltip-label"
                        >
                          2016/17
                        </p>
                        <ul
                          className="CustomTooltip_itemList___oA4a"
                          data-testid="chartTooltip-items"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div aria-hidden="true" className="govuk-!-margin-bottom-6">
            <ul
              className="recharts-default-legend"
              style={{ padding: '0px', margin: '0px', textAlign: 'left' }}
            >
              <li
                className="recharts-legend-item legend-item-0"
                style={{ display: 'block', marginRight: '10px' }}
              >
                <svg
                  className="recharts-surface"
                  width="14"
                  height="14"
                  viewBox="0 0 32 32"
                  style={{
                    display: 'inline-block',
                    verticalAlign: 'middle',
                    marginRight: '4px',
                  }}
                >
                  <title />
                  <desc />
                  <path
                    strokeWidth="4"
                    fill="none"
                    stroke="#12436D"
                    d="M0,16h10.666666666666666
            A5.333333333333333,5.333333333333333,0,1,1,21.333333333333332,16
            H32M21.333333333333332,16
            A5.333333333333333,5.333333333333333,0,1,1,10.666666666666666,16"
                    className="recharts-legend-icon"
                  />
                </svg>
                <span
                  className="recharts-legend-item-text"
                  style={{ color: 'rgb(18, 67, 109)' }}
                >
                  Further education and skills
                </span>
              </li>
              <li
                className="recharts-legend-item legend-item-1"
                style={{ display: 'block', marginRight: '10px' }}
              >
                <svg
                  className="recharts-surface"
                  width="14"
                  height="14"
                  viewBox="0 0 32 32"
                  style={{
                    display: 'inline-block',
                    verticalAlign: 'middle',
                    marginRight: '4px',
                  }}
                >
                  <title />
                  <desc />
                  <path
                    strokeWidth="4"
                    fill="none"
                    stroke="#28a197"
                    d="M0,16h10.666666666666666
            A5.333333333333333,5.333333333333333,0,1,1,21.333333333333332,16
            H32M21.333333333333332,16
            A5.333333333333333,5.333333333333333,0,1,1,10.666666666666666,16"
                    className="recharts-legend-icon"
                  />
                </svg>
                <span
                  className="recharts-legend-item-text"
                  style={{ color: 'rgb(40, 161, 151)' }}
                >
                  Education and training
                </span>
              </li>
              <li
                className="recharts-legend-item legend-item-2"
                style={{ display: 'block', marginRight: '10px' }}
              >
                <svg
                  className="recharts-surface"
                  width="14"
                  height="14"
                  viewBox="0 0 32 32"
                  style={{
                    display: 'inline-block',
                    verticalAlign: 'middle',
                    marginRight: '4px',
                  }}
                >
                  <title />
                  <desc />
                  <path
                    strokeWidth="4"
                    fill="none"
                    stroke="#801650"
                    d="M0,16h10.666666666666666
            A5.333333333333333,5.333333333333333,0,1,1,21.333333333333332,16
            H32M21.333333333333332,16
            A5.333333333333333,5.333333333333333,0,1,1,10.666666666666666,16"
                    className="recharts-legend-icon"
                  />
                </svg>
                <span
                  className="recharts-legend-item-text"
                  style={{ color: 'rgb(128, 22, 80)' }}
                >
                  Apprenticeships
                </span>
              </li>
              <li
                className="recharts-legend-item legend-item-3"
                style={{ display: 'block', marginRight: '10px' }}
              >
                <svg
                  className="recharts-surface"
                  width="14"
                  height="14"
                  viewBox="0 0 32 32"
                  style={{
                    display: 'inline-block',
                    verticalAlign: 'middle',
                    marginRight: '4px',
                  }}
                >
                  <title />
                  <desc />
                  <path
                    strokeWidth="4"
                    fill="none"
                    stroke="#f46a25"
                    d="M0,16h10.666666666666666
            A5.333333333333333,5.333333333333333,0,1,1,21.333333333333332,16
            H32M21.333333333333332,16
            A5.333333333333333,5.333333333333333,0,1,1,10.666666666666666,16"
                    className="recharts-legend-icon"
                  />
                </svg>
                <span
                  className="recharts-legend-item-text"
                  style={{ color: 'rgb(244, 106, 37)' }}
                >
                  Community learning
                </span>
              </li>
              <li
                className="recharts-legend-item legend-item-4"
                style={{ display: 'block', marginRight: '10px' }}
              >
                <svg
                  className="recharts-surface"
                  width="14"
                  height="14"
                  viewBox="0 0 32 32"
                  style={{
                    display: 'inline-block',
                    verticalAlign: 'middle',
                    marginRight: '4px',
                  }}
                >
                  <title />
                  <desc />
                  <path
                    strokeWidth="4"
                    fill="none"
                    stroke="#2073bc"
                    d="M0,16h10.666666666666666
            A5.333333333333333,5.333333333333333,0,1,1,21.333333333333332,16
            H32M21.333333333333332,16
            A5.333333333333333,5.333333333333333,0,1,1,10.666666666666666,16"
                    className="recharts-legend-icon"
                  />
                </svg>
                <span
                  className="recharts-legend-item-text"
                  style={{ color: 'rgb(32, 115, 188)' }}
                >
                  Workplace learning
                </span>
              </li>
            </ul>
          </div>
          <h3 className="govuk-heading-m">
            Footnotes
            <span className="govuk-visually-hidden">
              {' '}
              for Full history of Adult (19+) FE and skills participation
            </span>
          </h3>
          <ol
            id="chartFootnotes-dataBlock-0cd5437b-0be3-4cc8-9b62-249df4d4076c-chart"
            className="govuk-list govuk-list--number"
            data-testid="footnotes"
          >
            <li>
              <div className="dfe-content">
                Volumes are rounded to the nearest 100 and 'low' indicates a
                base value of fewer than 50. Where data shows 'x' this indicates
                data is unavailable, 'z' indicates data is not applicable, and
                'c' indicates data is suppressed.
              </div>
            </li>
            <li>
              <div className="dfe-content">
                Participation is the count of learners that participated at any
                point during the year. Learners undertaking more than one course
                will appear only once in the grand total.
              </div>
            </li>
          </ol>

          <p className="govuk-body-s">
            Source: Individualised Learner Record (ILR)
          </p>
        </figure>
      )}
    </>
  );
};

export default ExampleCharts;
