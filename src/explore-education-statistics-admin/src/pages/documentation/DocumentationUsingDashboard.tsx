import { RouteChildrenProps } from 'react-router';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import React from 'react';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imagePublicationCreate from './images/guidance/guidance-publication-create.png';
import imagePublicationSelect from './images/guidance/guidance-publication-select.png';
import imagePublicationSelectTheme from './images/guidance/guidance-publication-select-theme.png';
import imagePublicationCreateRelease from './images/guidance/guidance-publication-create-release.png';
import imageReleaseNavigation from './images/guidance/guidance-create-release-navigation.png';
import imageReleaseList from './images/guidance/guidance-release-list.png';
import imageReleaseDraftEdit from './images/guidance/guidance-release-draft-edit.png';
import imageReleaseScheduled from './images/guidance/guidance-release-scheduled.png';
import imageReleaseMethodology from './images/guidance/guidance-release-methodology.png';

const DocumentationCreateNewRelease = ({ location: _ }: RouteChildrenProps) => {
  // TODO: clean this up
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Using your administration dashboard' },
      ]}
      title="Using your administration dashboard"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step by step guidance</span>
            <h1 className="govuk-heading-xl">
              Using your administration dashboard
            </h1>
          </div>
          <p>
            How to use your administration dashboard to manage publications,
            releases and methodology.
          </p>

          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Create a new publication"
              open={step === 1}
            >
              <p>
                Only a ‘Responsible Statistician’ can create a new publication.
              </p>
              <p>
                If you’re not a responsible statistician and need to create a
                publication in their absence contact:{' '}
              </p>
              <h3>Explore education statistics team </h3>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Do</h3>
              <p>
                Under the main 'Manage publications and releases' tab on your
                administration dashboard, select the green 'Create new
                publication' button.
              </p>
              <img
                src={imagePublicationCreate}
                alt="Create new publication"
                className="govuk-!-width-three-quarters"
              />
              <h3>Help and support</h3>
              <p>
                If you can’t see the green 'Create new publication' button or
                have any issues or questions about creating a new publication
                contact:
              </p>
              <h4>Explore education statistics team</h4>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Next steps</h3>
              <p>
                For more detailed guidance:{' '}
                <Link to="/documentation/create-new-publication?step=2">
                  Creating a new publication: step by step
                </Link>
                .{' '}
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={2}
              stepHeading="Create a new release"
              open={step === 2}
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
                  <h4>
                    Under the main 'Manage publications and releases' tab,
                    select the publication where you want to create your
                    release.
                  </h4>
                  <img
                    src={imagePublicationSelect}
                    alt="Select publication"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
                <li>
                  <h4>
                    If you have access to a long list of publications - use the
                    ‘Select theme’ and ‘Select topic’ dropdowns to find a
                    publication.
                  </h4>
                  <img
                    src={imagePublicationSelectTheme}
                    alt="Select theme and topic to show relevant publications"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
                <li>
                  <h4>
                    Under the publication you’ve chosen, select the green
                    'Create new release' button.
                  </h4>
                  <img
                    src={imagePublicationCreateRelease}
                    alt="Select 'Create new release' button"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
              </ul>
              <h3>Don't</h3>
              <p>
                Worry if you haven't got all the data, files and content to
                complete your release.
              </p>
              <p>
                You can come back and add these later using the various tabs at
                the top of the page.
              </p>
              <img
                src={imageReleaseNavigation}
                alt="Release navigation"
                className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
              />
              <h3>Help and support</h3>
              <p>
                If you can't find the publication where you want to create your
                release or created a release in error and want to delete it
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
                For more detailed guidance:{' '}
                <Link to="/documentation/create-new-release?step=2">
                  Creating a new release: step by step
                </Link>
                .{' '}
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={3}
              stepHeading="Add and create a methodology"
              open={step === 3}
            >
              <h3>Help and support</h3>
              <p>
                If you can’t find the methodology you’re looking for or have any
                questions about creating a new methodology contact:
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
              stepNumber={4}
              stepHeading="View or edit a publication "
              open={step === 4}
            >
              <p>Only a ‘Team leader’ can view and edit a publication.</p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    Select the publication you want to view or edit under the
                    'Manage publications and releases' tab.
                  </h4>
                  <img
                    src={imagePublicationSelect}
                    alt="Select publication"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
                <li>
                  <h4>
                    If you have access to a long list of publications, to find
                    the publication you want to view or edit use the ‘Select
                    theme’ and ‘Select topic’ dropdowns.
                  </h4>
                  <img
                    src={imagePublicationSelectTheme}
                    alt="Select theme and topic to show relevant publications"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you can’t find the methodology you’re looking for or have any
                questions about creating a new methodology contact:
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
              stepNumber={5}
              stepHeading="View and edit a release"
              open={step === 5}
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
                However, only a ‘Responsible Statistician’ can edit a release
                once it’s status has been set to ‘Ready for higher review’.
              </p>
              <p>A release can have a status of the following:</p>
              <ul className="govuk-list govuk-list--bullet">
                <li>‘In draft’</li>
                <li>‘Ready for higher review’</li>
                <li>‘Approved for publication’</li>
              </ul>
              <p>
                If you need to edit a ‘Live’ release (i.e. a release which has
                been published on the service) contact:{' '}
              </p>
              <h4>Explore education statistics team </h4>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    To find the release you want to view and edit - use the
                    'Manage publications and releases' tab and open its parent
                    publication.
                  </h4>
                  <p>
                    If you need to edit a release which has been sent for
                    ‘Higher review’ contact your ‘Responsible Statistician’ or:
                  </p>
                  <h4>Explore education statistics team </h4>
                  <p>
                    Email <br />
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                  </p>
                  <img
                    src={imageReleaseList}
                    alt="Choose publication and select relevant release"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
                <li>
                  <h4>
                    If you have access to a long list of publications - use the
                    ‘Select theme’ and ‘Select topic’ dropdowns to first find
                    the publication and then the related release you want to
                    view and edit.
                  </h4>
                  <img
                    src={imagePublicationSelectTheme}
                    alt="Select theme and topic to show relevant publications"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
                <li>
                  <h4>
                    To view and edit a specific ‘In draft’ release - use the
                    'View draft releases' tab and select the green ‘View and
                    edit release’ button.
                  </h4>
                  <img
                    src={imageReleaseDraftEdit}
                    alt="View releases in draft"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
                <li>
                  <h4>
                    To view a specific release which has been sent for ‘Higher
                    review’ - use the 'View scheduled releases' tab and select
                    the green ‘Preview release’ button.
                  </h4>
                  <img
                    src={imageReleaseScheduled}
                    alt="View releases in scheduled for publiccation"
                    className="govuk-!-width-three-quarters"
                  />
                  <p>
                    If you need to edit a release which has been sent for
                    ‘Higher review’ contact your ‘Responsible Statistician’ or:
                  </p>
                  <h5>Explore education statistics team </h5>
                  <p>
                    Email <br />
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                  </p>
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you can’t find the release you’re looking to view or edit
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
                For detailed guidance on how to upload data and files and add
                footnotes to your release -{' '}
                <Link to="/documentation/manage-data">
                  Managing data: step by step
                </Link>
                .
              </p>
              <p>
                For detailed guidance on how to create data blocks, tables and
                charts for your release -{' '}
                <Link to="/documentation/manage-data-block">
                  Managing data blocks and creating tables and charts: step by
                  step
                </Link>
                .
              </p>
              <p>
                For detailed guidance on how to view and edit content and add,
                view and review comments -{' '}
                <Link to="/documentation/manage-content">
                  Managing content: step by step
                </Link>
                .
              </p>
              <p>
                For detailed guidance on how to edit a release and update
                release status -{' '}
                <Link to="/documentation/edit-release">
                  Editing a release and updating release status: step by step
                </Link>
                .
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={6}
              stepHeading="View or edit a methodology"
              open={step === 6}
            >
              <p>
                Anyone can view and edit a methodology within the admin system
                as long as they’re a member of its related ‘Production team’.
              </p>
              <p>
                If you’re methodology is still listed on GOV.UK, you’ll need to
                contact the GOV.UK team to edit its content.
              </p>
              <h3>Do</h3>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    To view and edit a methodology under a publication, use the
                    'Manage publications and releases' tab.
                  </h4>
                  <img
                    src={imageReleaseMethodology}
                    alt="View and edit methodology"
                    className="govuk-!-width-three-quarters"
                  />
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you can’t find the methodology you’re looking to view or edit
                contact:
              </p>
              <h4>Explore education statistics team</h4>
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
