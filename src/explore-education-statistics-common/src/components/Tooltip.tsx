import useId from '@common/hooks/useId';
import useToggle from '@common/hooks/useToggle';
import {
  createPopper,
  Instance as Popper,
  Options,
  Placement,
  VirtualElement,
} from '@popperjs/core';
import classNames from 'classnames';
import React, {
  Dispatch,
  ReactNode,
  SetStateAction,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import styles from './Tooltip.module.scss';

type HtmlElementEvent = keyof HTMLElementEventMap;

const defaultShowEvents: HtmlElementEvent[] = ['mouseenter', 'focus'];
const defaultHideEvents: HtmlElementEvent[] = ['mouseleave', 'blur'];

export interface TooltipChildProps {
  ref: Dispatch<SetStateAction<HTMLElement | null>>;
  show: () => void;
  hide: () => void;
}

export interface TooltipProps {
  children: (props: TooltipChildProps) => ReactNode;
  className?: string;
  enabled?: boolean;
  /**
   * Tooltip will follow the mouse as
   * it hovers the reference element.
   */
  followMouse?: boolean;
  hideEvents?: HtmlElementEvent[];
  id?: string;
  placement?: Placement;
  showEvents?: HtmlElementEvent[];
  text: ReactNode;
  variant?: 'warning' | 'danger';
}

export default function Tooltip({
  children,
  className,
  enabled = true,
  followMouse,
  hideEvents = defaultHideEvents,
  id,
  placement = 'top',
  showEvents = defaultShowEvents,
  text,
  variant,
}: TooltipProps) {
  const tooltipId = useId({ prefix: 'tooltip', id });

  const [referenceEl, setReferenceEl] = useState<HTMLElement | null>(null);
  const [tooltipEl, setTooltipEl] = useState<HTMLElement | null>(null);
  const [arrowEl, setArrowEl] = useState<HTMLElement | null>(null);

  const virtualElRef = useRef<VirtualElement>({
    getBoundingClientRect: generateGetBoundingClientRect(),
  });

  const [visible, toggleVisible] = useToggle(false);

  const isEnabled = enabled && text;

  const options = useMemo<Options>(
    () => ({
      placement,
      strategy: 'absolute',
      modifiers: [
        {
          name: 'arrow',
          options: {
            element: arrowEl,
            padding: 10,
          },
        },
        {
          name: 'offset',
          options: {
            offset: [0, 10],
          },
        },
        {
          name: 'eventListeners',
          enabled: visible,
        },
      ],
    }),
    [arrowEl, placement, visible],
  );

  const popper = useRef<Popper | null>(null);

  useEffect(() => {
    if (!referenceEl || !tooltipEl) {
      return undefined;
    }

    const popperInstance = createPopper(
      followMouse ? virtualElRef.current : referenceEl,
      tooltipEl,
      options,
    );

    popper.current = popperInstance;

    return () => {
      popperInstance.destroy();
      popper.current = null;
    };
  }, [followMouse, options, referenceEl, tooltipEl]);

  const show = useCallback(() => {
    toggleVisible.on();
    popper.current?.update();
  }, [toggleVisible]);

  const hide = toggleVisible.off;

  useEffect(() => {
    if (!referenceEl || !isEnabled) {
      return undefined;
    }

    const describedBy = referenceEl.getAttribute('aria-describedby');

    // Add `aria-describedby` to the reference element,
    // linking it to the tooltip for screen readers.
    if (describedBy) {
      referenceEl.setAttribute(
        'aria-describedby',
        `${tooltipId} ${describedBy}`,
      );
    } else {
      referenceEl.setAttribute('aria-describedby', tooltipId);
    }

    showEvents?.forEach(event => {
      referenceEl.addEventListener(event, show);
    });

    hideEvents?.forEach(event => {
      referenceEl.addEventListener(event, hide);
    });

    return () => {
      showEvents?.forEach(event => {
        referenceEl.removeEventListener(event, show);
      });

      hideEvents?.forEach(event => {
        referenceEl.removeEventListener(event, hide);
      });
    };
  }, [hide, hideEvents, isEnabled, referenceEl, show, showEvents, tooltipId]);

  useEffect(() => {
    if (!referenceEl || !followMouse) {
      return undefined;
    }

    const handleMouseMove = ({ x, y }: MouseEvent) => {
      toggleVisible.on();

      virtualElRef.current.getBoundingClientRect = generateGetBoundingClientRect(
        x,
        y,
      );

      popper.current?.update();
    };

    referenceEl.addEventListener('mousemove', handleMouseMove);

    return () => {
      referenceEl.removeEventListener('mousemove', handleMouseMove);
    };
  }, [followMouse, referenceEl, toggleVisible]);

  return (
    <>
      {children({
        ref: setReferenceEl,
        show,
        hide,
      })}

      {isEnabled && (
        <div
          className={classNames(
            styles.tooltip,
            variant && styles[variant],
            {
              [styles.visible]: visible,
            },
            className,
          )}
          hidden={!visible}
          id={tooltipId}
          ref={setTooltipEl}
          role="tooltip"
        >
          <span>{text}</span>
          <span className={styles.arrow} ref={setArrowEl} />
        </div>
      )}
    </>
  );
}

function generateGetBoundingClientRect(x = 0, y = 0) {
  return (): DOMRect => {
    const rect = {
      width: 0,
      height: 0,
      top: y,
      right: x,
      bottom: y,
      left: x,
      x,
      y,
    };

    return {
      ...rect,
      toJSON() {
        return rect;
      },
    };
  };
}
