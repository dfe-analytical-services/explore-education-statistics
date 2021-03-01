import { useDesktopMedia } from '@common/hooks/useMedia';
import isComponentType from '@common/utils/type-guards/components/isComponentType';
import classNames from 'classnames';
import React, {
  cloneElement,
  HTMLAttributes,
  ReactElement,
  ReactNode,
  RefAttributes,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';
import styles from './Tabs.module.scss';
import TabsSection, { TabsSectionProps } from './TabsSection';

interface Props {
  children: ReactNode;
  modifyHash?: boolean;
  id: string;
  onToggle?: (section: { id: string; title: string }) => void;
  testId?: string;
}

const Tabs = ({ children, id, modifyHash = true, testId, onToggle }: Props) => {
  const [selectedTabIndex, setSelectedTabIndex] = useState(0);
  const ref = useRef<HTMLDivElement>(null);

  const { onMedia } = useDesktopMedia();

  const filteredChildren = React.Children.toArray(children).filter(child =>
    isComponentType(child, TabsSection),
  ) as ReactElement<TabsSectionProps>[];

  const sections = filteredChildren.map((section, index) => {
    const sectionId = section.props.id || `${id}-${index + 1}`;

    return cloneElement<
      TabsSectionProps &
        HTMLAttributes<HTMLElement> &
        RefAttributes<HTMLElement>
    >(
      section,
      {
        'aria-labelledby': `${sectionId}-tab`,
        hidden: selectedTabIndex !== index,
        id: sectionId,
        key: sectionId,
      },
      section.props.children,
    );
  });

  const selectTab = useCallback(
    (index: number) => {
      setSelectedTabIndex(index);

      const sectionId = sections[index]?.props?.id;

      if (!sectionId) {
        throw new Error(`Could not find tab section id for index ${index}`);
      }

      if (modifyHash) {
        if (window.history.pushState) {
          window.history.pushState(null, '', `#${sectionId}`);
        } else {
          window.location.hash = sectionId;
        }
      }
    },
    [modifyHash, sections],
  );

  const focusTab = useCallback(
    (index: number) => {
      const sectionId = sections[index]?.props?.id;

      if (!sectionId) {
        throw new Error(`Could not find tab section id for index ${index}`);
      }

      if (ref.current) {
        const tab = ref.current.querySelector<HTMLLinkElement>(
          `#${sectionId}-tab`,
        );

        if (tab) {
          tab.focus();
        }
      }
    },
    [sections],
  );

  useEffect(() => {
    const handleHashChange = () => {
      if (window.location.hash) {
        const matchingTabIndex = sections.findIndex(
          element => element.props.id === window.location.hash.substr(1),
        );

        if (matchingTabIndex > -1) {
          // Set index directly as we don't want to modify
          // the location hash again by using `selectTab`
          setSelectedTabIndex(matchingTabIndex);
        }
      }
    };

    if (window) {
      if (selectedTabIndex >= sections.length) {
        selectTab(sections.length - 1);
      }

      handleHashChange();
      window.addEventListener('hashchange', handleHashChange);
    }

    return () => {
      if (window) {
        window.removeEventListener('hashchange', handleHashChange);
      }
    };
  }, [sections, selectTab, selectedTabIndex]);

  return (
    <div
      className={classNames('govuk-tabs', styles.tabs)}
      id={id}
      data-testid={testId}
      ref={ref}
    >
      <ul className="govuk-tabs__list" role="tablist">
        {sections.map(({ props }, index) => {
          const sectionId = props.id || `${id}-${index + 1}`;
          return (
            <li
              className={classNames('govuk-tabs__list-item', {
                'govuk-tabs__list-item--selected': selectedTabIndex === index,
              })}
              key={sectionId}
              role="presentation"
            >
              <a
                aria-controls={onMedia(sectionId)}
                aria-selected={onMedia(selectedTabIndex === index)}
                role={onMedia('tab')}
                className="govuk-tabs__tab"
                href={`#${sectionId}`}
                id={`${sectionId}-tab`}
                tabIndex={selectedTabIndex !== index ? -1 : undefined}
                onClick={event => {
                  event.preventDefault();
                  selectTab(index);

                  if (onToggle) {
                    onToggle({
                      title: props.title,
                      id: sectionId,
                    });
                  }
                }}
                onKeyDown={event => {
                  switch (event.key) {
                    case 'ArrowLeft': {
                      const nextTabIndex =
                        selectedTabIndex > 0
                          ? selectedTabIndex - 1
                          : sections.length - 1;

                      selectTab(nextTabIndex);
                      focusTab(nextTabIndex);

                      if (onToggle) {
                        onToggle({
                          title: `${props.title}`,
                          id: `${sectionId}`,
                        });
                      }
                      break;
                    }
                    case 'ArrowRight': {
                      const nextTabIndex =
                        selectedTabIndex < sections.length - 1
                          ? selectedTabIndex + 1
                          : 0;

                      selectTab(nextTabIndex);
                      focusTab(nextTabIndex);

                      if (onToggle) {
                        onToggle({
                          title: `${props.title}`,
                          id: `${sectionId}`,
                        });
                      }
                      break;
                    }
                    case 'ArrowDown': {
                      if (ref.current) {
                        const section = ref.current.querySelector<
                          HTMLDivElement
                        >(`#${sections[selectedTabIndex].props.id}`);

                        if (section) {
                          section.focus();
                        }
                      }

                      break;
                    }
                    default:
                      break;
                  }
                }}
              >
                {props.title}
              </a>
            </li>
          );
        })}
      </ul>

      {sections}
    </div>
  );
};

export default Tabs;
