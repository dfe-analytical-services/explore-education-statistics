import styles from '@common/components/PageNav.module.scss';
import { useMobileMedia } from '@common/hooks/useMedia';
import React from 'react';
import classNames from 'classnames';

interface Props {
  activeSection?: string;
  heading?: string;
  items: NavItem[];
  widthClassName?:
    | 'govuk-grid-column-one-quarter'
    | 'govuk-grid-column-one-third';
  onClickItem?: (id: string) => void;
}

export default function PageNav({
  activeSection,
  heading = 'On this page',
  items,
  widthClassName = 'govuk-grid-column-one-quarter',
  onClickItem,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={`${widthClassName} ${styles.navContainer}`}>
      <h2 className="govuk-heading-s" id="nav-label">
        {heading}
      </h2>
      <nav aria-labelledby="nav-label" role="navigation">
        <ul className="govuk-list">
          {items.map(item => (
            <NavItem
              key={item.id}
              id={item.id}
              isActive={!isMobileMedia && activeSection === item.id}
              text={item.text}
              subNavItems={item.subNavItems}
              onClick={onClickItem}
            />
          ))}
          {isMobileMedia ? (
            <hr />
          ) : (
            <NavItem
              className="govuk-!-padding-top-4"
              id="top"
              text="Back to top"
              onClick={() => onClickItem?.(items[0].id)}
            />
          )}
        </ul>
      </nav>
    </div>
  );
}

export interface NavItem {
  className?: string;
  id: string;
  isActive?: boolean;
  subNavItems?: NavItem[];
  text: string;
  onClick?: (id: string) => void;
}

function NavItem({
  className,
  id,
  isActive = false,
  subNavItems,
  text,
  onClick,
}: NavItem) {
  return (
    <li className={classNames(styles.navItem, className)}>
      <a
        className={classNames(
          'govuk-link--no-visited-state govuk-link--no-underline',
          {
            'govuk-!-font-weight-bold': isActive,
          },
        )}
        href={`#${id}`}
        onClick={() => onClick?.(id)}
      >
        {text}
      </a>
      {subNavItems && (
        <ul className={`${styles.subNav} govuk-list`}>
          {subNavItems.map(item => (
            <li className={styles.subNavItem} key={item.id}>
              <a
                className={`${styles.subNavLink} govuk-link--no-visited-state govuk-link--no-underline`}
                href={`#${item.id}`}
              >
                {item.text}
              </a>
            </li>
          ))}
        </ul>
      )}
    </li>
  );
}
