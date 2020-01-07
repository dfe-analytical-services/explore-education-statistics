import { useDesktopMedia } from '@common/hooks/useMedia';
import findAllParents from '@common/lib/dom/findAllParents';
import classNames from 'classnames';
import React, {
  createElement,
  forwardRef,
  HTMLAttributes,
  ReactNode,
} from 'react';
import styles from './TabsSection.module.scss';

export const classes = {
  panel: 'govuk-tabs__panel',
  panelHidden: 'govuk-visually-hidden',
};

export interface TabsSectionProps {
  children: ReactNode;
  id?: string;
  datablockId?: string;
  /**
   * Set to true if children should not be
   * rendered until tab has been selected.
   */
  lazy?: boolean;
  title: string;
  headingTitle?: string;
  headingTag?: 'h2' | 'h3' | 'h4';
}

const TabsSection = forwardRef<HTMLElement, TabsSectionProps>(
  (
    {
      children,
      id,
      datablockId,
      headingTitle = '',
      title,
      headingTag = 'h3',
      ...restProps
    }: TabsSectionProps,
    ref,
  ) => {
    const { onMedia } = useDesktopMedia();

    // Hide additional props from the component's public API to
    // avoid any confusion over this component's usage as
    // it should only be used in combination with Tabs.
    const tabProps = restProps as HTMLAttributes<HTMLElement>;

    return (
      <section
        aria-labelledby={onMedia(tabProps['aria-labelledby'])}
        className={classNames(
          classes.panel,
          'dfe-content-overflow',
          styles.panel,
          {
            [classes.panelHidden]: tabProps.hidden,
          },
        )}
        id={id}
        ref={ref}
        role={onMedia('tabpanel')}
        tabIndex={onMedia(-1)}
      >
        {headingTitle && createElement(headingTag, { children: headingTitle })}
        {title === 'Charts' && (
          <a href={`#${datablockId}-tables`} aria-live="assertive">
            If you are using a keyboard select this link for a accessible table
            view
          </a>
        )}
        {children}
      </section>
    );
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
