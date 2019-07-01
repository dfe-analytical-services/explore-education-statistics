import React from 'react';
import Link from './Link';

interface Props<HeadingType extends {}> {
  navigationHeadingText: string;
  navigationHeadingSubtitle: string;
  availableSections: NavigationHeader<HeadingType>[];
  selectedSection: HeadingType;
  task?: string;
}

export interface NavigationHeader<HeadingType extends {}> {
  section: HeadingType;
  label: string;
  linkTo: string;
}

const SecondLevelNavigationHeadings = <HeadingType extends {}>({
  navigationHeadingText,
  navigationHeadingSubtitle,
  availableSections,
  selectedSection,
}: Props<HeadingType>) => {
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

export default SecondLevelNavigationHeadings;
