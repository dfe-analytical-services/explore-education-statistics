import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Page from '@admin/components/Page';
import StepNav from './components/StepByStep';
import StepNavItem from './components/StepByStepItem';
import imageChoosePublication from './images/guidance/guidance-choose-publication.jpg';
import imageCreateReleaseButton from './images/guidance/guidance-create-release-button.jpg';
import imageCreateReleaseNavigation from './images/guidance/guidance-create-release-navigation.jpg';
import imageSelectTheme from './images/guidance/guidance-select-theme.jpg';

const DocumentationCreateNewRelease = ({ location: _ }: RouteChildrenProps) => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/documentation' },
        { name: 'Creating a new publication' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step by step guidance</span>
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
                Under the 'Manage publications and releases' tab, select the
                green 'Create new publication' button.
              </p>

              <img
                src={imageChoosePublication}
                className="govuk-!-width-three-quarters"
                alt="Choose a publication"
              />
              <p>
                2. If you have access to a long list of publications, use the
                ‘Select theme’ and ‘Select topic’ dropdowns to find a
                publication.
              </p>
              <img
                src={imageSelectTheme}
                className="govuk-!-width-three-quarters"
                alt=""
              />
            </StepNavItem>
            <StepNavItem
              stepNumber={2}
              stepHeading="Create a new release"
              open={step === 2}
            >
              <p>
                Once you’ve created a new release you’ll become its ‘Primary
                analyst / Author’ and (along with members of your ‘Production
                team’) be responsible for putting it together within the admin
                system.
              </p>
              <h3>Do</h3>
              <p>
                Under the publication you’ve chosen, select the green 'Create
                new release' button.
              </p>
              <img
                src={imageCreateReleaseButton}
                className="govuk-!-width-three-quarters"
                alt=""
              />
              <h3>Don't</h3>
              <p>
                Don’t worry if you haven't got all the data, files and content
                to complete your release.
              </p>
              <p>
                You can come back and add these later using the various tabs at
                the top of the page.
              </p>
              <img
                src={imageCreateReleaseNavigation}
                className="govuk-!-width-three-quarters"
                alt=""
              />
              <h3>Help and support</h3>
              <p>
                If you can't find the publication where you want to create your
                release contact:{' '}
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
              stepHeading="Release summary"
              open={step === 3}
            >
              <p>Write standard html here</p>
            </StepNavItem>
            <StepNavItem
              stepNumber={4}
              stepHeading="Manage data"
              open={step === 4}
            >
              <p>Write standard html here</p>
            </StepNavItem>
            <StepNavItem
              stepNumber={5}
              stepHeading="Manage data blocks"
              open={step === 5}
            >
              <p>Write standard html here</p>
            </StepNavItem>
            <StepNavItem
              stepNumber={6}
              stepHeading="Manage content"
              open={step === 6}
            >
              <p>Write standard html here</p>
            </StepNavItem>
            <StepNavItem
              stepNumber={7}
              stepHeading="Update release status"
              open={step === 7}
            >
              <p>Write standard html here</p>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </Page>
  );
};

export default DocumentationCreateNewRelease;
