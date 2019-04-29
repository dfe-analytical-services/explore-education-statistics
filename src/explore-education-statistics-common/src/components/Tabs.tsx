import { useDesktopMedia } from '@common/hooks/useMedia';
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
import TabsSection, { TabsSectionProps } from './TabsSection';

function filterSections(children: ReactNode): ReactElement<TabsSectionProps>[] {
  if (Array.isArray(children)) {
    return children.filter(child =>
      isComponentType(child, TabsSection),
    ) as ReactElement<TabsSectionProps>[];
  }

  if (isComponentType(children, TabsSection)) {
    return [children];
  }

  return [];
}

interface Props {
  children: ReactNode;
}

const Tabs = ({ children }: Props) => {
  const [loadedSections, setLoadedSections] = useState(new Set<number>());
  const [selectedTabIndex, setSelectedTabIndex] = useState(0);

  const { onMedia } = useDesktopMedia();

  const tabElements: HTMLAnchorElement[] = [];
  const sectionElements: HTMLElement[] = [];

  const setSelectedTab = (index: number) => {
    window.location.hash = sectionElements[index].id;

    setLoadedSections(loadedSections.add(index));
    setSelectedTabIndex(index);
  };

  const sections = filterSections(children);

  return (
    <div className="govuk-tabs">
      <ul className="govuk-tabs__list" role="tablist">
        {sections.map(({ props }, index) => (
          <li
            className="govuk-tabs__list-item"
            key={props.id}
            role="presentation"
          >
            <a
              aria-controls={onMedia(props.id)}
              aria-selected={onMedia(selectedTabIndex === index)}
              role={onMedia('tab')}
              className={classNames('govuk-tabs__tab', {
                'govuk-tabs__tab--selected': selectedTabIndex === index,
              })}
              href={`#${props.id}`}
              id={`${props.id}-tab`}
              ref={(element: HTMLAnchorElement) => tabElements.push(element)}
              tabIndex={selectedTabIndex !== index ? -1 : undefined}
              onClick={event => {
                event.preventDefault();
                setSelectedTab(index);
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

                    break;
                  }
                  case 'ArrowRight': {
                    const nextTabIndex =
                      selectedTabIndex < sections.length - 1
                        ? selectedTabIndex + 1
                        : 0;

                    setSelectedTab(nextTabIndex);
                    tabElements[nextTabIndex].focus();

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
        ))}
      </ul>

      {sections.map((section, index) => {
        const { id, lazy } = section.props;

        return cloneElement<
          TabsSectionProps &
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

export default Tabs;
