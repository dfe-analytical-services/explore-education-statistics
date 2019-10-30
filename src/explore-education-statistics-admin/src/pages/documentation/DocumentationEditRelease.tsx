import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Page from '@admin/components/Page';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imageReleaseFind from './images/guidance/guidance-edit-release-find.png';
import imageReleaseFilter from './images/guidance/guidance-edit-release-filter.png';
import imageReleaseDraft from './images/guidance/guidance-edit-release-draft.png';
import imageReleaseHighReview from './images/guidance/guidance-edit-release-high-review.png';
import imageReleaseContent from './images/guidance/guidance-edit-release-content.png';
import imageReleaseUpdateReleaseStatus from './images/guidance/guidance-edit-release-update-release-status.png';
import imageReleaseNotes from './images/guidance/guidance-edit-release-notes.png';

const DocumentationCreateNewRelease = ({ location: _ }: RouteChildrenProps) => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Editing a release and updating release status' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step by step guidance</span>
            <h1 className="govuk-heading-xl">
              Editing a release and updating release status
            </h1>
          </div>
          <p>
            How to edit a release and update a release’s status - including
            approving a release for sign-off.
          </p>
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="View and edit a release"
              open={step === 1}
            >
              <p>
                Anyone can view a release as long as they’re a member of its
                related ‘Production team’.
              </p>
              <p>
                Anyone can edit a release as long as they’re a member of its
                related ‘Production team’ and the release is listed as ‘In
                draft’.
              </p>
              <p>
                However, only a ‘Responsible statistician’ can edit a release
                once it’s been approved for sign-off by the ‘Production team’
                and sent for ‘Higher review’.
              </p>
              <p>
                A release is in ‘Higher review’ once it’s been reviewed,
                approved for sign-off and its release status updated to one of
                the following:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>‘Ready for sign-off’</li>
                <li>‘Approved for release’ </li>
                <li>‘In pre-release’</li>
              </ul>
              <p>
                If you need to edit a ‘Live’ release (ie a release which has
                been published on the service) contact:
              </p>
              <h3 className="govuk-heading-m">
                Explore education statistics team{' '}
              </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4 className="govuk-heading-s">
                    To find the release you want to view and edit - use the
                    'Manage publications and releases' tab and open its parent
                    publication from list in the lower half of your
                    administration dashboard.
                  </h4>
                  <p>
                    If you need to edit a release which has been sent for
                    ‘Higher review’ contact your ‘Responsible statistician’ or:
                  </p>
                  <h4>Explore education statistics team </h4>
                  <p>
                    Email <br />
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                  </p>
                  <img
                    src={imageReleaseFind}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    If you have access to a long list of publications - use the
                    ‘Select theme’ and ‘Select topic’ dropdowns to first find
                    the publication and then the related release you want to
                    view and edit.
                  </h4>
                  <img
                    src={imageReleaseFilter}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To view and edit a specific ‘In draft’ release - use the
                    'View draft releases' tab and select the green ‘View and
                    edit release’ button.
                  </h4>
                  <img
                    src={imageReleaseDraft}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4 className="govuk-heading-s">
                    To view a specific release which has been sent for ‘Higher
                    review’ - use the 'View scheduled releases' tab and select
                    the green ‘Preview release’ button.
                  </h4>
                  <img
                    src={imageReleaseHighReview}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                  <p>
                    If you need to edit a release which has been sent for
                    ‘Higher review’ contact your ‘Responsible statistician’ or:
                  </p>
                  <h4>Explore education statistics team </h4>
                  <p>
                    Email <br />
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                  </p>
                </li>
                <h3>Help and support</h3>
                <p>
                  If you can’t find the release you’re looking to view or edit
                  contact:{' '}
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
                  <a href="https://drive.google.com/open?id=1If5HD6zj-JZ62g-BDtJHxQPQTNnjp_6qb8y-2aqaM8o">
                    Managing data: step by step
                  </a>
                  .
                </p>
                <p>
                  For detailed guidance on how to create data blocks, tables and
                  charts for your release -{' '}
                  <a href="https://drive.google.com/open?id=1InzVL0xOHS8-VFVb_H_tFU6l4KJMMH3FWJNRV2aAab0">
                    Managing data blocks and creating tables and charts: step by
                    step
                  </a>
                  .
                </p>
                <p>
                  For detailed guidance on how to view and edit content and add,
                  view and review comments -{' '}
                  <a href="https://drive.google.com/open?id=1iKPB_uEITG3J3Lwl1rto4gYKg0Wr20gP6NGEPJ1DBCo">
                    Managing content: step by step
                  </a>
                  .
                </p>
              </ul>
            </StepNavItem>
            <StepNavItem
              stepNumber={2}
              stepHeading="Review and edit content"
              open={step === 2}
            >
              <p>
                You and the members of your ‘Production team’ can add and edit
                content within and add comments to any sections of your release.
              </p>
              <h3>Before you start</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <p>
                    Make sure you and the members of your production team
                    understand who’s responsible for the content within the
                    different sections of your release.
                  </p>
                </li>
                <li>
                  <p>
                    Make sure your content is written and formatted to the
                    standards set out in our{' '}
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
                    These guides will help you create clear and consistent
                    content which will help users understand your statistics and
                    data.
                  </p>
                </li>
              </ul>
              <h3>Do</h3>
              <h4>
                To create and edit any content and add comments to your release
                - under the main ‘Manage content’ tab across the top of the
                page, select the ‘Add / view comments and edit content’ radio
                button.
              </h4>
              <img
                src={imageReleaseContent}
                className="govuk-!-width-three-quarters"
                alt=""
              />
              <h3>Don't</h3>
              <h4>
                Don’t worry if you haven't got all the content to complete your
                release. You can come back and add it later.
              </h4>
              <h3>Help and support</h3>
              <p>
                If you have any issues creating, editing and updating content or
                adding, viewing and resolving comments contact:{' '}
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
                <a href="https://drive.google.com/open?id=1iKPB_uEITG3J3Lwl1rto4gYKg0Wr20gP6NGEPJ1DBCo">
                  Managing content: step by step
                </a>
                .
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={3}
              stepHeading="Update release status and approve for sign-off"
              open={step === 3}
            >
              <p>
                Anyone listed in your ‘Production team’ can update release
                status while your release ‘In draft’.
              </p>
              <p>
                If you and the members of your ‘Production team’ are happy with
                the content of your release, discuss whether to update its
                status to ‘Ready for sign-off’ and approve it for sign-off and
                ‘Higher review’.
              </p>
              <p>
                A release is in ‘Higher review’ once it’s been reviewed,
                approved for sign-off and its release status updated to one of
                the following:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>‘Ready for sign-off’</li>
                <li>‘Approved for release’ </li>
                <li>‘In pre-release’</li>
              </ul>
              <p>
                Only a ‘Responsible statistician’ can edit a release once it’s
                been approved for sign-off by the ‘Production team’ and sent for
                ‘Higher review’.
              </p>
              <p>
                You and your ‘Production team’ need to decide who’s going to
                update your release’s status from ‘In draft’ and approve it
                ‘Ready for sign-off’.
              </p>
              <h3>Before you start</h3>
              <p>
                A release will need to progress through all of the following
                release statuses before it can be published on Explore Education
                Statistics:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>‘In draft’</li>
                <li>‘Ready for sign-off’</li>
                <li>‘Approved for release’ </li>
                <li>‘In pre-release’</li>
              </ul>
              <p>
                Once your release has progressed through all of these stages it
                will get published and have a final status of ‘Live’.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    To update release status and approve your release for
                    sign-off - use the main ‘Update release status’ tab across
                    the top of the page, select the ‘Ready for sign-off’ radio
                    button and then the green ‘Update’ button.
                  </h4>
                  <div className="govuk-warning-text">
                    <span
                      className="govuk-warning-text__icon"
                      aria-hidden="true"
                    >
                      !
                    </span>
                    <strong className="govuk-warning-text__text">
                      <span className="govuk-warning-text__assistive">
                        Warning
                      </span>
                      Make sure you enter any relevant notes about the current
                      status of your release into the ‘Internal release notes’
                      text box.
                    </strong>
                  </div>
                  <img
                    src={imageReleaseUpdateReleaseStatus}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4>
                    Any notes you enter will appear under the ‘View draft
                    releases’ and ‘View scheduled releases’ tabs to keep the
                    members of your ‘Publication team’ up-to-date about your
                    release.
                  </h4>
                  <img
                    src={imageReleaseNotes}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you have any issues or questions about updating release
                status contact:{' '}
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationCreateNewRelease;
