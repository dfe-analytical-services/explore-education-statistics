import React from 'react';
import Link from '@admin/components/Link';
import { RouteChildrenProps } from 'react-router';
import Page from '@admin/components/Page';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imageDataBlockCreate from './images/guidance/guidance-datablocks-create.png';
import imageDataBlockDataset from './images/guidance/guidance-datablocks-dataset.png';
import imageDataBlockLocation from './images/guidance/guidance-datablocks-location.png';
import imageDataBlockTimePeriod from './images/guidance/guidance-datablocks-time-period.png';
import imageDataBlockFilters from './images/guidance/guidance-datablocks-filters.png';
import imageDataBlockViewSave from './images/guidance/guidance-datablocks-view-save.png';
import imageDataBlockCreateChart from './images/guidance/guidance-datablocks-create-chart.png';
import imageDataBlockTitle from './images/guidance/guidance-datablocks-title.png';
import imageDataBlockSourceFootnotes from './images/guidance/guidance-datablocks-source-footnotes.png';
import imageDataBlockName from './images/guidance/guidance-datablocks-name.png';
import imageDataBlockViewEdit from './images/guidance/guidance-datablocks-view-edit.png';
import imageDataBlockViewTable from './images/guidance/guidance-datablocks-view-table.png';
import imageDataBlockEditTable from './images/guidance/guidance-datablocks-edit-table.png';
import imageDataBlockViewChart from './images/guidance/guidance-datablocks-view-charts.png';
import imageDataBlockViewPermalink from './images/guidance/guidance-datablocks-view-permalink.png';
import imageDataBlockCopyPermalink from './images/guidance/guidance-datablocks-copy-permalink.png';
import imageDataBlockCopyChartPermalink from './images/guidance/guidance-datablocks-copy-chart-permalink.png';
import imageDataBlockDelete from './images/guidance/guidance-datablocks-delete.png';
import imageDataBlockDeleteTable from './images/guidance/guidance-datablocks-delete-table.png';
import imageDataBlockDeleteChart from './images/guidance/guidance-datablocks-delete-chart.png';
import imageFootnotesEdit from './images/guidance/guidance-footnotes-edit.png';

const DocumentationManageDataBlock = ({ location: _ }: RouteChildrenProps) => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Managing data blocks and creating tables and charts' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step by step guidance</span>
            <h1 className="govuk-heading-xl">
              Managing data blocks and creating tables and charts
            </h1>
          </div>
          <p>
            How to manage data blocks within a release - including creating
            tables and charts.
          </p>{' '}
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Manage data blocks"
              open={step === 1}
            >
              <p>
                You create data blocks by managing the data you’ve already
                uploaded.
              </p>
              <p>
                These data blocks will then be used to create tables and charts
                for your release.
              </p>
              <h3>Before you start</h3>
              <p>
                Make sure your data has uploaded and been processed before you
                try and create any data blocks.
              </p>
              <div className="govuk-warning-text">
                <span className="govuk-warning-text__icon" aria-hidden="true">
                  !
                </span>
                <strong className="govuk-warning-text__text">
                  <span className="govuk-warning-text__assistive">Warning</span>
                  You can’t create data blocks, tables and charts for your
                  release until your data has been uploaded and processed.
                </strong>
              </div>
              <p>
                For detailed guidance on how to add data to your release{' '}
                <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                  Managing data: step by step
                </a>
                .
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To start the process of creating a data block for your
                    release so you can create tables and charts - use the
                    ‘Create data blocks’ tab.
                  </h4>
                  <img
                    src={imageDataBlockCreate}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
              <h3>Don't</h3>
              <p>
                Don’t worry if you haven't uploaded all the data to create all
                the data blocks, tables and charts you need to complete your
                release.
              </p>
              <p>
                You can upload more later under the ‘Manage data’ tab. For
                detailed guidance on how to add data to your release{' '}
                <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                  Managing data: step by step
                </a>
                .
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={2}
              stepHeading="Create data blocks, tables and charts"
              open={step === 2}
            >
              <p>
                You need to choose a data set and then manage it using filters
                to first create a data block which is then used to create tables
                and charts.
              </p>
              <p>
                The data and filter options you’ll see are taken from the data
                you uploaded under the ‘Manage data’ tab.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To choose the data you want to use to create your initial
                    data block (and subsequent tables and charts) - under the
                    ‘Choose data’ subheading, select your chosen data set from
                    the list and select the green ‘Next step’ button.
                  </h4>
                  <img
                    src={imageDataBlockDataset}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Under the ‘Choose locations’ subheading - select at least
                    one location and select the green ‘Next step’ button.
                  </h4>
                  <p>
                    If you make a mistake and want to change your filter choice
                    - select the grey ‘Previous step’ button at the bottom of
                    each section.{' '}
                  </p>
                  <img
                    src={imageDataBlockLocation}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Under the ‘Choose time period’ subheading - choose a start
                    and end time period and select the green ‘Next step’ button.
                  </h4>
                  <p>
                    If you make a mistake and want to change your filter choice
                    - select the grey ‘Previous step’ button at the bottom of
                    each section.{' '}
                  </p>
                  <img
                    src={imageDataBlockTimePeriod}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Under the ‘Choose filters’ subheading - select at least one
                    checkbox from each category and select the green ‘Create
                    data block’ button.
                  </h4>
                  <p>
                    If you make a mistake and want to change your filter choice
                    - select the grey ‘Previous step’ button at the bottom of
                    each section.
                  </p>
                  <img
                    src={imageDataBlockFilters}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    The data block you’ve created automatically generates a
                    table, based on the data and filters you’ve chosen,under the
                    ‘View and save data block’ subheading and its ‘Table’ tab.
                  </h4>
                  <img
                    src={imageDataBlockViewSave}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To create charts for your release - use the ‘Create chart’
                    tab and select the type of chart you want to create.
                  </h4>
                  <p>
                    For detailed guidance on how to configure charts within your
                    release{' '}
                    <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                      Configuring charts: step by step
                    </a>{' '}
                  </p>
                  <p>
                    If you can’t create the type of chart you’re looking for -
                    you can upload an infographic as an alternative by selecting
                    the ‘Choose an infographic as alternative’ link.
                  </p>
                  <img
                    src={imageDataBlockCreateChart}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Add a plain English title for your data block into the open
                    text field under ‘Data block title’.
                  </h4>
                  <p>
                    This will be shown to users to identify the data block
                    within the content of your release.
                  </p>
                  <p>
                    You should also use this title when referring to the data
                    file within the content of your release.
                  </p>
                  <img
                    src={imageDataBlockTitle}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Add source information into the open text field under
                    ‘Source’ and add any release footnotes into the open text
                    box under ‘Release footnotes’.
                  </h4>
                  <p>
                    Your release footnotes should include details which will
                    help tell the story of your statistics and data to
                    non-expert users.
                  </p>
                  <img
                    src={imageDataBlockSourceFootnotes}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Finally, add a name for your data block into the open text
                    field under ‘Name data block’ and then select the green
                    ‘Save’ button.
                  </h4>
                  <p>
                    This data block name will help you identify it from within
                    your list of data blocks for your release which you’ll see
                    under the ‘View saved tables and charts’ tab at the top of
                    the page.
                  </p>
                  <img
                    src={imageDataBlockName}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
              <h3>Don't</h3>
              <ul className="govuk-list govuk-list--number">
                <li>
                  Don’t worry if you haven't uploaded all the data to create all
                  the data blocks and charts you need to complete your release.
                  You can upload more under the ‘Manage data’ tab.
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you have any issues creating data blocks, tables and charts
                contact:
              </p>
              <h4>Explore education statistics team </h4>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Next steps</h3>
              <p>
                For detailed guidance on how to configure charts within your
                release -{' '}
                <Link to="/documentation/chartConfigure">
                  Configuring charts: step by step
                </Link>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={3}
              stepHeading=" View and edit tables and charts"
              open={step === 3}
            >
              <p>
                You can view and edit any tables and charts you’ve already
                created.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To view and edit the tables and charts for your release -
                    use the ‘View saved tables and charts’ tab and select its
                    name from the dropdown menu under ‘Select saved tables and
                    charts.
                  </h4>
                  <img
                    src={imageDataBlockViewEdit}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To view a table - use the ‘Table’ tab. To edit a table -
                    select the green ‘Edit table’ button.
                  </h4>
                  <img
                    src={imageDataBlockViewTable}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To edit the table data - select the ‘Change’ links from the
                    right-hand side of the page, change your category and
                    indicators selections from under the ‘Choose filters’
                    subheading and select the green ‘Create data block’ button
                    to save your changes.
                  </h4>
                  <img
                    src={imageDataBlockEditTable}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To view a chart - use the ‘Chart’ tab. To edit a chart -
                    select the green ‘Edit chart’ button.
                  </h4>
                  <img
                    src={imageDataBlockViewChart}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To edit the data within the data block which has been used
                    to create your chart - select the ‘Change’ links from the
                    right-hand side of the page.
                  </h4>
                  <img
                    src={imageFootnotesEdit}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To change the configuration of your chart - use the ‘Charts’
                    tab to view the chart you want to edit and reconfigure it by
                    changing the available options.
                  </h4>
                  <p>
                    For detailed guidance on how to configure charts within your
                    release -{' '}
                    <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                      Configuring charts: step by step
                    </a>
                    .
                  </p>
                  <img
                    src={imageFootnotesEdit}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <h3>Help and support</h3>
                  <p>
                    If you have any issues viewing and editing tables and charts
                    contact:
                  </p>
                  <h4>Explore education statistics team </h4>
                  <p>
                    Email <br />
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                  </p>
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={4}
              stepHeading="Use permalinks"
              open={step === 4}
            >
              <p>
                You can use permalinks to send to people so they can see a
                standalone version of a table or chart.
              </p>
              <p>
                You should also use these kinds of links to refer to a table or
                chart within the content of your release.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To find a permalink for tables or charts - use the ‘View
                    saved tables and charts’ tab and select its name from the
                    dropdown menu under ‘Select saved tables and charts'.
                  </h4>
                  <img
                    src={imageDataBlockViewPermalink}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To copy a permalink for a table - use the ‘Table’ tab and
                    copy the link from within the text field under the
                    ‘Permalink’ subheading.
                  </h4>
                  <img
                    src={imageDataBlockCopyPermalink}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To copy a permalink for a chart - use the ‘Chart’ tab and
                    copy the link from within the text field under the
                    ‘Permalink’ subheading.
                  </h4>
                  <img
                    src={imageDataBlockCopyChartPermalink}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>If you have any issues using permalinks contact: </p>
              <strong>Explore education statistics team </strong>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={5}
              stepHeading="Delete tables and charts"
              open={step === 5}
            >
              <p>
                You can delete any tables and charts you’ve already created.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To delete any tables and charts - use the ‘View saved tables
                    and charts’ tab and select its name from the dropdown menu
                    under ‘Select saved tables and charts.
                  </h4>
                  <img
                    src={imageDataBlockDelete}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To delete a table - use the ‘Table’ tab and select the red
                    ‘Delete table’ button.
                  </h4>
                  <img
                    src={imageDataBlockDeleteTable}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To delete a chart - use the ‘Chart’ tab and select the red
                    ‘Delete chart’ button.
                  </h4>
                  <img
                    src={imageDataBlockDeleteChart}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <h3>Help and support</h3>
                <p>
                  If you have any issues deleting tables and charts contact:
                </p>
                <h4>Explore education statistics team </h4>
                <p>
                  Email <br />
                  <a href="mailto:explore.statistics@education.gov.uk">
                    explore.statistics@education.gov.uk
                  </a>
                </p>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={6}
              stepHeading="Next steps - manage content"
              open={step === 6}
            >
              <p>
                Once you’ve created the data blocks, tables and charts you can
                add them to your release along with the rest of your content.
              </p>
              <h3>Next steps</h3>
              <p>
                For detailed guidance on how to view and edit content and add,
                view and resolve comments -{' '}
                <Link to="./manage-content">
                  Managing content: step by step
                </Link>
                .
              </p>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationManageDataBlock;
