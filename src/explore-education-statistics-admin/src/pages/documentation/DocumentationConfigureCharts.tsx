import React from 'react';
import Link from '@admin/components/Link';
import { RouteChildrenProps } from 'react-router';
import Page from '@admin/components/Page';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imageChartCreate from './images/guidance/guidance-chart-create.png';
import imageChartSelect from './images/guidance/guidance-chart-select.png';
import imageChartAddData from './images/guidance/guidance-chart-add-data.png';
import imageChartPreview from './images/guidance/guidance-chart-preview.png';
import imageChartLabel from './images/guidance/guidance-chart-label.png';
import imageChartStyling from './images/guidance/guidance-chart-styling.png';
import imageChartColour from './images/guidance/guidance-chart-colour.png';
import imageChartShape from './images/guidance/guidance-chart-shape.png';
import imageChartTitle from './images/guidance/guidance-chart-title.png';
import imageChartLegend from './images/guidance/guidance-chart-legend.png';
import imageChartHeight from './images/guidance/guidance-chart-height.png';
import imageChartWidth from './images/guidance/guidance-chart-width.png';
import imageChartStackedOff from './images/guidance/guidance-chart-stacked-off.png';
import imageChartStackedOn from './images/guidance/guidance-chart-stacked-on.png';
import imageChartXGroupTime from './images/guidance/guidance-chart-x-group-time.png';
import imageChartXAxisSize from './images/guidance/guidance-chart-x-axis-size.png';
import imageChartXGroupLocation from './images/guidance/guidance-chart-x-group-locations.png';
import imageChartXAutomatic from './images/guidance/guidance-chart-x-type-automatic.png';
import imageChartXStartEnd from './images/guidance/guidance-chart-x-type-start-end.png';
import imageChartXCustom from './images/guidance/guidance-chart-x-type-custom.png';
import imageChartXSort from './images/guidance/guidance-chart-x-sort.png';
import imageChartXLimitData from './images/guidance/guidance-chart-x-limit-data.png';
import imageChartXRefLine from './images/guidance/guidance-chart-x-ref-line.png';
import imageChartXRefLineConfig from './images/guidance/guidance-chart-x-ref-line-config.png';
import imageChartYAxisSize from './images/guidance/guidance-chart-y-axis-size.png';
import imageChartYUnit from './images/guidance/guidance-chart-y-unit.png';
import imageChartYAxisDefault from './images/guidance/guidance-chart-y-axis-range-default.png';
import imageChartYAxisAdjusted from './images/guidance/guidance-chart-y-axis-range-adjusted.png';
import imageMapLocation from './images/guidance/guidance-map-locations.png';
import imageMapAddData from './images/guidance/guidance-map-add-data.png';
import imageMapPreview from './images/guidance/guidance-map-preview.png';
import imageMapConfig from './images/guidance/guidance-map-config.png';
import imageInfographicLink from './images/guidance/guidance-infographic-link.png';
import imageInfographicSelect from './images/guidance/guidance-infographic-select.png';
import imageInfographicLarge from './images/guidance/guidance-infographic-image-large.png';
import imageInfographicResized from './images/guidance/guidance-infographic-image-resized.png';

const DocumentationManageDataBlock = ({ location: _ }: RouteChildrenProps) => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Configuring charts' },
      ]}
      title="Configuring charts"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step-by-step guidance</span>
            <h1 className="govuk-heading-xl">Configuring charts</h1>
          </div>
          <p>How to configure charts for use within a release. </p>{' '}
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Create chart"
              open={step === 1}
            >
              <p>
                Charts are created based on the data from saved data blocks.
              </p>
              <h3>Before you start</h3>
              <p>
                You will need to have created a data block on which to base your
                chart.
              </p>

              <p>
                For detailed guidance on how to add data blocks{' '}
                <Link to="/documentation/manage-data">
                  Managing data blocks: step by step
                </Link>
                .
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To start the process of creating a chart - within 'Manage
                    data' select a saved data block from the dropdown box, and
                    then select the 'Configure content' tab.
                  </h4>
                  <p>
                    The page will default to the table view, click the
                    'Configure chart' tab to continue.
                  </p>
                  <img
                    src={imageChartCreate}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Select the type of chart</h4>
                  <p>
                    Choose from line, vertical bar, horizontal bar and
                    geographic map.
                  </p>
                  <img
                    src={imageChartSelect}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Add data to chart</h4>
                  <p>
                    Select an option from each of the available dropdown menus,
                    and select the 'Add data' button
                  </p>
                  <p>After selecting 'Add data' a preview chart will appear.</p>
                  <p>You can continue to add mulitiple items to the chart</p>
                  <img
                    src={imageChartAddData}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Preview chart</h4>
                  <p>
                    The preview chart will appear after data has been added.
                  </p>
                  <p>
                    At any stage you can make alterations to the chart
                    configuration, for instance change to a different type of
                    chart, or alter the configuration options (shown below the
                    chart preview).
                  </p>
                  <img
                    src={imageChartPreview}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Save chart options</h4>
                  <p>
                    Once you are happy with your chart, select the green 'Save
                    chart options' button.
                  </p>
                  <h3>Don't</h3>
                  <p>
                    Don't worry if you need to make any changes, you can come
                    back and edit the chart at any point, by following the steps
                    above again.
                  </p>
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={2}
              stepHeading="Create a geographic map"
              open={step === 2}
            >
              <h3>Before you start</h3>
              <p>
                Ensure you have first created a data block with multiple
                locations selected.
              </p>
              <p>Example below shows all local authorities selected</p>
              <img
                src={imageMapLocation}
                alt=""
                className="govuk-!-margin-bottom-9"
                width="400"
              />
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    Select a saved data block from the dropdown box, and then
                    select the 'Configure content' tab.
                  </h4>
                  <p>
                    The page will default to the table view, click the
                    'Configure chart' and then select the 'Geographic' link.
                  </p>

                  <img
                    src={imageMapAddData}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Add data and preview map</h4>
                  <p>
                    Select an option from each of the available dropdown menus,
                    and select the 'Add data' button
                  </p>
                  <p>After selecting 'Add data' a preview map will appear.</p>
                  <p>You can continue to add mulitiple items to the chart</p>
                  <img
                    src={imageMapPreview}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Resizing map</h4>
                  <p>
                    The default map size had a height of 300px, this can be
                    adjusted and increased in size if required.
                  </p>
                  <img
                    src={imageMapConfig}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Save chart options</h4>
                  <p>
                    Once you are happy with your map, select the green 'Save
                    chart options' button.
                  </p>
                  <h3>Don't</h3>
                  <p>
                    Don't worry you need to make any changes, you can come back
                    and edit the map at any point, by following the steps above
                    again.
                  </p>
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={3}
              stepHeading="Change line or bar styling"
              open={step === 3}
            >
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    Within the data tab, select the 'Change styling' link to
                    reveal the styling options available for each data type.
                  </h4>
                  <img
                    src={imageChartStyling}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Change legend titles</h4>
                  <p>
                    The label for the indicator can be edited, this option is
                    useful if the default indicator is a long string of text.
                  </p>
                  <img
                    src={imageChartLabel}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Change bar or line colour{' '}
                  </h4>
                  <p>
                    Click on the colour option to reveal pre-set options or
                    choose your own colour by selecting the 'other' button.
                  </p>
                  <img
                    src={imageChartColour}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Change symbol </h4>
                  <p>
                    This feature is used on line charts to help colour blind
                    users differentiate between each of the lines.
                  </p>
                  <img
                    src={imageChartShape}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={4}
              stepHeading="Chart configuration"
              open={step === 4}
            >
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">Give your chart a title</h4>
                  <p>The title will be displayed above your chart.</p>
                  <img
                    src={imageChartTitle}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Set legend position</h4>
                  <p>
                    The default legend position is 'top' shown above the chart.
                  </p>
                  <p>
                    The legend can be moved to 'bottom' shown below the chart,
                    or 'none' where the legend details won't be shown.
                  </p>
                  <img
                    src={imageChartLegend}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Set chart height</h4>
                  <p>
                    The default height is preset to 300px, depending on the
                    amount of data shown on your chart you may wish to increase
                    or decrease this value for maximum visual impact.
                  </p>
                  <img
                    src={imageChartHeight}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Set chart width</h4>
                  <p>
                    By default the chart is set to be displayed as full width of
                    the release page. This width can be reduced by adding in a
                    value less than 900.
                  </p>
                  <img
                    src={imageChartWidth}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Setting stacked bars option
                  </h4>
                  <p>
                    If you have chosen a bar chart, the stacked bar checkbox
                    will become available
                  </p>
                  <p>By default the stacked option is off</p>
                  <img
                    src={imageChartStackedOff}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                  <p>Selecting 'Stacked bars'</p>
                  <img
                    src={imageChartStackedOn}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={5}
              stepHeading="X Axis (major axis)"
              open={step === 5}
            >
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    Select data to show on axis
                  </h4>
                  <p>By default the data will be grouped by time period.</p>
                  <img
                    src={imageChartXGroupTime}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>
                    If the time period is not appropriate, e.g. when comparing
                    locations, then alernative options can be selected from the
                    drop down box.
                  </p>
                  <img
                    src={imageChartXGroupLocation}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Size of axis (px)</h4>
                  <p>This option is set to 50px as default.</p>
                  <p>The height can be adjusted if necessary.</p>
                  <img
                    src={imageChartXAxisSize}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Show grid lines</h4>
                  <p>This option is selected as default.</p>
                  <p>
                    Uncheck this option if you wish to remove the x axis grid
                    lines from your chart.
                  </p>
                </li>
                <li>
                  <h4 className="govuk-heading-s">Show axis labels</h4>
                  <p>This option is selected as default.</p>
                  <p>
                    Uncheck this option if you wish to remove the x axis labels.
                  </p>
                </li>
                <li>
                  <h4 className="govuk-heading-s">Display type</h4>
                  <p>
                    By default 'automatic' is pre-selected, this will show an
                    axis label for each point on the chart.
                  </p>
                  <img
                    src={imageChartXAutomatic}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                  <p>
                    Select 'Start and end only', to show only the start and end
                    values on the axis.
                  </p>
                  <img
                    src={imageChartXStartEnd}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                  <p>
                    Select 'custom', and add a value to the 'Every nth value'
                    input. The example shown below displays a label on the axis
                    for every 2nd value.
                  </p>
                  <img
                    src={imageChartXCustom}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Sorting</h4>
                  <p>
                    Choose how to order the values in the chart by selecting an
                    indicator from the 'Sort data by' select box.
                  </p>
                  <p>
                    You can choose to have the results shown in ascending or
                    decending order by toggling the 'Sort ascending' checkbox
                    either on or off.
                  </p>
                  <img
                    src={imageChartXSort}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Limiting data</h4>
                  <p>
                    If you only want to show part of a range of results you can
                    limit the data shown in the chart by selecting from the
                    'Minimum' and 'Maximum' select boxes
                  </p>
                  <img src={imageChartXLimitData} width="300" alt="" />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Reference lines</h4>
                  <p>
                    A reference line can be set on the chart as shown in the
                    screenshot below.
                  </p>
                  <img
                    src={imageChartXRefLine}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>
                    Select the position for placement on the x-axis from the
                    drop down options, then provide a label in the text input
                    box.
                  </p>
                  <img
                    src={imageChartXRefLineConfig}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={6}
              stepHeading="Y Axis (minor axis)"
              open={step === 6}
            >
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">Size of axis (px)</h4>
                  <p>This option is set to 50px as default.</p>
                  <p>The width can be adjusted if necessary.</p>
                  <img
                    src={imageChartYAxisSize}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Override displayed unit</h4>
                  <p>
                    This option allow a unit to be appended to the end of the
                    axis labels
                  </p>
                  <img
                    src={imageChartYUnit}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Axis range</h4>
                  <p>Change the minimum and maximum values if necessary.</p>
                  <img
                    src={imageChartYAxisDefault}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                  <p> Adjusted range shown below.</p>
                  <img
                    src={imageChartYAxisAdjusted}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={7}
              stepHeading="Choose an infographic as an alternative"
              open={step === 7}
            >
              <h3>Before you start</h3>
              <p>Please try to create a chart using the steps above.</p>
              <p>
                If you are unable to create the correct style of chart required
                you can upload an image as an alternative, by following the
                steps below.
              </p>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    Select 'Choose an infographic as an alternative' link
                  </h4>
                  <img
                    src={imageInfographicLink}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Select an image, name the file and upload
                  </h4>
                  <p>Image files include:</p>
                  <ul className="govuk-list govuk-list--bullet">
                    <li className="govuk-!-margin-bottom-0">.png</li>
                    <li className="govuk-!-margin-bottom-0">.jpg</li>
                    <li className="govuk-!-margin-bottom-0">.gif</li>
                  </ul>
                  <img
                    src={imageInfographicSelect}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Configure image size to fit to page
                  </h4>
                  <p>
                    Alter the height value so the infographic is positioned
                    correctly in the preview pane.
                  </p>
                  <p>
                    Large images may break out of the page boundary, so it is
                    important to reduce the image dimensions if this is the
                    case.
                  </p>
                  <img
                    src={imageInfographicLarge}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>Adjusted image shown below:</p>
                  <img
                    src={imageInfographicResized}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Save chart options</h4>
                  <p>
                    Once you are happy with your infographic, click the green
                    'Save chart options' button.
                  </p>
                  <h3>Don't</h3>
                  <p>
                    Don't worry you need to make any changes, you can come back
                    and edit the infographic at any point, by following the
                    steps above again.
                  </p>
                </li>
              </ul>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationManageDataBlock;
