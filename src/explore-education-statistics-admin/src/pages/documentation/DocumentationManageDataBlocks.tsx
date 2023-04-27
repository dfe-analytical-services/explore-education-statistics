import Link from '@admin/components/Link';
import { RouteChildrenProps } from 'react-router';
import Page from '@admin/components/Page';
import React from 'react';
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
import imageDataBlockSource from './images/guidance/guidance-datablocks-source.png';
import imageDataBlockName from './images/guidance/guidance-datablocks-name.png';
import imageDataBlockViewEdit from './images/guidance/guidance-datablocks-view-edit.png';
import imageDataBlockEditTable from './images/guidance/guidance-datablocks-edit-table.png';
import imageDataBlockViewChart from './images/guidance/guidance-datablocks-view-charts.png';
import imageDataBlockDelete from './images/guidance/guidance-datablocks-delete.png';

const DocumentationManageDataBlock = ({ location: _ }: RouteChildrenProps) => {
  // TODO: clean this up
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Managing data blocks and creating tables and charts' },
      ]}
      title="Managing data blocks and creating tables and charts"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step-by-step guidance</span>
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
                <Link to="/documentation/manage-data">
                  Managing data: step by step
                </Link>
                .
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To start the process of creating a data block for your
                    release so you can create tables and charts - select ‘Create
                    new data block’ from the dropdown select box.
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
                    ‘Choose a subject’ subheading, select your chosen data set
                    from the list and select the green ‘Next step’ button.
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
                    one location and click the green ‘Next step’ button.
                  </h4>
                  <p>
                    If you make a mistake and want to change your filter choice
                    - select the grey ‘Previous step’ button at the bottom of
                    each section.
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
                    and end time period and click the green ‘Next step’ button.
                  </h4>
                  <p>
                    If you make a mistake and want to change your filter choice
                    - select the grey ‘Previous step’ button at the bottom of
                    each section.
                  </p>
                  <img
                    src={imageDataBlockTimePeriod}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Under the ‘Choose your filters’ subheading - select at least
                    one checkbox from each category and select the green ‘Create
                    data block’ button.
                  </h4>
                  <p>
                    If you don't select a filter for a particular category,
                    'Total' shall be automatically selected when creating your
                    table.
                  </p>
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
                    table, based on the data and filters you’ve chosen.
                  </h4>
                  <img
                    src={imageDataBlockViewSave}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Add a plain English title for your data block into the open
                    text field under ‘Data block name’.
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
                    ‘Source’.
                  </h4>
                  <p>
                    The source you enter will appear underneath the table or
                    chart associated with the data block.
                  </p>
                  <img
                    src={imageDataBlockSource}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Finally, add a name for your data block into the open text
                    field under ‘Name data block’ and then click the green
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
                <li>
                  <h4 className="govuk-heading-s">
                    After saving your data block, the 'Configure content' tab
                    will appear, this will allow you to add a chart if required.
                  </h4>
                  <p>
                    For detailed guidance on how to configure charts within your
                    release{' '}
                    <Link to="/documentation/configure-charts">
                      Configuring charts: step by step
                    </Link>
                  </p>
                  <img
                    src={imageDataBlockCreateChart}
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
                <Link to="/documentation/configure-charts">
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
                    select your saved data block name from the dropdown select
                    box.
                  </h4>
                  <p>
                    Select the 'Configure content' tab, this will show you a
                    view of the data block table, and also the option to create
                    a chart if required.
                  </p>
                  <img
                    src={imageDataBlockViewEdit}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To edit the table data - select the 'Update data source'
                    tab.
                  </h4>
                  <p>
                    This will return you to the data block configuration. Click
                    on a 'Go to this step' link if you want to change anything
                    in the data block configuration.
                  </p>
                  <p>
                    Once you have made your changes and recreated the table,
                    click the 'Save' button at the bottom of the page.
                  </p>
                  <img
                    src={imageDataBlockEditTable}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To view a chart - select the 'Configure content' tab, then
                    select ‘Chart’ tab.
                  </h4>
                  <p>
                    To edit a chart, make changes to the inputs available below
                    the chart preview.
                  </p>

                  <p>
                    Once you have made your changes select the 'Save chart
                    options' button at the bottom of the page.
                  </p>
                  <p>
                    For detailed guidance on how to configure charts within your
                    release{' '}
                    <Link to="/documentation/configure-charts">
                      Configuring charts: step by step
                    </Link>
                  </p>
                  <img
                    src={imageDataBlockViewChart}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
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
            </StepNavItem>
            {/* <StepNavItem
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
            </StepNavItem>*/}
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
                    under Select saved tables and charts.
                  </h4>
                  <p>
                    Please note if you choose to delete a data block, it will
                    also be removed from any instance where it has already been
                    embedded within a release.
                  </p>
                  <img
                    src={imageDataBlockDelete}
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
