import React, { ReactNode } from 'react';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import Link from '@admin/components/Link';

export interface NavigationHeader<HeadingType extends {}> {
  section: HeadingType;
  label: string;
  linkTo: string;
}

interface SectionedPageProps<SectionType> {
  navigationHeadingText: string;
  navigationHeadingSubtitle: string;
  availableSections: NavigationHeader<SectionType>[];
  selectedSection: NavigationHeader<SectionType>;
  children: ReactNode;
}

const SectionedPage = <SectionType extends {}>({
  navigationHeadingText,
  navigationHeadingSubtitle,
  availableSections,
  selectedSection,
  children,
}: SectionedPageProps<SectionType>) => {
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
      <SectionedPageNavigation
        navigationHeadingText={navigationHeadingText}
        navigationHeadingSubtitle="Edit release"
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

interface SectionedPageNavigationProps<SectionType> {
  navigationHeadingText: string;
  navigationHeadingSubtitle: string;
  availableSections: NavigationHeader<SectionType>[];
  selectedSection: SectionType;
}

const SectionedPageNavigation = <SectionType extends {}>({
  navigationHeadingText,
  navigationHeadingSubtitle,
  availableSections,
  selectedSection,
}: SectionedPageNavigationProps<SectionType>) => {
  return (
    <>
      <h1 className="govuk-heading-l">
        {navigationHeadingText}
        <span className="govuk-caption-l">{navigationHeadingSubtitle}</span>
      </h1>

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

export default SectionedPage;
