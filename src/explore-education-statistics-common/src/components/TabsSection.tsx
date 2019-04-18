import { useDesktopMedia } from '@common/hooks/useMedia';
import classNames from 'classnames';
import React, { forwardRef, HTMLAttributes, ReactNode } from 'react';
import styles from './TabsSection.module.scss';

export interface TabsSectionProps {
  children: ReactNode;
  id: string;
  /**
   * Set to true if children should not be
   * rendered until tab has been selected.
   */
  lazy?: boolean;
  title: string;
}

const TabsSection = forwardRef<HTMLElement, TabsSectionProps>(
  ({ children, id, ...restProps }, ref) => {
    const { onMedia } = useDesktopMedia();

    // Hide additional props from the component's public API to
    // avoid any confusion over this component's usage as
    // it should only be used in combination with Tabs.
    const tabProps = restProps as HTMLAttributes<HTMLElement>;

    return (
      <section
        aria-labelledby={onMedia(tabProps['aria-labelledby'])}
        className={classNames('govuk-tabs__panel', styles.panel, {
          'govuk-tabs__panel--hidden': tabProps.hidden,
        })}
        id={id}
        ref={ref}
        role={onMedia('tabpanel')}
        tabIndex={onMedia(-1)}
        data-testid={tabProps.title}
      >
        {children}
      </section>
    );
  },
);

TabsSection.displayName = 'TabsSection';

export default TabsSection;
