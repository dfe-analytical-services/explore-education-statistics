import Link from '@admin/components/Link';
import { RouteChildrenProps } from 'react-router';
import Page from '@admin/components/Page';
import React from 'react';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imageDataTab from './images/guidance/guidance-data-tab.png';
import imageDataChooseFile from './images/guidance/guidance-data-choose-file.png';
import imageDataChooseMetaFile from './images/guidance/guidance-data-choose-meta-file.png';
import imageDataTitle from './images/guidance/guidance-data-title.png';
import imageDataUpload from './images/guidance/guidance-data-upload.png';
import imageDataCurrentData from './images/guidance/guidance-data-current-data.png';
import imageDataActions from './images/guidance/guidance-data-actions.png';
import imageDataDelete from './images/guidance/guidance-data-delete.png';
import imageFootnotesTab from './images/guidance/guidance-footnotes-tab.png';
import imageFootnotesSelect from './images/guidance/guidance-footnotes-select.png';
import imageFootnotesConfig from './images/guidance/guidance-footnotes-config.png';
import imageFootnotesSave from './images/guidance/guidance-footnotes-save.png';
import imageFootnotesSummary from './images/guidance/guidance-footnotes-summary.png';
import imageFootnotesEdit from './images/guidance/guidance-footnotes-edit.png';
import imageFileTab from './images/guidance/guidance-file-tab.png';
import imageFileUpload from './images/guidance/guidance-file-choose.png';
import imageFileTitle from './images/guidance/guidance-file-title.png';
import imageFileSave from './images/guidance/guidance-file-save.png';
import imageFileSummary from './images/guidance/guidance-file-summary.png';
import imageFileDelete from './images/guidance/guidance-file-delete.png';

const DocumentationManageContent = ({ location: _ }: RouteChildrenProps) => {
  // TODO: clean this up
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Managing data' },
      ]}
      title="Managing data"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step by step guidance</span>
            <h1 className="govuk-heading-xl">Managing data</h1>
          </div>
          <p>
            How to manage data for and within a release - including preparing
            data and adding data, other files and footnotes.
          </p>
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Manage and prepare data"
              open={step === 1}
            >
              <p>
                Make sure your data is in the required CSV format set out in our{' '}
                <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                  Underlying data standards guide
                </a>
                .
              </p>
              <p>
                This guide will help you create accessible and consistent data
                and provide users with the underlying data for your statistics.
              </p>
              <div className="govuk-warning-text">
                <span className="govuk-warning-text__icon" aria-hidden="true">
                  !
                </span>
                <strong className="govuk-warning-text__text">
                  <span className="govuk-warning-text__assistive">Warning</span>
                  If your data doesn’t meet these standards, you won’t be able
                  to upload it to your release.
                </strong>
              </div>
              <h3>Before you start</h3>
              <p>
                Make sure you and the members of your production team understand
                who’s responsible for the data within your release.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  make sure your data files have passed the checks in our{' '}
                  <a href="https://rsconnect/rsc/dfe-published-data-qa/">
                    screening app
                  </a>
                </li>
                <li>
                  if your data doesn’t meet these standards, you won’t be able
                  to upload it to your release
                </li>
                <li>
                  if you have any issues uploading data and files, or questions
                  about data standards contact:{' '}
                  <a href="mailto:explore.statistics@education.gov.uk">
                    explore.statistics@education.gov.uk
                  </a>
                </li>
              </ul>
              <h3>Do</h3>
              <p>
                Make sure any data and metadata files titles you add stick to
                the following naming convention:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <strong>for data files - </strong> file_name.csv
                </li>
                <li>
                  <strong>for metadata files - </strong> file_name.meta.csv
                </li>
              </ul>
              <h3>Don't</h3>
              <p>
                Don’t worry if you haven't got all the data to complete your
                release. You can come back and add more later.
              </p>

              <h3>Help and support</h3>
              <p>
                If you have any issues preparing your data or questions about
                data standards contact:{' '}
              </p>
              <strong>Explore education statistics team </strong>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={2}
              stepHeading="Upload data"
              open={step === 2}
            >
              <p>
                Users will be able to access and download any data you upload to
                your release. That data will:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  serve as the accessible underlying data for your statistics
                  and release
                </li>
                <li>
                  be used to create the data blocks, tables and charts for your
                  release
                </li>
              </ul>
              <h3>Before you start</h3>
              <p>
                You have to upload each data file one at a time. Any data file
                you upload will require a corresponding metadata file.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To upload any data - use the ‘Data uploads’ tab.
                  </h4>
                  <img
                    src={imageDataTab}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To choose the data you want to upload from your computer -
                    click the grey ‘Choose File’ button under ‘Upload data’.
                  </h4>
                  <img
                    src={imageDataChooseFile}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>
                    The name of your data file should be based on the name of
                    your statistics.
                  </p>
                  <p>
                    For example, the characteristic data for absence statistics
                    would be:
                  </p>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>absence_by_characteristic.csv</li>
                  </ul>
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To choose the corresponding metadata file for your data -
                    select the grey ‘Choose File’ button under ‘Upload
                    metadata’.
                  </h4>
                  <img
                    src={imageDataChooseMetaFile}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>
                    The name of your metadata file should be based on the name
                    of your data with ‘.meta’ added to the end.
                  </p>
                  <p>
                    For example, the metadata name for the absence statistics
                    data would be:
                  </p>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>absence_by_characteristic.meta.csv</li>
                  </ul>
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Add a plain English title for your data file into the open
                    text field under ‘Add data title’.
                  </h4>
                  <p>
                    This will be shown to users to identify the data file within
                    the content of your release.
                  </p>
                  <div className="govuk-inset-text">
                    Don’t include the _ and . symbols or .csv in this title.
                  </div>
                  <p>
                    You should also use this title when referring to the data
                    file within the content of your release.
                  </p>
                  <img
                    src={imageDataTitle}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To upload your chosen data and metadata files to your
                    release - click the green ‘Upload data’ at the bottom of the
                    page.
                  </h4>
                  <p>
                    Depending on your internet connection speed, your data may
                    take some time to upload.
                  </p>
                  <p>
                    You can carry on creating or editing the rest of the release
                    while your data uploads.
                  </p>
                  <img
                    src={imageDataUpload}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Once your data and metadata files have uploaded to your
                    release, their details will appear under the ‘Current data
                    for this release’ section of the page.
                  </h4>
                  <img
                    src={imageDataCurrentData}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To delete any of your data, click the 'Delete files' link.
                  </h4>
                  <img
                    src={imageDataActions}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">Confirm deletion</h4>
                  <p>
                    A confirmation window will appear, this will also alert you
                    to any instances where this data is already being used in
                    your release.
                  </p>
                  <p>Click the green 'Confirm' button to delete the files. </p>
                  <img
                    src={imageDataDelete}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  {' '}
                  <h4 className="govuk-heading-s">
                    To upload more data files repeat steps 2 to 5 above
                  </h4>
                </li>
              </ul>
              <h3>Don't</h3>
              <ul className="govuk-list govuk-list--number">
                <li>
                  Don’t upload any sensitive data. Only upload data which is
                  suitable for the public domain.
                </li>
                <li>
                  Don’t close your browser or browser tab or window until your
                  data is queued, otherwise the upload will fail.
                </li>
                <li>
                  Don’t worry if you haven't got all the data to complete your
                  release. You can come back and add more later.
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you have any issues uploading data to your release or
                questions about data standards read our{' '}
                <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                  Underlying data standards guide
                </a>{' '}
                or contact:
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
                For detailed guidance on how to use your data to create data
                blocks, tables and charts for your release -{' '}
                <Link to="/documentation/manage-data-block">
                  Managing data blocks and creating tables and charts: step by
                  step.
                </Link>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={3}
              stepHeading="Add footnotes"
              open={step === 3}
            >
              <p>
                Any footnotes you add under the ‘Manage data’ tab will appear
                when users use your data to create tables using the ‘table tool’
                in the live service.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To add footnotes to your data - use the ‘Footnotes’ tab and
                    click the green ‘Add footnotes’ button.
                  </h4>
                  <img
                    src={imageFootnotesTab}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    You will see a list of each uploaded subject, together with
                    any available indicators and filters.
                  </h4>
                  <img
                    src={imageFootnotesSelect}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Use the combination of checkboxes to choose the subjects,
                    indicators and filters under which you want your footnotes
                    to appear for users.
                  </h4>
                  <p>
                    You can select any combination of checkboxes to identify
                    where you want your footnote to appear for users.
                  </p>
                  <p>
                    However, you must select at least one subject, indicator and
                    filter checkbox to add a footnote.
                  </p>
                  <p>
                    You can also choose to select all subjects and indicators
                    using the ‘Select all indicators’ or ‘Select all filters’
                    checkboxes at the bottom of each of these sections.
                  </p>
                  <img
                    src={imageFootnotesConfig}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Add the content of your footnote into the text box at the
                    bottom of the page and then select the green ‘Save’ button.{' '}
                  </h4>
                  <img
                    src={imageFootnotesSave}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Once your footnote has been saved to your release its
                    details will appear on the page.
                  </h4>
                  <img
                    src={imageFootnotesSummary}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To edit or delete any footnotes - click the corresponding
                    green ‘Edit’ or grey ‘Delete’ buttons under the ‘Actions’
                    column.
                  </h4>
                  <img
                    src={imageFootnotesEdit}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To add another footnote - click the green ‘Add another
                    footnote button.
                  </h4>
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={4}
              stepHeading="Upload files"
              open={step === 4}
            >
              <p>You can upload various file types including:</p>
              <ul className="govuk-bullet govuk-bullet--list">
                <li>Plain text: .txt</li>
                <li>Microsoft word: .doc, .docx</li>
                <li>Excel: .xls</li>
                <li>PDF</li>
                <li>Open document format files</li>
                <li>Open XML formats</li>
                <li>Image files: .png, .jpg, .gif</li>
              </ul>
              <p>
                Uploading these kinds of files is optional but users will be
                able to download them from your release.
              </p>
              <h3>Before you start</h3>
              <p>You have to upload each file one at a time.</p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To upload files you want users to view within or download
                    from your release - use the ‘Ancillary file uploads’ tab.
                  </h4>
                  <img
                    src={imageFileTab}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To choose the file you want to upload from your computer -
                    click the grey ‘Choose File’ button under ‘Upload file’.
                  </h4>
                  <img
                    src={imageFileUpload}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>
                    The name of your metadata file should be descriptive and
                    based on the contents of the file.
                  </p>
                  <p>
                    For example, a file showing a complex table about absence
                    statistics would be:
                  </p>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>complex_absence_statistics_table.png</li>
                  </ul>
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Add a title for your data by entering a plain English
                    version of the file into the open text field under ‘Add file
                    title’.
                  </h4>
                  <p>
                    This will be shown to users to identify the file within the
                    content of your release.
                  </p>
                  <p className="govuk-inset-text">
                    Don’t include any symbols or file format types (for example,
                    .png) in this title.
                  </p>
                  <p>
                    You should also use this title when referring to the file
                    within the content of your release.
                  </p>
                  <img
                    src={imageFileTitle}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To upload your chosen files to your release - click the
                    green ‘Upload file’ at the bottom of the page.
                  </h4>
                  <p>
                    Depending on your internet connection speed, your file may
                    take some time to upload.
                  </p>
                  <p>
                    As long as you don't close your browser or browser tab or
                    window, you can carry on creating or editing the rest of the
                    release while your file uploads.
                  </p>
                  <img
                    src={imageFileSave}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    Once your files have uploaded to your release their details
                    will appear under the ‘Current files for this release’
                    section of the page.
                  </h4>
                  <img
                    src={imageFileSummary}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To upload more files repeat steps 2 to 5 above.
                  </h4>
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To view or delete any of your files - select the
                    corresponding links under the ‘Actions’ column.
                  </h4>
                  <img
                    src={imageFileDelete}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
              <h3>Don't</h3>
              <ul className="govuk-list govuk-list--number">
                <li>
                  Don’t upload any sensitive files. Only upload files which are
                  suitable for the public domain.
                </li>
                <li>
                  Don’t close your browser or browser tab or window until your
                  file has uploaded otherwise the upload will fail.
                </li>
                <li>
                  Don’t worry if you haven't got all the files to complete your
                  release. You can come back and add more later.
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you have any issues uploading files to your release contact:{' '}
              </p>
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
              stepHeading="Next steps - manage data blocks and create tables and charts
              "
              open={step === 5}
            >
              <p>
                Once you’ve uploaded your data and files and added you footnotes
                you can create data blocks by managing the data you’ve already
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
              <h3>Next steps</h3>
              <p>
                For more detailed guidance on how to create data blocks, tables
                and charts for your release -{' '}
                <Link to="/documentation/manage-data-block">
                  Managing data blocks and creating tables and charts: step by
                  step.
                </Link>
              </p>
              <p>
                For more detailed guidance on how to configure charts within
                your release -{' '}
                <Link to="/documentation/configure-charts">
                  Configuring charts: step by step.
                </Link>
              </p>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationManageContent;
