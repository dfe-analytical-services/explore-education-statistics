import React, { ReactNode } from 'react';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import Link from '@admin/components/Link';

export interface Section<SectionType extends {}> {
  section: SectionType;
  label: string;
  linkTo: string;
}

interface Props<SectionType> {
  navigationHeadingText: string;
  navigationHeadingSubtitle: string;
  availableSections: Section<SectionType>[];
  selectedSection: Section<SectionType>;
  children: ReactNode;
}

/**
 * Represents a portion of a page with navigable tabbed sections, and Next / Previous buttons to switch between the
 * sections.  A client of this component provides a full ordered list of the available sections, and the currently
 * selected section.  From this, the Previous and Next links are inferred.
 *
 * The client also provides the appropriate body for the selected section, which is rendered within this component
 * as its props.children.
 *
 * The SectionType generic type represents a type for a unique id per section, whatever works best for the client.
 * This could for instance be an enum of available sections, or a string or number id.
 *
 * @param availableSections
 * @param selectedSection
 * @param children
 * @constructor
 */
const NavigableSections = <SectionType extends {}>({
  availableSections,
  selectedSection,
  children,
}: Props<SectionType>) => {
  const nextSection =
    availableSections.indexOf(selectedSection) < availableSections.length - 1
      ? availableSections[availableSections.indexOf(selectedSection) + 1]
      : null;

  const previousSection =
    availableSections.indexOf(selectedSection) > 0
      ? availableSections[availableSections.indexOf(selectedSection) - 1]
      : null;

  return (
    <>
      <MainSectionNavigation
        selectedSection={selectedSection.section}
        availableSections={availableSections}
      />
      {children}
      <PreviousNextLinks
        previousSection={previousSection || undefined}
        nextSection={nextSection || undefined}
      />
    </>
  );
};

interface NavigationProps<SectionType> {
  availableSections: Section<SectionType>[];
  selectedSection: SectionType;
}

const MainSectionNavigation = <SectionType extends {}>({
  availableSections,
  selectedSection,
}: NavigationProps<SectionType>) => {
  return (
    <>
      <nav className="app-navigation govuk-!-margin-bottom-9">
        <ul className="app-navigation__list govuk-!-margin-bottom-0">
          {availableSections.map(section => (
            <li
              key={section.section.toString()}
              className={
                section.section === selectedSection
                  ? 'app-navigation--current-page'
                  : 'app-navigation--non-selected-page'
              }
            >
              <Link
                to={section.linkTo}
                className="govuk-link govuk-link--no-visited-state"
              >
                {section.label}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
    </>
  );
};

export default NavigableSections;
