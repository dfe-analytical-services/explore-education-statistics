import useRendered from '@common/hooks/useRendered';
import isComponentType from '@common/lib/type-guards/components/isComponentType';
import classNames from 'classnames';
import React, {
  cloneElement,
  HTMLAttributes,
  ReactElement,
  ReactNode,
  RefAttributes,
  useState,
} from 'react';
import SideTabsSection, {
  SideTabsSectionProps,
} from 'src/components/SideTabsSection';
import styles from './SideTabs.module.scss';

function filterSections(
  children: ReactNode,
): ReactElement<SideTabsSectionProps>[] {
  if (Array.isArray(children)) {
    return children.filter(child =>
      isComponentType(child, SideTabsSection),
    ) as ReactElement<SideTabsSectionProps>[];
  }

  if (isComponentType(children, SideTabsSection)) {
    return [children];
  }

  return [];
}

interface Props {
  children: ReactNode;
}

const SideTabs = ({ children }: Props) => {
  const [loadedSections, setLoadedSections] = useState(new Set<number>());
  const [selectedTabIndex, setSelectedTabIndex] = useState(0);

  const { onRendered } = useRendered();

  const tabElements: HTMLAnchorElement[] = [];
  const sectionElements: HTMLElement[] = [];

  const setSelectedTab = (index: number) => {
    setLoadedSections(loadedSections.add(index));
    setSelectedTabIndex(index);
  };

  const sections = filterSections(children);

  return (
    <div className={styles.container}>
      <ul className={styles.tabList} role="tablist">
        {sections.map(({ props }, index) => (
          <li className={styles.tabItem} key={props.id} role="presentation">
            <a
              aria-controls={onRendered(props.id)}
              aria-selected={onRendered(selectedTabIndex === index)}
              className={classNames(styles.tabLink, {
                [styles.tabLinkSelected]: selectedTabIndex === index,
              })}
              href={`#${props.id}`}
              id={`${props.id}-tab`}
              ref={(element: HTMLAnchorElement) => tabElements.push(element)}
              role={onRendered('tab')}
              onClick={() => setSelectedTab(index)}
              onKeyDown={event => {
                switch (event.key) {
                  case 'ArrowUp': {
                    const nextTabIndex =
                      selectedTabIndex > 0
                        ? selectedTabIndex - 1
                        : sections.length - 1;

                    setSelectedTab(nextTabIndex);
                    tabElements[nextTabIndex].focus();

                    break;
                  }
                  case 'ArrowDown': {
                    const nextTabIndex =
                      selectedTabIndex < sections.length - 1
                        ? selectedTabIndex + 1
                        : 0;

                    setSelectedTab(nextTabIndex);
                    tabElements[nextTabIndex].focus();

                    break;
                  }
                  case 'ArrowRight':
                    sectionElements[selectedTabIndex].focus();
                    break;
                }
              }}
              tabIndex={selectedTabIndex !== index ? -1 : undefined}
            >
              {props.title}
            </a>
          </li>
        ))}
      </ul>

      {sections.map((section, index) => {
        const { id, lazy } = section.props;

        return cloneElement<
          SideTabsSectionProps &
            HTMLAttributes<HTMLElement> &
            RefAttributes<HTMLElement>
        >(
          section,
          {
            'aria-labelledby': `${id}-tab`,
            hidden: selectedTabIndex !== index,
            key: id,
            ref: (element: HTMLElement) => sectionElements.push(element),
          },
          lazy && !loadedSections.has(index) ? null : section.props.children,
        );
      })}
    </div>
  );
};

export default SideTabs;
