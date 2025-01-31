import Page from '@admin/components/Page';
import Link from '@admin/components/Link';
import React from 'react';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';

import imageContentEdit from './images/guidance/guidance-content-edit.png';
import imageCommentAdd from './images/guidance/guidance-content-comments-add.png';
import imageCommentSave from './images/guidance/guidance-content-comment-save.png';
import imageContentAddTextBlock from './images/guidance/guidance-content-add-text.png';
import imageContentEditButton from './images/guidance/guidance-content-edit-button.png';
import imageContentSave from './images/guidance/guidance-content-save.png';
import imageAddDatablock from './images/guidance/guidance-content-add-datablock.png';
import imageEmbedDatablock from './images/guidance/guidance-content-add-embed-data.png';
import imageKeyStatBtn from './images/guidance/guidance-content-keystat-btn.png';
import imageKeyStatSelect from './images/guidance/guidance-content-embed-keystat.png';
import imageKeyStatEdit from './images/guidance/guidance-content-edit-keystat.png';
import imageKeyStatGuidance from './images/guidance/guidance-content-add-guidance.png';
import imageKeyStatMultiple from './images/guidance/guidance-content-add-multiple-stats.png';
import imageRelatedBtn from './images/guidance/guidance-content-add-related.png';
import imageRelatedAdd from './images/guidance/guidance-content-create-related.png';
import imageSectionAdd from './images/guidance/guidance-content-add-section.png';
import imageReorderBtn from './images/guidance/guidance-content-reorder-sections.png';
import imageReorderSection from './images/guidance/guidance-content-reorder-sections-move.png';
import imageContentPreview from './images/guidance/guidance-content-preview.png';
import imageReleaseNote from './images/guidance/guidance-content-release-note.png';

const DocumentationManageContent = () => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Managing content' },
      ]}
      title="Managing content"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step by step guidance</span>
            <h1 className="govuk-heading-xl">Managing content</h1>
          </div>
          <p>
            How to manage content within a release - including adding new and
            editing existing content.
          </p>{' '}
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Manage content"
              open={step === 1}
            >
              <p>
                Make sure your content is written and formatted to the standards
                set out in our{' '}
                <Link to="/documentation/content-design-standards-guide">
                  Content design standards guide
                </Link>{' '}
                and{' '}
                <Link to="/documentation/style-guide">
                  Content design style guide
                </Link>
                .
              </p>
              <p>
                These guides will help you create clear and consistent content
                which will help users understand your statistics and data.
              </p>
              <h3>Before you start</h3>
              <p>
                Make sure you and the members of your production team understand
                who's responsible for the content within the different sections
                of your release.
              </p>
              <h3>Do</h3>
              <p>
                To create and edit any content - click the 'Edit content' radio
                button.
              </p>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    Click the 'Edit content' radio button located in the bottom
                    left of the screen.
                  </h4>
                  <p>
                    This will be selected by default when you enter this page.
                  </p>
                </li>
              </ul>
              <img
                src={imageContentEdit}
                className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
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
                adding, viewing and resolving comments contact:{' '}
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
              stepHeading="Add, view and resolve comments"
              open={step === 2}
            >
              <p>
                You and the members of your production team can add comments to
                and view any comments on any sections of content within your
                release.
              </p>
              <p>
                You can add as many comments as you like but you should try and
                keep them as simple and as straightforward as possible.
              </p>
              <p>
                Any complicated issues related to your release should be
                discussed and resolved outside the admin system's comments
                boxes.
              </p>
              <p>
                Once you've dealt with the issues raised within comments, you
                should record them as either being resolved or remove them if
                they're no longer relevant.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    To add and view comments - click any of the 'Add / View
                    comments to section' links within the orange boxes down the
                    left hand side of a release.
                  </h4>
                  <p>Click 'Add / view comments' link</p>
                  <img
                    src={imageCommentAdd}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                </li>
                <li>
                  <h4>
                    To add comments - add them into the text box and click the
                    green 'Submit' button.
                  </h4>
                  <p>Add comments and click green 'Submit' button.</p>
                  <img
                    src={imageCommentSave}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                </li>
                {/*
                <li>
                  <h4>
                    To resolve comments - click the grey 'Resolve' button if
                    you’ve dealt with them or select 'Remove' if they're no
                    longer relevant.
                  </h4>
                  <p>Click grey 'Resolve' button or select 'Remove'</p>
                </li>
                */}
              </ul>

              <h3>Help and support</h3>
              <p>
                If you have any issues adding, viewing or resolving comments
                contact:{' '}
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
              stepNumber={3}
              stepHeading="Add and edit content"
              open={step === 3}
            >
              <p>
                You and the members of your production team can add and edit
                content within any sections of your release.
              </p>
              <h3>Before you start</h3>
              <p>
                Make sure you and the members of your production team understand
                who’s responsible for the content within the different sections
                of your release.
              </p>
              <p>
                Our research has shown you should aim to add up to a maximum of
                400 words of content per section using a combination of
                subheadings and bullets where appropriate.
              </p>
              <p>
                For more detailed guidance and tips on how to create clear and
                consistent content read and refer to our:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <Link to="/documentation/content-design-standards-guide">
                    Content design standards guide
                  </Link>
                </li>
                <li>
                  <Link to="/documentation/style-guide">
                    Content design style guide
                  </Link>
                </li>
              </ul>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    To add content to or edit content within any section - click
                    the 'Add text block' button, a text area will appear which
                    you will then be able to edit.
                  </h4>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>
                      <h5>
                        If you want to add a new content block click the 'Add
                        text block' button
                      </h5>
                      <img
                        src={imageContentAddTextBlock}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                    <li>
                      <h5>
                        Click 'Edit' button when hovering on the grey editable
                        content blocks
                      </h5>
                      <img
                        src={imageContentEditButton}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                    <li>
                      <h5>
                        Add and edit content and click green 'Save' button
                      </h5>
                      <p>
                        Add and edit content in the same way you would using
                        Word or in an email and then click the green 'Save'
                        button when you've finished.
                      </p>
                      <img
                        src={imageContentSave}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                  </ul>
                </li>
                <li>
                  <h4>
                    To add tables and charts to a section - click the 'Add data
                    block' button.
                  </h4>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>
                      <h5>Click 'Add data block' button</h5>
                      <img
                        src={imageAddDatablock}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                    <li>
                      <h5>
                        Select data block using 'Select a data block' dropdown
                      </h5>
                      <p>
                        Select the related data block you're looking for using
                        the 'Select a data block' dropdown and the tables will
                        appear. Select the 'Embed' button and the charts will be
                        added to the section.
                      </p>
                      <img
                        src={imageEmbedDatablock}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                  </ul>
                </li>
                <li>
                  <h4>
                    To add key indicators to the 'Headline facts and figures'
                    section - click the green 'Add key statistic' button.
                  </h4>
                  <p>
                    Our research has shown you should aim to add up to a maximum
                    of 6 key indicators within this section.
                  </p>
                  <p>
                    You'll also need to add text to the 'Guidance text' text box
                    to explain what each indicator means.
                  </p>
                  <p>
                    Read and refer to the guidance and tips about 'Key
                    indicators' in our{' '}
                    <Link to="/documentation/style-guide">
                      Content design style guide
                    </Link>
                  </p>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>
                      <h5>Click green 'Add key statistic' button</h5>
                      <img
                        src={imageKeyStatBtn}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                    <li>
                      <h5>
                        Choose key statistic from drop down list and click green
                        'Embed' button
                      </h5>
                      <img
                        src={imageKeyStatSelect}
                        className="govuk-!-width-three-quarters  govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                    <li>
                      <h5>
                        Click the green 'Edit' button to add trend and guidance
                        details
                      </h5>
                      <img
                        src={imageKeyStatEdit}
                        className="govuk-!-margin-bottom-9"
                        alt=""
                        width="300"
                      />
                    </li>
                    <li>
                      <h5>Add guidance text</h5>
                      <img
                        src={imageKeyStatGuidance}
                        className="govuk-!-margin-bottom-9"
                        alt=""
                        width="300"
                      />
                      <p>Click the green 'Save' button when you've finished.</p>
                    </li>
                    <li>
                      <h5>Add up to a maximum of 6 key indicators</h5>
                      <img
                        src={imageKeyStatMultiple}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                  </ul>
                </li>
                <li>
                  <h4>
                    To add related information - add the related information
                    title and link to the text boxes and then click the green
                    'Add related information'.
                  </h4>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>
                      <h5>click 'Add related information button'</h5>
                      <img
                        src={imageRelatedBtn}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                    <li>
                      <h5>
                        Add title and link and click green 'Add related
                        information'
                      </h5>
                      <img
                        src={imageRelatedAdd}
                        className="govuk-!-margin-bottom-9"
                        alt=""
                        width="300"
                      />
                    </li>
                  </ul>
                </li>
                <li>
                  <h4>
                    To add and edit content to the 'Help and support' section of
                    your release contact:{' '}
                  </h4>
                  <strong>Explore education statistics team </strong>
                  <p>
                    Email <br />
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                  </p>
                  <h3>Don't</h3>
                  <p>
                    Worry if you haven't got all the content to complete your
                    release. You can come back and add it later.
                  </p>
                  <h3>Help and support</h3>
                  <p>
                    If you have any questions about how to write and present
                    content contact your nominated 'Content Design Champion'.
                  </p>
                  <p>
                    If you have any issues adding or editing content within the
                    admin system:
                  </p>
                  <strong>Explore education statistics team</strong>
                  <p>
                    Email <br />
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                  </p>
                  <h3>Next steps</h3>
                  <p>
                    Don't worry if you haven't got all the content to complete
                    your release. You can come back and add it later.
                  </p>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>
                      <Link to="/documentation/content-design-standards-guide">
                        Content design standards guide
                      </Link>
                    </li>
                    <li>
                      <Link to="/documentation/style-guide">
                        Content design style guide
                      </Link>
                    </li>
                  </ul>
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={4}
              stepHeading="Add and reorder sections"
              open={step === 4}
            >
              <p>
                Our research has shown you should aim to add up to a maximum of
                8 sections of content within a release.
              </p>
              <p>
                Once you've added all your sections you can drag and drop them
                into any order which you think will make most sense to users.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    To add a new section - click the green 'Add new section'
                    button.
                  </h4>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>
                      <h5>Click green 'Add new section' button</h5>
                      <img
                        src={imageSectionAdd}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                  </ul>
                </li>
                <li>
                  <h4>To reorder your sections:</h4>
                  <ul className="govuk-list govuk-list--bullet">
                    <li>
                      <h5>click the grey 'Reorder sections' button</h5>
                      <img
                        src={imageReorderBtn}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                    <li>
                      <h5>
                        reorder them using the 3 grey lines on the right-hand
                        side of each section, once in the correct order click
                        the green 'Save reordering' button
                      </h5>
                      <img
                        src={imageReorderSection}
                        className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                        alt=""
                      />
                    </li>
                  </ul>
                </li>
              </ul>

              <h3>Don't</h3>
              <p>
                Don't worry if you haven't got all the content to complete your
                sections and release.
              </p>
              <p>
                You can add the sections now and come back and add the content
                later.
              </p>
              <h3>Help and support</h3>
              <p>
                If you have any issues adding or reordering sections contact:
              </p>
              <strong>Explore education statistics team</strong>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={5}
              stepHeading="Preview content"
              open={step === 5}
            >
              <p>
                Once you've created, edited or updated content you can preview
                and check how it will appear on your live release.
              </p>
              <h3>Do</h3>
              <p>
                To preview your content as it will appear on your live release -
                click the 'Preview content' radio button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <h4>Click 'Preview content' radio button</h4>
                  <img
                    src={imageContentPreview}
                    className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                    alt=""
                  />
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>If you have any issues previewing your release contact: </p>
              <strong>Explore education statistics team </strong>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={6}
              stepHeading="Add release notes"
              open={step === 6}
            >
              <p>
                Once you and the members of your production team are happy with
                the content of your release, you should add release notes.
              </p>
              <p>
                The content in the release notes should explain to users how
                your release has been updated over time.
              </p>
              <p>
                The notes will be shown to users when they click the 'See
                updates' link under the 'About these statistics' section of your
                live release.
              </p>
              <h3>Do</h3>
              <p>
                To add release notes - add your notes into the 'Add release
                note' text box and click the green 'Add note' button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <h4>Add notes and click green 'Add note' button</h4>
                  <img
                    src={imageReleaseNote}
                    className="govuk-!-margin-bottom-9"
                    alt=""
                    width="300"
                  />
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>If you have any issues adding release notes contact: </p>
              <strong>Explore education statistics team</strong>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Next steps</h3>
              <p>
                If you and the members of your production team are happy with
                the content of your release you should discuss whether to update
                its status to 'Ready for sign-off' and move it on for 'Higher
                review'.
              </p>
              <p>
                For detailed guidance on how to update a release's status -{' '}
                <Link to="./edit-release">
                  Updating release status: step by step.
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
