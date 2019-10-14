import Page from '@admin/components/Page';
import React from 'react';
import { RouteChildrenProps } from 'react-router';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';

const DocumentationManageContent = ({ location: _ }: RouteChildrenProps) => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Managing content' },
      ]}
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
                <a href="https://drive.google.com/open?id=1Z1D7bxLVXAZEp855LSiR7b40ayWeVWaMOCuixItBVDo">
                  Content design standards guide
                </a>{' '}
                and{' '}
                <a href="https://eesadminprototype.z33.web.core.windows.net/prototypes/documentation/style-guide">
                  Content design style guide
                </a>
                .
              </p>
              <p>
                These guides will help you create clear and consistent content
                which will help users understand your statistics and data.
              </p>
              <h3>Before you start</h3>
              <p>
                Make sure you and the members of your [roduction team understand
                who's responsible for the content within the different sections
                of your release.
              </p>
              <h3>Do</h3>
              <p>
                To create and edit any content and add comments to your release
                - select the 'Add / view comments and edit content' radio
                button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1jqjgpGFMDmUrmk-pePOrjGneQ_3ZNLht">
                    Select the 'Add / view comments and edit content' radio
                    button
                  </a>
                </li>
              </ul>
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
                Once you've dealt with the issues raised within comments you
                should record them as either being resolved or remove them if
                they're no longer relevant.
              </p>
              <h3>Do</h3>
              <p>
                1. To add and view comments - select any of the 'Add / view
                comments to section' links within the orange boxes down the left
                hand side of a release.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1USwHwp1YMTHXwdp4S6hWGqFkFBaRle7S">
                    Select 'Add / view comments to section' link
                  </a>
                </li>
              </ul>
              <p>
                2. To add comments - add them into the text box and select the
                green 'Submit' button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1-oKvKHOrGABKEWw1axnMQJ6-dfVcKe33">
                    Add comments and select green 'Submit' button.
                  </a>
                </li>
              </ul>
              <p>
                3. To resolve comments - select the grey 'Resolve' button if
                you’ve dealt with them or select 'Remove' if they're no longer
                relevant.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1iB7oeQiBZdoOf-qtit71COcjUbnWIhUR">
                    Select grey 'Resolve' button or select 'Remove'
                  </a>
                </li>
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
                  <a href="https://drive.google.com/open?id=1Z1D7bxLVXAZEp855LSiR7b40ayWeVWaMOCuixItBVDo">
                    Content design standards guide
                  </a>
                </li>
                <li>
                  <a href="https://eesadminprototype.z33.web.core.windows.net/prototypes/documentation/style-guide">
                    Content design style guide
                  </a>
                </li>
              </ul>
              <h3>Do</h3>
              <p>
                1. To add content to or edit content within any section - select
                the green 'Edit this section' button in the top right-hand
                corner of any section.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1USwHwp1YMTHXwdp4S6hWGqFkFBaRle7S">
                    Select green 'Edit this section' button
                  </a>
                </li>
              </ul>
              <p>
                Add and edit content in the same way you would using Word or in
                an email and then select the green 'Save' button when you've
                finished.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1aKxT6XYJ3UnlseMWfhJNqM1LuGWlxaI6">
                    Add and edit content and select green 'Save' button
                  </a>
                </li>
              </ul>
              <p>
                If you're adding or editing content to 'Latest headline facts
                and figures' section read and refer to the guidance and tips
                about 'Key indicators' in our{' '}
                <a href="https://eesadminprototype.z33.web.core.windows.net/prototypes/documentation/style-guide">
                  Content design style guide
                </a>
                .
              </p>
              <p>
                2. To add tables and charts to a section - select the green 'Add
                data black to this section' button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1Cm71wi4nX6tffWGdMr_14dMQkN7s175h">
                    Select green 'Add data black to this section' button
                  </a>
                </li>
              </ul>
              <p>
                Select the related data block you're looking for using the
                'Select a data block' dropdown and the tables and charts will be
                added to the section.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1T7LAHc7qtLgJX085hyPr5LQq1iTaVP5B">
                    Select data block using 'Select a data block' dropdown
                  </a>
                </li>
              </ul>
              <p>
                3. To add key indicators to the 'Latest headline facts and
                figures' section - select the green 'Add another key indicator'
                button.
              </p>
              <p>
                Our research has shown you should aim to add up to a maximum of
                6 key indicators within this section.
              </p>
              <p>
                You'll also need to add text to 'Guidance title' and 'Guidance
                text' text boxes to explain what the indicator means.
              </p>
              <p>
                Read and refer to the guidance and tips about 'Key indicators'
                in our{' '}
                <a href="https://eesadminprototype.z33.web.core.windows.net/prototypes/documentation/style-guide">
                  Content design style guide
                </a>
              </p>
              <p>Select the green 'Save' button when you've finished.</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1IAqyiKwP1EdvPR5GI7tU9Z4i_3qKwvKy">
                    Select green 'Add another key indicator' button, add
                    guidance text and select green 'Save' button
                  </a>
                </li>
              </ul>
              <p>
                4. To add related information - add the related information
                title and link to the text boxes and then select the green 'Add
                related information'.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1QcUTnCzYstAOKk5mIcWXrSVYGWSEKmBM">
                    Add title and link and select green 'Add related
                    information'
                  </a>
                </li>
              </ul>
              <p>
                5. To add and edit content to the 'Help and support' section of
                your release contact:{' '}
              </p>
              <strong>Explore education statistics team </strong>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Don't</h3>
              <p>
                Don't worry if you haven't got all the content to complete your
                release. You can come back and add it later.
              </p>
              <h3>Help and support</h3>
              <p>
                If you have any questions about how to write and present content
                contact your nominated 'Content Design Champion'.
              </p>
              <p>
                If you have any issues adding or editing content within the
                admin system:{' '}
              </p>
              <strong>Explore education statistics team </strong>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Next steps</h3>
              <p>
                Don't worry if you haven't got all the content to complete your
                release. You can come back and add it later.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1Z1D7bxLVXAZEp855LSiR7b40ayWeVWaMOCuixItBVDo">
                    Content design standards guide
                  </a>
                </li>
                <li>
                  <a href="https://eesadminprototype.z33.web.core.windows.net/prototypes/documentation/style-guide">
                    Content design style guide
                  </a>
                </li>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={4}
              stepHeading="Add and reorder sections"
              open={step === 4}
            >
              <p>
                You should aim to add up to 8 sections to the 'Contents' section
                of your release.
              </p>
              <p>
                Our research has shown you should aim to add up to a maximum of
                8 sections of content within a release.
              </p>
              <p>
                Once you've added all your sections you can drag and drop them
                into any order which you think will make most sense to users.
              </p>
              <h3>Do</h3>
              <p>
                1. To add a new section - select the green 'Add new section'
                button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1XhzngKyxuFyTRygMW7wFAKvCZePkTgvd">
                    Select green 'Add new section' button
                  </a>
                </li>
              </ul>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1fgU-Nke0f1YrPGUOQkzE9BNu7KS7olh8">
                    View added sections
                  </a>
                </li>
              </ul>
              <p>2. To reorder your sections:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>select the grey 'Reorder sections' button</li>
                <li>
                  reorder them using the 3 grey lines on the right-hand side of
                  each section
                </li>
                <li>select the green 'Save reordering' button</li>
              </ul>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=121HgJh3OjoMf98R2g5IA6NSOz9TC8vOw">
                    Select grey 'Reorder sections' button
                  </a>
                </li>
              </ul>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1xi-kjUYRLOoOVID0nCU9YIjg2-Kkcinm">
                    Reorder using 3 grey lines and select green 'Save
                    reordering' button
                  </a>
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
                If you have any issues adding or reordering sections contact:{' '}
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
                select the 'Preview content' radio button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1oLJsUiG8tHxSq_g5u-t1oWUl6woTsv9r">
                    Select 'Preview content' radio button
                  </a>
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
                the content of your release you should add 'release notes.
              </p>
              <p>
                The content in the release notes should explain how your release
                has been updated over time to users
              </p>
              <p>
                They notes will be shown to users when they select the 'See
                updates' link under the 'About these statistics' section of your
                live release.
              </p>
              <p>
                'First published' will appear by default under 'See updates' for
                all newly published releases.
              </p>
              <h3>Do</h3>
              <p>
                To add release notes - add your notes into the 'Add release
                note' text box and select the green 'Add note' button.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://drive.google.com/open?id=1kF4Uvu4Yuln0d2v-_Fkyvt5JyvjEYXvg">
                    Add notes and select green 'Add note' button
                  </a>
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>If you have any issues adding release notes contact: </p>
              <strong>Explore education statistics team </strong>
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
                <a href="https://drive.google.com/open?id=1oQ1NHGlKthD_erwjmOAlyp6Zd2kTTOjzCSYPkuD67uk">
                  Updating release status: step by step.
                </a>
              </p>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationManageContent;
