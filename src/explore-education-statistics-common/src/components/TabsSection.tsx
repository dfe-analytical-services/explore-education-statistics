import { useDesktopMedia } from '@common/hooks/useMedia';
import findAllParents from '@common/utils/dom/findAllParents';
import classNames from 'classnames';
import React, {
  createElement,
  forwardRef,
  HTMLAttributes,
  ReactNode,
  useEffect,
  useState,
} from 'react';
import styles from './TabsSection.module.scss';

export const classes = {
  panel: 'govuk-tabs__panel',
};

export interface TabsSectionProps {
  children: ReactNode;
  hasSiblings?: boolean;
  id?: string;
  /**
   * Set to true if children should not be
   * rendered until tab has been selected.
   */
  lazy?: boolean;
  headingTitle?: string;
  headingTag?: 'h2' | 'h3' | 'h4';
  tabLabel?: ReactNode;
  title: string;
}

const TabsSection = forwardRef<HTMLElement, TabsSectionProps>(
  (
    {
      children,
      id,
      headingTitle = '',
      headingTag = 'h3',
      hasSiblings,
      lazy,
      ...restProps
    }: TabsSectionProps,
    ref,
  ) => {
    // Hide additional props from the component's public API to
    // avoid any confusion over this component's usage as
    // it should only be used in combination with Tabs.
    const { hidden, ...tabProps } = restProps as HTMLAttributes<HTMLElement>;

    const { onMedia } = useDesktopMedia();
    const [hasRendered, setRendered] = useState(!lazy);

    useEffect(() => {
      if (lazy && !hasRendered && !hidden) {
        setRendered(true);
      }
    }, [hasRendered, hidden, lazy]);

    return hasRendered ? (
      <section
        aria-labelledby={
          hasSiblings ? onMedia(tabProps['aria-labelledby']) : undefined
        }
        hidden={hidden}
        className={classNames(classes.panel, styles.panel, {
          [styles.panelHidden]: hidden,
        })}
        id={id}
        ref={ref}
        role={hasSiblings ? onMedia('tabpanel') : undefined}
        tabIndex={hasSiblings ? onMedia(-1) : undefined}
      >
        {headingTitle && createElement(headingTag, null, headingTitle)}
        {children}
      </section>
    ) : null;
  },
);

TabsSection.displayName = 'TabsSection';

export default TabsSection;

export const openAllParentTabSections = (target: HTMLElement) => {
  const panels = findAllParents(target, `.${styles.panel}`);

  panels.forEach(panel => {
    const tabId = panel.getAttribute('aria-labelledby');

    if (tabId) {
      const tab = document.getElementById(tabId);

      if (tab) {
        tab.click();
      }
    }
  });
};
