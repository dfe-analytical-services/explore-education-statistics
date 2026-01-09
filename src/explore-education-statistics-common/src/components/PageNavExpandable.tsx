import styles from '@common/components/PageNavExpandable.module.scss';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import classNames from 'classnames';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useToggle from '@common/hooks/useToggle';

interface Props {
  heading?: string;
  items: NavItem[];
  onClickNavItem?: (title: string) => void;
}

export default function PageNavExpandable({
  heading = 'On this page',
  items,
  onClickNavItem,
}: Props) {
  const [activeSection, setActiveSection] = useState(items[0].id);
  const [activeSubSection, setActiveSubSection] = useState<string | null>(null);

  const [isScrollHandlingBlocked, toggleScrollHandlingBlocked] =
    useToggle(false);

  const timeoutRef = React.useRef<ReturnType<typeof setTimeout> | null>(null);

  // Calculate the parent map for lookups (subitem -> parent)
  // and a flattened array of all IDs
  const { parentMap, allIds } = useMemo(() => {
    const map: Map<string, string> = new Map();
    const ids: string[] = [];

    items.forEach(item => {
      ids.push(item.id);
      if (item.subNavItems) {
        item.subNavItems.forEach(sub => {
          map.set(sub.id, item.id);
          ids.push(sub.id);
        });
      }
    });

    return { parentMap: map, allIds: ids };
  }, [items]);

  useEffect(() => {
    setActiveSection(items[0].id);
    setActiveSubSection(null);
  }, [items]);

  // Update logic accepts either a parent or child ID
  const updateActiveSection = (id: string) => {
    const parentId = parentMap.get(id);

    if (parentId) {
      // It's a sub-item: Set active section and subsection
      setActiveSection(parentId);
      setActiveSubSection(id);
    } else {
      // It's a top-level item: Set active section and clear subsection
      setActiveSection(id);
      setActiveSubSection(null);
    }
  };

  const handleNavItemClick = (id: string) => {
    startBlocking();
    updateActiveSection(id);
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

    // We need to find out which of our potential nav item targets is closest to the
    // top of the page (accounting for a buffer space for the sticky nav)
    const buffer = window.innerHeight / 4;
    const windowScrollPosition = window.scrollY + buffer;

    let currentActiveId = items[0].id; // Default to first item

    // Map over allIds to find their DOM element targets
    allIds.forEach(id => {
      const element = document.getElementById(id);
      if (!element) return;

      const { offsetTop } = element;

      // If we have scrolled past this element's top, it is a candidate
      if (windowScrollPosition >= offsetTop) {
        currentActiveId = id;
      }
    });

    // Update state based on the last element past the scroll position
    // and avoid re-renders if nothing changed
    if (
      currentActiveId !== activeSubSection &&
      currentActiveId !== activeSection
    ) {
      updateActiveSection(currentActiveId);
    }
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

      <a
        href={`#${items[0].id}`}
        className="govuk-skip-link govuk-!-margin-bottom-4"
        onClick={() => handleNavItemClick(items[0].id)}
      >
        Skip in page navigation
      </a>

      <nav aria-labelledby="nav-label" role="navigation">
        <ul className={styles.navSection}>
          {items.map(item => (
            <NavItem
              key={item.id}
              activeSubSection={activeSubSection}
              id={item.id}
              isActive={activeSection === item.id}
              text={item.text}
              subNavItems={item.subNavItems}
              onClick={(id, title) => {
                handleNavItemClick(id);
                // onClickNavItem is used for GA event tracking
                onClickNavItem?.(title);
              }}
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
  activeSubSection?: string | null;
  className?: string;
  id: string;
  isActive?: boolean;
  subNavItems?: NavItem[];
  text: string;
  onClick?: (id: string, title: string) => void;
}

function NavItem({
  activeSubSection,
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
        onClick={() => onClick?.(id, text)}
        aria-current={isActive ? 'true' : undefined}
      >
        {text}
      </a>
      {subNavItems && (
        <ul className={`${styles.subNav} govuk-list`}>
          {subNavItems.map(item => (
            <li className={styles.subNavItem} key={item.id}>
              <a
                className={classNames(
                  styles.navLink,
                  'govuk-link--no-underline govuk-link--no-visited-state',
                  {
                    'govuk-!-font-weight-bold': activeSubSection === item.id,
                  },
                )}
                href={`#${item.id}`}
                onClick={() => onClick?.(item.id, `${text} - ${item.text}`)}
                aria-current={
                  activeSubSection === item.id ? 'location' : undefined
                }
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
