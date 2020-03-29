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
import styles from './Accordion.module.scss';
import {
  accordionSectionClasses,
  AccordionSectionProps,
} from './AccordionSection';

export interface AccordionProps {
  children: ReactNode;
  id: string;
  openAll?: boolean;
  onToggleAll?: (open: boolean) => void;
  onToggle?: (accordionSection: { id: string; title: string }) => void;
}

export function generateIdList(count: number) {
  return new Array(count).fill('content-section-').map((id, n) => id + (n + 1));
}

const Accordion = ({
  children,
  id,
  openAll,
  onToggleAll,
  onToggle,
}: AccordionProps) => {
  const ref = useRef<HTMLDivElement>(null);

  const filteredChildren: ReactElement[] = useMemo(
    () => React.Children.toArray(children).filter(isValidElement),
    [children],
  );

  const [openSections, updateOpenSections] = useImmer<Dictionary<boolean>>(
    () => {
      return filteredChildren.reduce<Dictionary<boolean>>((acc, section) => {
        if (section.key) {
          acc[section.key] = openAll ?? section.props.open ?? false;
        }

        return acc;
      }, {});
    },
  );

  const isAllOpen = Object.values(openSections).every(isOpen => isOpen);

  const sections: ReactElement<AccordionSectionProps>[] = useMemo(
    () =>
      filteredChildren.map((section: ReactElement, index) => {
        const headingId =
          section.props.headingId ?? `${id}-${index + 1}-heading`;
        const contentId =
          section.props.contentId ?? `${id}-${index + 1}-content`;

        return cloneElement<AccordionSectionProps>(section, {
          headingId,
          contentId,
          open: openSections[section.key ?? ''] ?? false,
          onToggle(isOpen) {
            updateOpenSections(draft => {
              if (section.key) {
                draft[section.key] = isOpen;
              }
            });

            if (onToggle && isOpen) {
              onToggle({
                id: headingId,
                title: section.props.heading,
              });
            }

            if (section.props.onToggle) {
              section.props.onToggle(isOpen);
            }
          },
        });
      }),
    [filteredChildren, id, onToggle, openSections, updateOpenSections],
  );

  /**
   * Changing `openAll` prop toggles all sections.
   */
  useEffect(() => {
    updateOpenSections(draft => {
      sections.forEach(section => {
        if (section.key) {
          draft[section.key] = openAll ?? draft[section.key] ?? false;
        }
      });
    });
  }, [sections, updateOpenSections, openAll]);

  const { isMounted } = useMounted(() => {
    const goToHash = () => {
      if (!ref.current || !window.location.hash) {
        return;
      }

      let locationHashEl: HTMLElement | null = null;

      try {
        locationHashEl = ref.current.querySelector(window.location.hash);
      } catch (_) {
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

      const contentEl = sectionEl.querySelector(
        `.${accordionSectionClasses.sectionContent}`,
      );

      if (contentEl) {
        updateOpenSections(draft => {
          const matchingSection = sections.find(section => {
            const hashId = contentEl.id;

            return (
              hashId === section.props.contentId ||
              hashId === section.props.headingId
            );
          });

          if (matchingSection?.key) {
            draft[matchingSection.key] = true;
          }
        });

        setTimeout(
          () =>
            (locationHashEl as HTMLElement).scrollIntoView({
              block: 'start',
            }),
          100,
        );
      }
    };

    goToHash();
    window.addEventListener('hashchange', goToHash);

    return () => window.removeEventListener('hashchange', goToHash);
  });

  return (
    <div
      className={classNames('govuk-accordion', styles.accordionPrint)}
      id={id}
      ref={ref}
      role="none"
      data-module="govuk-accordion"
    >
      {isMounted && typeof openAll !== 'boolean' && (
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
            <span className="govuk-visually-hidden">sections</span>
          </button>
        </div>
      )}

      {sections}
    </div>
  );
};

export default Accordion;
