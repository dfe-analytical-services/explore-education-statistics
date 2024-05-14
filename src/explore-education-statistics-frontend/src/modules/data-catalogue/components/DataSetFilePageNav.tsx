import { useMobileMedia } from '@common/hooks/useMedia';
import { Dictionary } from '@common/types';
import styles from '@frontend/modules/data-catalogue/components/DataSetFilePageNav.module.scss';
import {
  ApiPageSection,
  PageSection,
} from '@frontend/modules/data-catalogue/DataSetFilePage';
import classNames from 'classnames';
import React from 'react';

interface Props {
  activeSection: PageSection | ApiPageSection;
  sections: Dictionary<string>;
  onClickItem: (id: PageSection) => void;
}
export default function DataSetFilePageNav({
  activeSection,
  sections,
  onClickItem,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div
      className={classNames('govuk-grid-column-one-third', styles.navContainer)}
    >
      <h2 className="govuk-heading-s" id="nav-heading">
        On this page
      </h2>
      <nav aria-labelledby="nav-heading" role="navigation">
        <ul className={classNames('govuk-list', styles.navList)}>
          {Object.keys(sections).map(id => (
            <NavItem
              hashId={id}
              key={id}
              text={sections[id]}
              isActive={!isMobileMedia && activeSection === id}
              onClick={() => onClickItem(id)}
            />
          ))}

          {isMobileMedia ? (
            <hr />
          ) : (
            <NavItem
              className="govuk-!-padding-top-4"
              hashId="main-content"
              text="Back to top"
              onClick={() => onClickItem('details')}
            />
          )}
        </ul>
      </nav>
    </div>
  );
}

interface NavItemProps {
  className?: string;
  hashId: PageSection | ApiPageSection | 'main-content';
  isActive?: boolean;
  text: string;
  onClick: () => void;
}

function NavItem({
  className,
  hashId,
  isActive = false,
  text,
  onClick,
}: NavItemProps) {
  return (
    <li className={className}>
      <a
        className={classNames(
          'govuk-link--no-visited-state govuk-link--no-underline',
          {
            'govuk-!-font-weight-bold': isActive,
          },
        )}
        href={`#${hashId}`}
        onClick={onClick}
      >
        {text}
      </a>
    </li>
  );
}
