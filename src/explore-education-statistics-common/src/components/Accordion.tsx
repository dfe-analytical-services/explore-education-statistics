import styles from '@common/components/Accordion.module.scss';
import {
  accordionSectionClasses,
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import VisuallyHidden from '@common/components/VisuallyHidden';
import AccordionToggleButton from '@common/components/AccordionToggleButton';
import useMounted from '@common/hooks/useMounted';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, {
  cloneElement,
  isValidElement,
  ReactElement,
  ReactNode,
  useEffect,
  useMemo,
  useRef,
} from 'react';
import { useImmer } from 'use-immer';

export interface AccordionProps {
  children: ReactNode;
  className?: string;
  id: string;
  openAll?: boolean;
  /** When supplied, overrides `openAll` and section `open` defaults. */
  openSectionIds?: string[];
  /** Reports individual, bulk and hash-navigation expansion changes. */
  onOpenSectionIdsChange?: (ids: string[]) => void;
  showOpenAll?: boolean;
  testId?: string;
  toggleAllHiddenText?: string;
  onToggleAll?: (open: boolean) => void;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
}

const Accordion = ({
  children,
  className,
  id,
  openAll,
  openSectionIds,
  onOpenSectionIdsChange,
  showOpenAll = true,
  testId = 'accordion',
  toggleAllHiddenText,
  onToggleAll,
  onSectionOpen,
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

  const isSectionOpen = (sectionId: string) =>
    openSectionIds !== undefined
      ? openSectionIds.includes(sectionId)
      : (openSections[sectionId] ?? false);

  const changeOpenSections = (ids: string[]) => {
    if (openSectionIds === undefined) {
      updateOpenSections(draft => {
        sections.forEach(section => {
          draft[section.id] = ids.includes(section.id);
        });
      });
    }
    onOpenSectionIdsChange?.(ids);
  };

  // Hash listeners are installed once, but must use the latest controlled state.
  const expansionRef = useRef({ sections, isSectionOpen, changeOpenSections });
  expansionRef.current = { sections, isSectionOpen, changeOpenSections };

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

        const expansion = expansionRef.current;
        const matchingSection = expansion.sections.find(
          section => sectionEl.id === section.id,
        );
        if (matchingSection) {
          expansion.changeOpenSections(
            expansion.sections
              .filter(
                section =>
                  section.id === matchingSection.id ||
                  expansion.isSectionOpen(section.id),
              )
              .map(section => section.id),
          );
        }

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
    if (openSectionIds !== undefined) {
      return;
    }
    // Changing `openAll` prop toggles all sections.
    updateOpenSections(draft => {
      sections.forEach(section => {
        draft[section.id] =
          openAll || section.element.props.open || draft[section.id] || false;
      });
    });
  }, [sections, updateOpenSections, openAll, openSectionIds]);

  const handleToggle = (isOpen: boolean, sectionId: string) => {
    const matchingSection = sections.find(section => section.id === sectionId);

    if (!matchingSection) {
      return;
    }

    changeOpenSections(
      sections
        .filter(section =>
          section.id === sectionId ? isOpen : isSectionOpen(section.id),
        )
        .map(section => section.id),
    );

    if (onSectionOpen && isOpen) {
      onSectionOpen({
        id: sectionId,
        title: matchingSection.element.props.heading,
      });
    }

    if (matchingSection.element.props.onToggle) {
      matchingSection.element.props.onToggle(isOpen, sectionId);
    }
  };

  const isAllOpen = sections.every(section => isSectionOpen(section.id));

  return (
    <div
      className={classNames('govuk-accordion', styles.accordion, className)}
      id={id}
      ref={ref}
      data-testid={testId}
    >
      {isMounted && showOpenAll && (
        <div className="govuk-accordion__controls">
          <AccordionToggleButton
            expanded={isAllOpen}
            label={
              <>
                {`${isAllOpen ? 'Hide' : 'Show'} all `}
                {toggleAllHiddenText && (
                  <VisuallyHidden>{toggleAllHiddenText} </VisuallyHidden>
                )}
                sections
              </>
            }
            onClick={() => {
              changeOpenSections(
                isAllOpen ? [] : sections.map(section => section.id),
              );

              if (onToggleAll) {
                onToggleAll(!isAllOpen);
              }
            }}
          />
        </div>
      )}

      {sections.map(section =>
        cloneElement<AccordionSectionProps>(section.element, {
          id: section.id,
          open: isSectionOpen(section.id),
          onToggle: handleToggle,
        }),
      )}
    </div>
  );
};

export default Accordion;
