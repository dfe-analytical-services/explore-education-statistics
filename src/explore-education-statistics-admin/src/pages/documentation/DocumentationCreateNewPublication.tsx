import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imageCreatePublication from './images/guidance/guidance-publication-create.png';
import imagePublicationMethodologySelect from './images/guidance/guidance-publication-methodology-select.png';
import imagePublicationMethodologyLink from './images/guidance/guidance-publication-methodology-link.png';
import imageContactPublication from './images/guidance/guidance-publication-contact.png';
import imageProductionTeamPublication from './images/guidance/guidance-publication-team.png';
import imageCreateReleasePublication from './images/guidance/guidance-publication-create-new-release.png';

const DocumentationCreateNewRelease = ({ location: _ }: RouteChildrenProps) => {
  // TODO: clean the below up
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Creating a new publication' },
      ]}
      title="Creating a new publication"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step-by-step guidance</span>
            <h1 className="govuk-heading-xl">Creating a new publication</h1>
          </div>
          <p>
            How to create a new publication - including adding a methodology and
            contact details.
          </p>
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Create a publication"
              open={step === 1}
            >
              <p>Only a ‘Team leader’ can create a new publication.</p>
              <p>
                If you’re not a ‘Team leader’ and need to create a publication
                in their absence contact:
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
                Under the 'Manage publications and releases' tab, click the
                green 'Create new publication' button.
              </p>
              <img
                src={imageCreatePublication}
                className="govuk-!-width-three-quarters"
                alt="Choose a publication"
              />
              <h3>Help and support</h3>
              <p>
                If you can’t see the green 'Create new publication' button or
                have any issues or questions about creating a new publication
                contact:{' '}
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
              stepNumber={2}
              stepHeading="Choose a methodology"
              open={step === 2}
            >
              <p>
                You’ll need to link your publication to a methodology before it
                goes live.
              </p>
              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>
                    Choose an existing methodology, select from the drop down
                    select box
                  </h4>
                  <img
                    src={imagePublicationMethodologySelect}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4>Alternatively link to an external methodology file</h4>
                  <p>
                    For example, this may be a link to a methodology page hosted
                    on the legacy system
                  </p>
                  <img
                    src={imagePublicationMethodologyLink}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4>
                    If a methodology file isn't currently ready, choose 'Select
                    a methodology later'.
                  </h4>
                  <p>
                    You can apply a methodology file at any point prior to
                    publication.{' '}
                  </p>
                </li>
              </ul>
              <h3>Help and support</h3>
              <p>
                If you can’t find the methodology you’re looking for or have any
                questions about creating a new methodology contact:
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
                For detailed guidance on how to add a methodology to and create
                a methodology for your release -{' '}
                <a href="https://docs.google.com/document/d/1QhND0vsawE2moPFW5TgJvMto-dJ9DnOSsBHk7_9Ixho/edit?usp=sharing">
                  Adding and creating a methodology: step by step
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={3}
              stepHeading="Choose a contact"
              open={step === 3}
            >
              <p>
                According to ONS and GSS standards, all publications (and their
                related releases) must include relevant contact details which
                users can use if they have any queries about your statistics and
                data:{' '}
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://gss.civilservice.gov.uk/wp-content/uploads/2018/11/Writing-about-statistics-Edition-2.0-October-2018-1.pdf">
                    GSS - Writing about statistics: Guidance for producers
                  </a>{' '}
                  - Page 20
                </li>
                <li>
                  <a href="https://style.ons.gov.uk/category/writing-for-the-web/gov-uk-release-calendar/#contact-details">
                    ONS - Writing for the web guidance
                  </a>
                </li>
              </ul>
              <p>
                You must choose a contact for your publication. Their name and
                contact details will then appear as the nominated and main point
                of contact for data and methodology enquiries for the
                publication and all its related releases.
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <a href="https://gss.civilservice.gov.uk/wp-content/uploads/2018/11/Writing-about-statistics-Edition-2.0-October-2018-1.pdf">
                    GSS - Writing about statistics: Guidance for producers
                  </a>{' '}
                  - Page 20
                </li>
              </ul>

              <ul className="govuk-list govuk-list--number dfe-guidance-list">
                <li>
                  <h4>Choose a contact from the drop down select box</h4>
                  <img
                    src={imageContactPublication}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4>
                    After making a selection, a summary of the contact details
                    will appear. If you select the wrong contact, you can select
                    a different contact using the step above.
                  </h4>
                  <img
                    src={imageProductionTeamPublication}
                    className="govuk-!-width-three-quarters"
                    alt=""
                  />
                </li>
                <li>
                  <h4>
                    After checking that all the details are correct, click
                    'Create publication'
                  </h4>
                </li>
              </ul>

              <h3>Help and support</h3>
              <p>
                If you can’t find the contact you’re looking for or the details
                are incorrect and need updating contact:
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
              stepNumber={5}
              stepHeading="Edit and delete a publication"
              open={step === 5}
            >
              <p>If you need to edit or delete a publication contact:</p>
              <p>
                Email <br />
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </p>
            </StepNavItem>
            <StepNavItem
              stepNumber={6}
              stepHeading="Next steps - create a new release"
              open={step === 6}
            >
              <p>
                Once you’ve created your publication, you and any members of
                your ‘Production team’ can start creating its related releases.
              </p>
              <p>
                Under the 'Manage publications and releases' tab, select the
                name of the publication you’ve just created which will include
                the button 'Create new release'.
              </p>
              <img
                src={imageCreateReleasePublication}
                className="govuk-!-width-three-quarters govuk-!-margin-bottom-9"
                alt="Create a new release"
              />
              <h3>Next steps</h3>
              <p>
                For detailed guidance on how to create a new release -{' '}
                <Link to="./create-new-release">
                  Creating a new release: step by step
                </Link>
              </p>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationCreateNewRelease;
