import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import { RouteChildrenProps } from 'react-router';
import PrototypePage from './components/PrototypePage';
import StepNav from './components/PrototypeStepByStep';
import StepNavItem from './components/PrototypeStepByStepItem';

const DocumentationCreateNewRelease = ({ location }: RouteChildrenProps) => {
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
            <h1 className="govuk-heading-xl">Creating a new publication</h1>
          </div>
          <p>How to create a new publication.</p>
          <h2>Before you start</h2>
          <p>
            Lorem ipsum dolor sit amet consectetur adipisicing elit. Repellat
            earum facilis voluptatibus quae eum ex dignissimos. Odit pariatur ut
            veritatis. Accusantium quibusdam optio aliquid doloremque sit, quam
            pariatur explicabo debitis!
          </p>{' '}
          <StepNav>
            <StepNavItem stepNumber={1} stepHeading="Step 1" open={step === 1}>
              <p>Write standard html here</p>
            </StepNavItem>
            <StepNavItem stepNumber={2} stepHeading="Step 2" open={step === 2}>
              <p>Write standard html here</p>
            </StepNavItem>
            <StepNavItem stepNumber={3} stepHeading="Step 3" open={step === 3}>
              <p>Write standard html here</p>
            </StepNavItem>
            <StepNavItem stepNumber={4} stepHeading="Step 4" open={step === 4}>
              <p>Write standard html here</p>
            </StepNavItem>
          </StepNav>
        </div>
      </div>
    </PrototypePage>
  );
};

export default DocumentationCreateNewRelease;
