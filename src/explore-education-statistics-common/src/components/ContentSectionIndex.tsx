import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useMounted from '@common/hooks/useMounted';
import React, {
  CSSProperties,
  RefObject,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react';
import Details from '@common/components/Details';

const TOP_MARGIN = 20;
const BOTTOM_MARGIN = 40;

interface Props {
  contentRef: RefObject<HTMLElement>;
  id: string;
  selector?: string;
  sticky?: boolean;
  visible?: boolean;
}

type ViewportPosition = 'before' | 'within' | 'after';

const ContentSectionIndex = ({
  contentRef,
  id,
  selector = 'h2, h3, h4, h5',
  sticky,
  visible = true,
}: Props) => {
  const outerRef = useRef<HTMLDivElement>(null);
  const ref = useRef<HTMLDivElement>(null);

  const [elements, setElements] = useState<HTMLElement[][]>([]);
  const [viewportPosition, setViewportPosition] = useState<ViewportPosition>();
  const [initialBounds, setInitialBounds] = useState<DOMRect>();

  const { isMounted } = useMounted(() => {
    if (contentRef.current) {
      const nextElements = Array.from(
        contentRef.current.querySelectorAll(selector),
      ) as HTMLElement[];

      if (!id) {
        throw new Error('Prop `id` must not be empty');
      }

      nextElements.forEach((element, index) => {
        if (!element.id) {
          // eslint-disable-next-line no-param-reassign
          element.id = `${id}-${index + 1}`;
        }
      });

      const headingCollection: HTMLElement[][] = [];
      let headingNumberSection: HTMLElement[] = [];
      nextElements.forEach((value, index, array) => {
        if (value.tagName === 'H3') {
          headingCollection.push(headingNumberSection);
          headingNumberSection = [];
        }
        headingNumberSection.push(value);
      });
      setElements(headingCollection);
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

  if (!isMounted || !elements.length || !visible) {
    return null;
  }

  return (
    <div ref={outerRef} className="dfe-print-hidden">
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
            {elements.map((element, index) => {
              if (element.length > 1) {
                return (
                  // eslint-disable-next-line react/no-array-index-key
                  <Details key={index} summary={element[0].innerText}>
                    {element.map((value, subIndex, array) => (
                      // eslint-disable-next-line react/no-array-index-key
                      <li key={subIndex}>
                        <a href={`#${value.id}`}>{value.textContent}</a>
                      </li>
                    ))}
                  </Details>
                  // // eslint-disable-next-line react/no-array-index-key
                );
              }
              if (element.length === 1) {
                return (
                  // eslint-disable-next-line react/no-array-index-key
                  <li key={index}>
                    <a href={`#${element[0].id}`}>{element[0].textContent}</a>
                  </li>
                );
              }
              return null;
            })}
          </ul>
        </div>
      </div>
    </div>
  );
};
export default ContentSectionIndex;
