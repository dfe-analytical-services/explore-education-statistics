import styles from '@common/components/PageNavExpandable.module.scss';
import React, { useCallback, useEffect, useState } from 'react';
import classNames from 'classnames';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useToggle from '@common/hooks/useToggle';

interface Props {
  heading?: string;
  items: NavItem[];
}

export default function PageNavExpandable({
  heading = 'On this page',
  items,
}: Props) {
  const [activeSection, setActiveSection] = useState(items[0].id);
  const [isScrollHandlingBlocked, toggleScrollHandlingBlocked] =
    useToggle(false);

  const timeoutRef = React.useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    setActiveSection(items[0].id);
  }, [items]);

  const setActiveSectionIfValid = (sectionId: string) => {
    if (items.some(item => item.id === sectionId)) {
      setActiveSection(sectionId);
    }
  };

  const handleNavItemClick = (id: string) => {
    startBlocking();
    setActiveSection(id);
  };

  // Set temporary state to 'block' scroll handling when nav item is clicked
  const startBlocking = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    toggleScrollHandlingBlocked.on();

    timeoutRef.current = setTimeout(
      () => toggleScrollHandlingBlocked.off(),
      200,
    );
  }, [toggleScrollHandlingBlocked]);

  useEffect(() => {
    return () => {
      // When the component unmounts, clear any active timer
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  const [handleScroll] = useDebouncedCallback(() => {
    // If the user has just clicked a nav item, don't auto-update the active section
    // when the window scrolls to the target
    if (isScrollHandlingBlocked) {
      return;
    }

    const sections = document.querySelectorAll('[data-page-section]');

    // Set a section as active when it's in the top third of the page.
    const buffer = window.innerHeight / 4;
    const scrollPosition = window.scrollY + buffer;

    sections.forEach(section => {
      if (section.id === activeSection) {
        return;
      }

      const { height } = section.getBoundingClientRect();
      const { offsetTop } = section as HTMLElement;
      const offsetBottom = offsetTop + height;

      const pageSectionId = section.id;

      if (scrollPosition > offsetTop && scrollPosition < offsetBottom) {
        setActiveSectionIfValid(pageSectionId);
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
              onClick={() => handleNavItemClick(item.id)}
            />
          ))}
          <NavItem
            className={items.length ? 'govuk-!-margin-top-8' : undefined}
            id="top"
            text="Back to top"
            onClick={() => handleNavItemClick(items[0].id)}
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
