import useMounted from '@common/hooks/useMounted';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, {
  cloneElement,
  isValidElement,
  ReactElement,
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';
import { useImmer } from 'use-immer';
import styles from './Accordion.module.scss';
import {
  accordionSectionClasses,
  AccordionSectionProps,
} from './AccordionSection';
import VisuallyHidden from './VisuallyHidden';

export interface AccordionProps {
  children: ReactNode;
  className?: string;
  id: string;
  openAll?: boolean;
  showOpenAll?: boolean;
  onToggleAll?: (open: boolean) => void;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
  toggleAllHiddenText?: string;
}

const Accordion = ({
  children,
  className,
  id,
  openAll,
  showOpenAll = true,
  onToggleAll,
  onSectionOpen,
  toggleAllHiddenText,
}: AccordionProps) => {
  const ref = useRef<HTMLDivElement>(null);

  const sections = useMemo(
    () =>
      React.Children.toArray(children)
        .filter<ReactElement<AccordionSectionProps>>(isValidElement)
        .map((element, index) => {
          return {
            id: element.props.id ?? `${id}-${index + 1}`,
            element,
          };
        }),
    [children, id],
  );

  const [openSections, updateOpenSections] = useImmer<Dictionary<boolean>>(() =>
    sections.reduce<Dictionary<boolean>>((acc, section) => {
      if (section.id) {
        acc[section.id] = openAll ?? section.element.props.open ?? false;
      }

      return acc;
    }, {}),
  );

  const { isMounted } = useMounted(() => {
    const goToAndOpenHash = async () => {
      setTimeout(() => {
        if (!ref.current || !window.location.hash) {
          return;
        }

        let locationHashEl: HTMLElement | null = null;

        try {
          locationHashEl = ref.current.querySelector(window.location.hash);
        } catch (e) {
          // eslint-disable-next-line no-console
          console.error(e);
          return;
        }

        if (!locationHashEl) {
          return;
        }

        const sectionEl = locationHashEl.closest(
          `.${accordionSectionClasses.section}`,
        );

        if (!sectionEl) {
          return;
        }

        updateOpenSections(draft => {
          const matchingSection = sections.find(
            section => sectionEl.id === section.id,
          );

          if (matchingSection) {
            draft[matchingSection.id] = true;
          }
        });

        setTimeout(() => {
          (locationHashEl as HTMLElement).scrollIntoView({
            block: 'start',
          });
        }, 100);
      }, 200);
    };

    goToAndOpenHash();
    window.addEventListener('hashchange', goToAndOpenHash);

    return () => window.removeEventListener('hashchange', goToAndOpenHash);
  });

  useEffect(() => {
    // Changing `openAll` prop toggles all sections.
    updateOpenSections(draft => {
      sections.forEach(section => {
        draft[section.id] =
          openAll || section.element.props.open || draft[section.id] || false;
      });
    });
  }, [sections, updateOpenSections, openAll]);

  const handleToggle = useCallback(
    (isOpen: boolean, sectionId: string) => {
      const matchingSection = sections.find(
        section => section.id === sectionId,
      );

      if (!matchingSection) {
        return;
      }

      updateOpenSections(draft => {
        draft[matchingSection.id] = isOpen;
      });

      if (onSectionOpen && isOpen) {
        onSectionOpen({
          id: sectionId,
          title: matchingSection.element.props.heading,
        });
      }

      if (matchingSection.element.props.onToggle) {
        matchingSection.element.props.onToggle(isOpen, sectionId);
      }
    },
    [onSectionOpen, sections, updateOpenSections],
  );

  const isAllOpen = Object.values(openSections).every(isOpen => isOpen);

  return (
    <div
      className={classNames('govuk-accordion', styles.accordion, className)}
      id={id}
      ref={ref}
      data-testid="accordion"
    >
      {isMounted && showOpenAll && (
        <div className="govuk-accordion__controls">
          <button
            aria-expanded={isAllOpen}
            type="button"
            className="govuk-accordion__open-all"
            onClick={() => {
              updateOpenSections(draft => {
                Object.keys(draft).forEach(key => {
                  draft[key] = !isAllOpen;
                });
              });

              if (onToggleAll) {
                onToggleAll(!isAllOpen);
              }
            }}
          >
            {isAllOpen ? 'Close all ' : 'Open all '}
            <VisuallyHidden>
              {!toggleAllHiddenText
                ? ' sections'
                : `${' '}${toggleAllHiddenText}`}
            </VisuallyHidden>
          </button>
        </div>
      )}

      {sections.map(section =>
        cloneElement<AccordionSectionProps>(section.element, {
          id: section.id,
          open: openSections[section.id] ?? false,
          onToggle: handleToggle,
        }),
      )}
    </div>
  );
};

export default Accordion;
