import generateContentList from '@common/components/util/generateContentList';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useMounted from '@common/hooks/useMounted';
import classNames from 'classnames';
import React, {
  CSSProperties,
  RefObject,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react';

const TOP_MARGIN = 20;
const BOTTOM_MARGIN = 40;

interface ListItem {
  id: string;
  tagName: string;
  textContent: string;
}

export interface ParentListItem extends ListItem {
  children: ListItem[];
}

type ViewportPosition = 'before' | 'within' | 'after';

interface Props {
  className?: string;
  contentRef: RefObject<HTMLElement>;
  id: string;
  selector?: string;
  sticky?: boolean;
  visible?: boolean;
  onMount?: (hasIndex: boolean) => void;
}

const ContentSectionIndex = ({
  className,
  contentRef,
  id,
  selector = 'h3, h4',
  sticky,
  visible = true,
  onMount,
}: Props) => {
  const outerRef = useRef<HTMLDivElement>(null);
  const ref = useRef<HTMLDivElement>(null);
  const [headingsList, setHeadingsList] = useState<ParentListItem[]>([]);
  const [viewportPosition, setViewportPosition] = useState<ViewportPosition>();
  const [initialBounds, setInitialBounds] = useState<DOMRect>();

  const { isMounted } = useMounted(() => {
    if (contentRef.current) {
      const headingElements = Array.from(
        contentRef.current.querySelectorAll(selector),
      ) as HTMLElement[];

      if (!id) {
        throw new Error('Prop `id` must not be empty');
      }

      headingElements.forEach((element, index) => {
        if (!element.id) {
          // eslint-disable-next-line no-param-reassign
          element.id = `${id}-${index + 1}`;
        }
      });

      onMount?.(headingElements.length > 0);
      setHeadingsList(generateContentList(headingElements));
    }
  });

  const [handleScroll] = useDebouncedCallback(() => {
    if (!ref.current || !contentRef.current) {
      return;
    }

    const { height: indexHeight } = ref.current.getBoundingClientRect();
    const { top, bottom } = contentRef.current.getBoundingClientRect();

    const isBefore = top > TOP_MARGIN;
    const isAfter = bottom < indexHeight + TOP_MARGIN + BOTTOM_MARGIN;
    const isWithin = !isBefore && !isAfter;

    let nextViewportPosition: ViewportPosition = 'before';

    if (isWithin) {
      nextViewportPosition = 'within';
    } else if (isAfter) {
      nextViewportPosition = 'after';
    }

    if (nextViewportPosition !== viewportPosition) {
      setViewportPosition(nextViewportPosition);
    }
  }, 10);

  useEffect(() => {
    if (sticky && visible) {
      handleScroll();
      window.addEventListener('scroll', handleScroll);
    }

    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, [handleScroll, visible, sticky]);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useLayoutEffect(() => {
    const handleResize = () => {
      // Need to explicitly set heights/widths using the
      // height/width calculated by the outer-most div.
      if (outerRef.current) {
        setInitialBounds(outerRef.current.getBoundingClientRect());
      }
    };

    if (!initialBounds) {
      handleResize();
    }

    if (visible) {
      window.addEventListener('resize', handleResize);
    }

    return () => {
      window.removeEventListener('resize', handleResize);
    };
  });

  const { height, width } = initialBounds ?? {};

  const getContainerStyle = (): CSSProperties => {
    switch (viewportPosition) {
      case 'after':
        return {
          height: contentRef.current?.getBoundingClientRect()?.height,
        };
      default:
        return {};
    }
  };

  const getStyle = (): CSSProperties => {
    switch (viewportPosition) {
      case 'within':
        return {
          position: 'fixed',
          top: TOP_MARGIN,
        };
      case 'after':
        return {
          position: 'absolute',
          bottom: BOTTOM_MARGIN,
        };
      default:
        return {};
    }
  };

  if (!isMounted || !headingsList.length || !visible) {
    return null;
  }
  return (
    <div ref={outerRef} className={classNames('dfe-print-hidden', className)}>
      <div
        style={{
          position: 'relative',
          height,
          width,
          ...getContainerStyle(),
        }}
      >
        <div
          ref={ref}
          style={{
            height,
            width,
            ...getStyle(),
          }}
        >
          <h3 className="govuk-heading-s">In this section</h3>

          <ul className="govuk-body-s">
            {headingsList.map(listItem => (
              <li key={listItem.id}>
                <a href={`#${listItem.id}`}>{listItem.textContent.trim()}</a>
                {listItem.children.length > 0 && (
                  <ul>
                    {listItem.children.map(childListItem => (
                      <li key={childListItem.id}>
                        <a href={`#${childListItem.id}`}>
                          {childListItem.textContent.trim()}
                        </a>
                      </li>
                    ))}
                  </ul>
                )}
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
};
export default ContentSectionIndex;
