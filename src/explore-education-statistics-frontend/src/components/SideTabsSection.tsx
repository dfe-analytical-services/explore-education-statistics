import classNames from 'classnames';
import React, {
  forwardRef,
  FunctionComponent,
  HTMLAttributes,
  ReactNode,
  Ref,
} from 'react';
import useRendered from 'src/hooks/useRendered';
import styles from './SideTabsSection.module.scss';

export interface SideTabsSectionProps {
  children: ReactNode;
  id: string;
  /**
   * Set to true if children should not be
   * rendered until tab has been selected.
   */
  lazy?: boolean;
  title: string;
}

const SideTabsSection: FunctionComponent<SideTabsSectionProps> = forwardRef(
  (
    { children, id, lazy = false, ...restProps }: SideTabsSectionProps,
    ref: Ref<HTMLElement>,
  ) => {
    const { onRendered } = useRendered();

    // Hide additional props from the component's public API to
    // avoid any confusion over this component's usage as
    // it should only be used in combination with Tabs.
    const tabProps = restProps as HTMLAttributes<HTMLElement>;

    return (
      <section
        aria-labelledby={onRendered(tabProps['aria-labelledby'])}
        className={classNames(styles.panel, {
          [styles.panelHidden]: onRendered(tabProps.hidden),
        })}
        hidden={onRendered(tabProps.hidden)}
        id={id}
        ref={ref}
        role={onRendered('tabpanel')}
        tabIndex={onRendered(-1)}
        data-testid={tabProps.title}
      >
        {children}
      </section>
    );
  },
);

SideTabsSection.displayName = 'SideTabsSection';

export default SideTabsSection;
