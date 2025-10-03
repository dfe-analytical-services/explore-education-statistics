import styles from '@common/components/PageNavExpandable.module.scss';
import React, { useEffect } from 'react';
import classNames from 'classnames';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';

interface Props {
  activeSection?: string;
  heading?: string;
  items: NavItem[];
  onChangeSection?: (id: string) => void;
  onClickItem?: (id: string) => void;
}

export default function PageNavExpandable({
  activeSection,
  heading = 'On this page',
  items,
  onChangeSection,
  onClickItem,
}: Props) {
  const [handleScroll] = useDebouncedCallback(() => {
    if (!onChangeSection) {
      return;
    }

    const sections = document.querySelectorAll('[data-page-section]');

    // Set a section as active when it's in the top third of the page.
    const buffer = window.innerHeight / 3;
    const scrollPosition = window.scrollY + buffer;

    sections.forEach(section => {
      if (!section || section.id === activeSection) {
        return;
      }

      const { height } = section.getBoundingClientRect();
      const { offsetTop } = section as HTMLElement;
      const offsetBottom = offsetTop + height;

      const pageSectionId = section.id;

      if (scrollPosition > offsetTop && scrollPosition < offsetBottom) {
        onChangeSection(pageSectionId);
      }
    });
  }, 10);

  useEffect(() => {
    window.addEventListener('scroll', handleScroll);

    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, [handleScroll]);

  return (
    <>
      <h2 className="govuk-body-m" id="nav-label">
        {heading}
      </h2>
      <nav aria-labelledby="nav-label" role="navigation">
        <ul className={styles.navSection}>
          {items.map(item => (
            <NavItem
              key={item.id}
              id={item.id}
              isActive={activeSection === item.id}
              text={item.text}
              subNavItems={item.subNavItems}
              onClick={onClickItem}
            />
          ))}
          <NavItem
            className={items.length ? 'govuk-!-margin-top-8' : undefined}
            id="main-content"
            text="Back to top"
            onClick={() => onClickItem?.(items[0].id)}
          />
        </ul>
      </nav>
    </>
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
    <li
      className={classNames(
        styles.navSectionItem,
        isActive && styles.navSectionItemCurrent,
        className,
      )}
    >
      <a
        className={classNames(
          styles.navLink,
          'govuk-link--no-underline govuk-link--no-visited-state',
          {
            'govuk-!-font-weight-bold': isActive,
          },
        )}
        href={`#${id}`}
        onClick={() => onClick?.(id)}
        aria-current={isActive ? 'true' : undefined}
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
                onClick={() => onClick?.(id)}
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
