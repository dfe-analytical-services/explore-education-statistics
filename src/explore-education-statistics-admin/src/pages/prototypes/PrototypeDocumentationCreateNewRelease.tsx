import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import { RouteChildrenProps } from 'react-router';
import PrototypePage from './components/PrototypePage';
import StepNav from './components/PrototypeStepByStep';
import StepNavItem from './components/PrototypeStepByStepItem';

const DocumentationGlossary = ({ location }: RouteChildrenProps) => {
  const query = new URLSearchParams(window.location.search);
  const step = Number(query.get('step'));

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { text: 'Documentation', link: '/prototypes/documentation' },
        { text: 'Training' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="app-content__header">
            <span className="govuk-caption-xl">Step by step guidance</span>
            <h1 className="govuk-heading-xl">Creating a new release</h1>
          </div>
          <p>
            How to create a new release - including uploading data and files and
            creating data blocks (ie tables and charts) and content.
          </p>
          <h2>Before you start</h2>
          <p>
            You can create a release in any order but need to make sure your:
          </p>{' '}
          <ul className="govuk-list govuk-list--bullet">
            <li>
              data is in the required csv format set out in our{' '}
              <a href="https://drive.google.com/open?id=15h7FWsdK7gqgYA1oM4YESvW8_sx4bgob">
                Data standards guide
              </a>
            </li>
            <li>
              content is written and formatted to the standards set out in our{' '}
              <a href="https://drive.google.com/open?id=1Z1D7bxLVXAZEp855LSiR7b40ayWeVWaMOCuixItBVDo">
                Content design standards guide
              </a>{' '}
              and{' '}
              <a href="https://eesadminprototype.z33.web.core.windows.net/prototypes/documentation/style-guide">
                Content design style guide
              </a>
            </li>
          </ul>
          <StepNav>
            <StepNavItem
              stepNumber={1}
              stepHeading="Choose a publication"
              open={step === 1}
            >
              <p>
                Anyone can create a new release under their assigned list of
                publications.
              </p>
              <h3>Do</h3>
              <p>
                1. Under the 'Manage publications and releases' tab on your
                administration dashboard, select the publication where you want
                to create your release.
              </p>
              <img
                src="/static/images/guidance/guidance-choose-pub.png"
                alt=""
              />
              <p>
                2. If you have access to a long list of publications, use the
                ‘Select theme’ and ‘Select topic’ dropdowns to find a
                publication.
              </p>
              <img
                src="/static/images/guidance/guidance-select-theme.png"
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
                src="/static/images/guidance/guidance-create-release-button.png"
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
                src="/static/images/guidance/guidance-create-release-navigation.png"
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
    </PrototypePage>
  );
};

export default DocumentationGlossary;
