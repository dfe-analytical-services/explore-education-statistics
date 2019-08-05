import useMounted from '@common/hooks/useMounted';
import isComponentType from '@common/lib/type-guards/components/isComponentType';
import classNames from 'classnames';
import React, {
  cloneElement,
  ReactComponentElement,
  ReactNode,
  useEffect,
  useRef,
} from 'react';
import { useImmer } from 'use-immer';
import styles from './Accordion.module.scss';
import AccordionSection, {
  accordionSectionClasses,
  AccordionSectionProps,
} from './AccordionSection';

export interface AccordionProps {
  children: ReactNode;
  id: string;
  onToggleAll?: (open: boolean) => void;
  onToggle?: (accordionSection: { id: string; title: string }) => void;
}

const Accordion = ({ children, id, onToggleAll, onToggle }: AccordionProps) => {
  const ref = useRef<HTMLDivElement>(null);

  const [openSections, updateOpenSections] = useImmer<boolean[]>([]);

  const sections = React.Children.toArray(children).filter(child =>
    isComponentType(child, AccordionSection),
  ) as ReactComponentElement<typeof AccordionSection>[];

  const getSectionIds = (
    sectionProps: AccordionSectionProps,
    index: number,
  ) => {
    return {
      contentId: sectionProps.contentId || `${id}-${index + 1}-content`,
      headingId: sectionProps.headingId || `${id}-${index + 1}-heading`,
    };
  };

  useEffect(() => {
    updateOpenSections(() =>
      sections.map(section => {
        return section.props.open || false;
      }),
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [children, updateOpenSections]);

  const { isMounted } = useMounted(() => {
    const goToHash = () => {
      if (ref.current && window.location.hash) {
        let locationHashEl: HTMLElement | null = null;

        try {
          locationHashEl = ref.current.querySelector(window.location.hash);
        } catch (_) {
          return;
        }

        if (locationHashEl) {
          const sectionEl = locationHashEl.closest(
            `.${accordionSectionClasses.section}`,
          );

          if (sectionEl) {
            const contentEl = sectionEl.querySelector(
              `.${accordionSectionClasses.sectionContent}`,
            );

            if (contentEl) {
              updateOpenSections(draft => {
                const openIndex = sections.findIndex((section, index) => {
                  const { contentId, headingId } = getSectionIds(
                    section.props,
                    index,
                  );
                  const hashId = contentEl.id;
                  return hashId === contentId || hashId === headingId;
                });

                if (openIndex > -1) {
                  draft[openIndex] = true;
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
          }
        }
      }
    };

    goToHash();
    window.addEventListener('hashchange', goToHash);

    return () => window.removeEventListener('hashchange', goToHash);
  });

  const isAllOpen = openSections.every(isOpen => isOpen);

  return (
    <div
      className={classNames('govuk-accordion', styles.accordionPrint)}
      id={id}
      ref={ref}
      role="none"
    >
      {isMounted && (
        <div className="govuk-accordion__controls">
          <button
            aria-expanded={isAllOpen}
            type="button"
            className="govuk-accordion__open-all"
            onClick={() => {
              updateOpenSections(draft => {
                return draft.map(() => !isAllOpen);
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

      {sections.map((section, index) => {
        const { headingId, contentId } = getSectionIds(section.props, index);

        const isSectionOpen = isAllOpen || openSections[index];

        return cloneElement<AccordionSectionProps>(section, {
          key: headingId,
          headingId,
          contentId,
          open: isSectionOpen,
          onToggle(isOpen) {
            updateOpenSections(draft => {
              draft[index] = isOpen;
            });

            if (onToggle && isOpen) {
              onToggle({ id: headingId, title: section.props.heading });
            }

            if (section.props.onToggle) {
              section.props.onToggle(isOpen);
            }
          },
        });
      })}
    </div>
  );
};

export default Accordion;
