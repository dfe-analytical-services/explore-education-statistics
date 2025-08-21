import { useMobileMedia } from '@common/hooks/useMedia';
import Link from '@frontend/components/Link';
import styles from '@frontend/components/SubNav.module.scss';
import React from 'react';
import classNames from 'classnames';

interface Props {
  heading?: string;
  headingVisible?: boolean;
  items: NavItem[];
  widthClassName?:
    | 'govuk-grid-column-one-quarter'
    | 'govuk-grid-column-one-third';
}

export default function SubNav({
  heading = 'In this section',
  headingVisible = true,
  items,
  widthClassName = 'govuk-grid-column-one-quarter',
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={`${widthClassName} ${styles.navContainer}`}>
      <h2
        className={headingVisible ? 'govuk-heading-s' : 'govuk-visually-hidden'}
        id="nav-label"
      >
        {heading}
      </h2>
      <nav aria-labelledby="nav-label" role="navigation">
        <ul className={classNames(styles.navSection)}>
          {items.map(item => (
            <li
              className={classNames([
                styles.navSectionItem,
                item.isActive && styles.navSectionItemCurrent,
              ])}
              key={item.href}
            >
              <NavItem
                href={item.href}
                isActive={item.isActive}
                text={item.text}
              />
            </li>
          ))}
          {isMobileMedia && <hr />}
        </ul>
      </nav>
    </div>
  );
}

export interface NavItem {
  href: string;
  isActive?: boolean;
  text: string;
}

function NavItem({ href, isActive = false, text }: NavItem) {
  return (
    <Link
      className={classNames(styles.navLink, 'govuk-link--no-underline', {
        'govuk-!-font-weight-bold': isActive,
      })}
      to={href}
      unvisited
      aria-current={isActive && href ? 'page' : undefined}
    >
      {text}
    </Link>
  );
}
