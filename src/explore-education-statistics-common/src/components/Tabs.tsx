import { useDesktopMedia } from '@common/hooks/useMedia';
import isComponentType from '@common/lib/type-guards/components/isComponentType';
import classNames from 'classnames';
import React, {
  cloneElement,
  HTMLAttributes,
  ReactComponentElement,
  ReactNode,
  RefAttributes,
  useState,
} from 'react';
import TabsSection, { TabsSectionProps } from './TabsSection';

interface Props {
  children: ReactNode;
  id: string;
  analytics?: (event: { category: string; action: string }) => void;
}

const Tabs = ({ children, id, analytics }: Props) => {
  const [loadedSections, setLoadedSections] = useState(new Set<number>());
  const [selectedTabIndex, setSelectedTabIndex] = useState(0);

  const { onMedia } = useDesktopMedia();

  const tabElements: HTMLAnchorElement[] = [];
  const sectionElements: HTMLElement[] = [];

  const setSelectedTab = (index: number) => {
    if (window.history.pushState) {
      window.history.pushState(null, '', `#${sectionElements[index].id}`);
    } else {
      window.location.hash = sectionElements[index].id;
    }

    setLoadedSections(loadedSections.add(index));
    setSelectedTabIndex(index);
  };

  const sections = React.Children.toArray(children).filter(child =>
    isComponentType(child, TabsSection),
  ) as ReactComponentElement<typeof TabsSection>[];

  return (
    <div className="govuk-tabs">
      <ul className="govuk-tabs__list" role="tablist">
        {sections.map(({ props }, index) => {
          const sectionId = props.id || `${id}-${index + 1}`;

          return (
            <li
              className="govuk-tabs__list-item"
              key={sectionId}
              role="presentation"
            >
              <a
                aria-controls={onMedia(sectionId)}
                aria-selected={onMedia(selectedTabIndex === index)}
                role={onMedia('tab')}
                className={classNames('govuk-tabs__tab', {
                  'govuk-tabs__tab--selected': selectedTabIndex === index,
                })}
                href={`#${sectionId}`}
                id={`${sectionId}-tab`}
                ref={(element: HTMLAnchorElement) => tabElements.push(element)}
                tabIndex={selectedTabIndex !== index ? -1 : undefined}
                onClick={event => {
                  event.preventDefault();
                  setSelectedTab(index);
                  if (typeof analytics === 'function') {
                    analytics({
                      category: 'Statistics Tabs',
                      action: `${props.title} (${sectionId}) Tab Opened`,
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

                      setSelectedTab(nextTabIndex);
                      tabElements[nextTabIndex].focus();
                      if (typeof analytics === 'function') {
                        analytics({
                          category: 'Statistics Tabs',
                          action: `${props.title} (${sectionId}) Tab Opened`,
                        });
                      }
                      break;
                    }
                    case 'ArrowRight': {
                      const nextTabIndex =
                        selectedTabIndex < sections.length - 1
                          ? selectedTabIndex + 1
                          : 0;

                      setSelectedTab(nextTabIndex);
                      tabElements[nextTabIndex].focus();
                      if (typeof analytics === 'function') {
                        analytics({
                          category: 'Statistics Tabs',
                          action: `${props.title} (${sectionId}) Tab Opened`,
                        });
                      }
                      break;
                    }
                    case 'ArrowDown':
                      sectionElements[selectedTabIndex].focus();
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

      {sections.map((section, index) => {
        const { lazy } = section.props;
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
            ref: (element: HTMLElement) => sectionElements.push(element),
          },
          lazy && !loadedSections.has(index) ? null : section.props.children,
        );
      })}
    </div>
  );
};

export default Tabs;
