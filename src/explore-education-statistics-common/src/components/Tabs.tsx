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
  openId?: string;
  testId?: string;
}

const Tabs = ({ children, id, modifyHash = true, testId, onToggle }: Props) => {
  const [selectedTabIndex, setSelectedTabIndex] = useState(0);

  const { onMedia } = useDesktopMedia();

  const tabElements = useRef<HTMLAnchorElement[]>([]);
  const sectionElements = useRef<HTMLElement[]>([]);

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
        ref: (element: HTMLElement) => sectionElements.current.push(element),
      },
      section.props.children,
    );
  });

  const selectTab = useCallback(
    (index: number) => {
      if (sectionElements.current[index] && modifyHash) {
        if (window.history.pushState) {
          window.history.pushState(
            null,
            '',
            `#${sectionElements.current[index].id}`,
          );
        } else {
          window.location.hash = sectionElements.current[index].id;
        }
      }

      setSelectedTabIndex(index);
    },
    [modifyHash],
  );

  useEffect(() => {
    if (selectedTabIndex >= sections.length) {
      setSelectedTabIndex(sections.length - 1);
    }
  }, [sections, selectedTabIndex, selectTab]);

  useEffect(() => {
    if (window) {
      sections.forEach((element, index) => {
        if (element.props.id === window.location.hash.substr(1)) {
          selectTab(index);
        }
      });
    }
  }, [sections, selectTab]);

  return (
    <div
      className={classNames('govuk-tabs', styles.tabs)}
      id={id}
      data-testid={testId}
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
                ref={(element: HTMLAnchorElement) =>
                  tabElements.current.push(element)
                }
                tabIndex={selectedTabIndex !== index ? -1 : undefined}
                onClick={event => {
                  event.preventDefault();
                  selectTab(index);
                  if (typeof onToggle === 'function') {
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
                      tabElements.current[nextTabIndex].focus();
                      if (typeof onToggle === 'function') {
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
                      tabElements.current[nextTabIndex].focus();
                      if (typeof onToggle === 'function') {
                        onToggle({
                          title: `${props.title}`,
                          id: `${sectionId}`,
                        });
                      }
                      break;
                    }
                    case 'ArrowDown':
                      sectionElements.current[selectedTabIndex].focus();
                      break;
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
