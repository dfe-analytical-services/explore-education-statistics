import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Page from '@admin/components/Page';
import Link from '@admin/components/Link';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imageChoosePublication from './images/guidance/guidance-publication-select.png';
import imageCreateReleaseButton from './images/guidance/guidance-create-release-button.png';
import imageCreateReleaseEditSummary from './images/guidance/guidance-create-release-edit-summary.png';
import imageCreateReleasePage from './images/guidance/guidance-create-release-page.png';
import imageCreateReleaseDataTab from './images/guidance/guidance-data-tab.png';
import imageCreateReleaseFootnotesTab from './images/guidance/guidance-footnotes-tab.png';
import imageCreateReleaseFileTab from './images/guidance/guidance-file-tab.png';
import imageCreateReleaseDatablockCreate from './images/guidance/guidance-datablocks-create.png';
import imageCreateReleaseDatablockSaved from './images/guidance/guidance-datablocks-configure.png';
import imageCreateReleaseManageContent from './images/guidance/guidance-content-edit.png';
import imageSelectTheme from './images/guidance/guidance-publication-select-theme.png';

const DocumentationCreateNewRelease = ({ location: _ }: RouteChildrenProps) => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Creating a new release' },
      ]}
      title="'Creating a new release"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step-by-step guidance</span>
            <h1 className="govuk-heading-xl">Creating a new release</h1>
          </div>
          <p>
            How to create a new release - including uploading data and files and
            creating data blocks (i.e. tables and charts) and content.
          </p>
          <h2>Before you start</h2>
          <p>
            You can create a release in any order but need to make sure your:
          </p>{' '}
          <ul className="govuk-list govuk-list--bullet">
            <li>
              data is in the required CSV format set out in our{' '}
              <a href="https://drive.google.com/file/d/15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob/view">
                Data standards guide
              </a>
            </li>
            <li>
              content is written and formatted to the standards set out in our{' '}
              <Link to="./content-design-standards-guide">
                Content design standards guide
              </Link>{' '}
              and <Link to="style-guide">Content design style guide</Link>
            </li>
          </ul>
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Create a new release"
              open={step === 1}
            >
              <p>
                Anyone can create a new release under their assigned list of
                publications.
              </p>
              <p>
                Once you’ve created a new release, you and the members of your
                ‘Production team’ will be responsible for putting the release
                together via the admin system.
              </p>
              <h3>Do</h3>

              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    Under the 'Manage publications and releases' tab on your
                    administration dashboard, select the publication where you
                    want to create your release.
                  </h4>
                  <img
                    src={imageChoosePublication}
                    className="govuk-!-width-three-quarters"
                    alt="Choose a publication"
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    If you have access to a long list of publications, use the
                    ‘Select theme’ and ‘Select topic’ dropdowns to find a
                    publication.
                  </h4>
                  <img
                    src={imageSelectTheme}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Under the publication you’ve chosen, click the green 'Create
                    new release' button.
                  </h4>
                  <img
                    src={imageCreateReleaseButton}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Fill out the Create new release page.
                  </h4>
                  <img
                    src={imageCreateReleasePage}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>
                    Select the time period and enter the year for the release.
                    If your release is spread over two years, just enter the
                    first year (i.e. if your release is for 2020/21 enter 2020).
                  </p>
                  <p>
                    The Scheduled publish date is the date that the release will
                    be published for the public to read.
                  </p>
                  <p>
                    Entering a Next release expected date is optional. You don't
                    need to enter a specific day - you can enter just a year, or
                    a month and a year.
                  </p>
                  <p>Choose the Release type for your release.</p>
                  <p>
                    If you have previously created releases under the same
                    publication, the Select template options will appear. Select
                    "Copy existing template" if you wish to use the content
                    sections from a previous release.
                  </p>
                  <p>
                    Once you've filled in all sections, click the 'Create new
                    release' button.
                  </p>
                </li>
              </ul>
              <h3>Don't</h3>
              <p>
                Don’t worry if you haven't got all the data, files and content
                to complete your release.
              </p>
              <p>
                Don't worry if you've made a mistake when creating your release.
                You can change details about the release later.
              </p>
              <h3>Help and support</h3>
              <p>
                If you can't find the publication where you want to create your
                release or created a release in error and want to delete it
                contact:{' '}
              </p>
              <h4>Explore education statistics team </h4>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={2}
              stepHeading="Release summary"
              open={step === 2}
            >
              <p>
                The details you enter will be used to identify your release and
                help users find your statistics and data.
              </p>
              <h3>Do</h3>
              <p>
                Use the main ‘Release summary’ tab across the top of your page
                to make sure you:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  set the scheduled publish date for your release - you can’t
                  create a release without one
                </li>
                <li>
                  check all your release summary details are correct - including
                  the members of your ‘Production team’ so they can help create
                  and review your release
                </li>
              </ul>
              <p>
                To update details (including the scheduled publish date) - use
                the ‘Edit release summary’ link.
              </p>
              <img
                src={imageCreateReleaseEditSummary}
                className="govuk-!-width-three-quarters"
                alt=""
              />
              <p>
                If you need to add or remove members of your ‘Production Team’
                or individual details need updating contact:
              </p>
              <h4>Explore education statistics team </h4>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Don't</h3>
              <p>
                Don’t worry if you can't provide an 'Expected next release
                date'. You can come back and add it later.
              </p>

              <h3>Help and support</h3>
              <p>
                If you have any issues about details listed in the release
                summary contact:
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={3}
              stepHeading="Manage data"
              open={step === 3}
            >
              <p>
                Users will be able to access and download any data and files you
                upload to your release.
              </p>
              <h3>Before you start</h3>
              <p>
                Make sure your data is in the required csv format set out in our{' '}
                <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                  Underlying data standards guide
                </a>
                .
              </p>
              <p>
                If your data doesn’t meet these standards, you won’t be able to
                upload it to your release.
              </p>
              <p>
                Any footnotes you add under the ‘Manage data’ tab will appear
                when users use your data to create tables using the ‘table tool’
                in the live service.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To upload any data - select the ‘Data uploads’ tab under the
                    main ‘Manage data’ tab across the top of the page.
                  </h4>
                  <img
                    src={imageCreateReleaseDataTab}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To add footnotes to your data - use the ‘Footnotes’ tab.
                  </h4>
                  <img
                    src={imageCreateReleaseFootnotesTab}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To upload any files you want to users to download from your
                    release (for example, infographics or images of more complex
                    tables) - use the ‘Ancillary file uploads’ tab.
                  </h4>
                  <img
                    src={imageCreateReleaseFileTab}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
              <h3>Don't</h3>
              <p>
                Don’t upload any sensitive data or files. Only upload data and
                files that are suitable for the public domain.
              </p>
              <p>
                Don’t worry if you haven't got all the data and files to
                complete your release. You can come back and add more later.
              </p>
              <h3>Help and support</h3>
              <p>
                If you have any issues uploading data and files to your release,
                adding footnotes to data or questions about data standards
                contact:
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Next steps</h3>
              <p>
                For detailed guidance on how to upload data and files and add
                footnotes to your release -{' '}
                <Link to="./manage-data">Managing data: step by step.</Link>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={4}
              stepHeading="Manage data blocks"
              open={step === 4}
            >
              <p>
                You need to create data blocks so you can create tables and
                charts for your release.
              </p>
              <h3>Before you start</h3>
              <p>
                Make sure your data has uploaded and been processed before you
                try and create any data blocks under the ‘Manage data blocks’
                tab.
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
                The service will let you know when your data has been processed.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To start the process of creating a data block (so you can
                    then create tables and charts), select the ‘Create data
                    blocks’ dropdown option under the main ‘Manage data blocks’
                    tab across the top of the page.
                  </h4>
                  <img
                    src={imageCreateReleaseDatablockCreate}
                    className="govuk-!-width-three-quarters"
                    alt="Choose a publication"
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To view the data blocks you’ve already created and saved for
                    your release - use the top dropdown under the 'Manage data
                    blocks' tab.
                  </h4>
                  <img
                    src={imageCreateReleaseDatablockSaved}
                    className="govuk-!-width-three-quarters"
                    alt="Choose a publication"
                  />
                </li>
              </ul>
              <h3>Don't</h3>
              <p>
                Don’t worry if you haven't uploaded all the data to create the
                data blocks, tables and charts you need to complete your
                release. This doesn't need to be done all at once.
              </p>
              <p>
                You can upload more later under the ‘Manage data’ tab. For
                detailed guidance on how to add data to your release -{' '}
                <Link to="./manage-data">Managing data: step by step</Link>.
              </p>
              <h3>Help and support</h3>
              <p>
                If you have any issues creating data blocks, tables and charts
                contact:
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Next steps</h3>
              <p>
                For detailed guidance on how to create data blocks, tables and
                charts for your release -{' '}
                <Link to="./manage-data-block">
                  Managing data blocks and creating tables and charts: step by
                  step
                </Link>
                .
              </p>
              <p>
                For detailed guidance on how to configure charts for your
                release -{' '}
                <Link to="./manage-data-block">
                  Configure charts: step by step
                </Link>
                .
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={5}
              stepHeading="Manage content"
              open={step === 5}
            >
              <p>
                Make sure your content is written and formatted to the standards
                set out in our{' '}
                <Link to="./content-design-standards-guide">
                  Content design standards guide
                </Link>{' '}
                and <a href="style-guide">Content design style guide</a>{' '}
              </p>
              <p>
                Make sure you familiarise yourself with these standards to make
                sure your release allows users to simply and quickly understand
                your statistics and data.
              </p>
              <h3>Before you start</h3>
              <p>
                Make sure you and your Production team have decided and
                understand who’s responsible for managing (i.e. creating and
                editing) the different sections of your release.
              </p>
              <h3>Do</h3>
              <p>
                To create and edit any content - under the main ‘Manage content’
                tab across the top of the page, select the ‘Edit content’ radio
                button.
              </p>
              <img
                src={imageCreateReleaseManageContent}
                className="govuk-!-width-three-quarters"
                alt=""
              />
              <h3>Don't</h3>
              <p>
                Don’t worry if you haven't got all the content to complete your
                release. You can come back and add it later.
              </p>
              <h3>Help and support</h3>
              <p>
                If you have any issues creating, editing and updating content or
                adding, viewing and resolving comments contact:
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
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
            <StepNavItem
              stepNumber={6}
              stepHeading="Update release status"
              open={step === 6}
            >
              <p>
                Once a release has been created it automatically moves into ‘In
                draft’ status.
              </p>
              <p>
                If you and the members of your ‘Production team’ are happy with
                the content of your release, discuss whether to update its
                status to ‘Ready for sign-off’ and approve it for sign-off and
                ‘Higher review’.
              </p>
              <h3>Next steps</h3>
              <p>
                For detailed guidance on how to update release status -{' '}
                <Link to="./edit-release">
                  Editing a release and updating release status: step by step
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

export default DocumentationCreateNewRelease;
